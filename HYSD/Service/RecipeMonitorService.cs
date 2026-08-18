using Serilog;
using SqlSugar;
using System;
using System.Threading;

namespace HYSD
{
    /// <summary>
    /// 配方运行监控服务实现（基于 SQLite）。
    /// 订阅 IReadDataService.DataUpdated 事件（该事件在 ReadDataService 的后台读取线程触发），
    /// 检测"涂层数据记录位"上升沿，将 TCData 写入当前指定的批次数据库。
    ///
    /// 设计要点：
    /// 1. 复用 ReadDataService 已有的后台线程，无需额外创建线程，避免 PLC 重复读取。
    /// 2. 通过 EdgeDetector 检测上升沿，确保每次配方运行只记录一条数据。
    /// 3. 数据库写入在后台线程执行，不阻塞 UI；使用 lock 保护 _batchDb 的并发访问。
    /// 4. 服务生命周期独立于页面——即使 HistoryData 画面未显示，数据仍会被记录。
    /// 5. 使用 SQLite + WAL 模式，支持并发读写（监控写入时，HistoryData 画面可同时读取）。
    /// </summary>
    public class RecipeMonitorService : IRecipeMonitorService
    {
        private readonly IReadDataService _readData;
        private readonly ILogger _logger;

        /// <summary>当前批次对应的 SQLite 数据库 SqlSugarClient</summary>
        private SqlSugarClient _batchDb;

        /// <summary>保护 _batchDb 并发访问的锁</summary>
        private readonly object _dbLock = new object();

        /// <summary>是否已订阅 DataUpdated 事件</summary>
        private volatile bool _subscribed;

        /// <summary>"涂层数据记录位"上升沿检测器</summary>
        private readonly EdgeDetector _recordEdge = new EdgeDetector();

        public bool IsMonitoring => _subscribed && _batchDb != null;

        public string CurrentDbPath { get; private set; }

        public string CurrentBatchName { get; private set; }

        public RecipeMonitorService(IReadDataService readData, ILogger logger)
        {
            _readData = readData ?? throw new ArgumentNullException(nameof(readData));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// 启动监控，将配方数据写入指定批次数据库。
        /// 若当前已在监控其他数据库，会先停止旧的再启动新的。
        /// </summary>
        public void Start(string dbPath, string batchName)
        {
            if (string.IsNullOrEmpty(dbPath))
                throw new ArgumentNullException(nameof(dbPath));

            // 若已在监控同一数据库，直接返回（避免重复启动）
            if (CurrentDbPath == dbPath && IsMonitoring)
            {
                _logger.Information("配方监控已在运行中，批次: {Batch}", batchName);
                return;
            }

            // 若在监控其他数据库，先停止
            if (IsMonitoring)
            {
                _logger.Information("切换批次监控：{OldBatch} -> {NewBatch}", CurrentBatchName, batchName);
                Stop();
            }

            lock (_dbLock)
            {
                // 创建针对该批次数据库的 SqlSugarClient
                _batchDb = BatchDbHelper.CreateClient(dbPath);
                CurrentDbPath = dbPath;
                CurrentBatchName = batchName;

                // 重置上升沿检测器，避免上次监控的残留状态影响
                _recordEdge.Reset();
            }

            // 订阅 DataUpdated 事件
            _readData.DataUpdated += OnDataUpdated;
            _subscribed = true;

            _logger.Information("配方监控已启动，批次: {Batch}, 数据库: {Path}", batchName, dbPath);
        }

        /// <summary>停止监控，释放数据库连接</summary>
        public void Stop()
        {
            if (!_subscribed) return;

            _readData.DataUpdated -= OnDataUpdated;
            _subscribed = false;

            lock (_dbLock)
            {
                try
                {
                    _batchDb?.Dispose();
                }
                catch (Exception ex)
                {
                    _logger.Debug(ex, "释放批次数据库连接时异常");
                }
                _batchDb = null;
            }

            _logger.Information("配方监控已停止，批次: {Batch}", CurrentBatchName);
            CurrentDbPath = null;
            CurrentBatchName = null;
        }

        /// <summary>DataUpdated 事件回调（后台读取线程触发）</summary>
        private void OnDataUpdated(object sender, EventArgs e)
        {
            // PLC 未连接时不处理
            if (!_readData.IsConnected) return;
            if (_readData != null && !_readData.isRunning) return;

            try
            {
                //// 读取"涂层数据记录位"信号
                //object recordFlagObj = _readData.TryGetValueW("涂层数据记录位");
                //if (recordFlagObj == null) return;

                //bool recordFlag = Convert.ToBoolean(recordFlagObj);

                // 检测上升沿：false -> true 的瞬间
                if (_recordEdge.DetectRisingEdge((bool)_readData.TryGetValueW("涂层数据记录位")))
                {
                    RecordRecipeData();
                }
            }
            catch (Exception ex)
            {
                _logger.Debug(ex, "配方监控数据处理异常");
            }
        }

        /// <summary>
        /// 采集当前所有工艺参数并写入批次数据库。
        /// 在后台线程执行，不阻塞 UI。
        /// </summary>
        private void RecordRecipeData()
        {
            SqlSugarClient db;
            string batchName;

            lock (_dbLock)
            {
                if (_batchDb == null) return;
                db = _batchDb;
                batchName = CurrentBatchName;
            }

            try
            {
                // 采集 TCData（复用 HistoryData.PlcDataEx 的字段映射逻辑）
                TCData ds = BuildTCData();

                // 写入批次数据库
                db.Insertable(ds).ExecuteCommand();

                _logger.Information(
                    "批次 [{Batch}] 记录配方数据，时间: {Time}",
                    batchName, ds.DateTime.ToString("yyyy-MM-dd HH:mm:ss"));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "批次 [{Batch}] 写入配方数据失败", batchName);
            }
        }

        /// <summary>
        /// 从 IReadDataService 读取当前所有工艺参数，构建 TCData 实体。
        /// 字段映射与 HistoryData.PlcDataEx 完全一致，确保数据口径统一。
        /// </summary>
        private TCData BuildTCData()
        {
            var ds = new TCData();
            ds.DateTime = DateTime.Now;
            ds.UpHeat = (float)Math.Round((float)_readData.TryGetValueD29("上温度"), 1, MidpointRounding.AwayFromZero);
            ds.DnHeat = (float)Math.Round((float)_readData.TryGetValueD29("下温度"), 1, MidpointRounding.AwayFromZero);
            ds.Rotation = (ushort)_readData.TryGetValueD29("转速");
            ds.N2SV = (ushort)_readData.TryGetValueD29("氮气实际流量");
            ds.H2SV = (ushort)_readData.TryGetValueD29("氢气实际流量");
            ds.ArSV = (ushort)_readData.TryGetValueD29("氩气实际流量");
            ds.WaterTemp = (float)Math.Round((float)_readData.TryGetValueD29("冰水机实际温度"), 1, MidpointRounding.AwayFromZero);
            ds.B1Curr = (float)Math.Round((float)_readData.TryGetValueD29("弧源1电流"), 1, MidpointRounding.AwayFromZero);
            ds.B1Volt = (float)Math.Round((float)_readData.TryGetValueD29("弧源1电压"), 1, MidpointRounding.AwayFromZero);
            ds.B2Curr = (float)Math.Round((float)_readData.TryGetValueD29("弧源2电流"), 1, MidpointRounding.AwayFromZero);
            ds.B2Volt = (float)Math.Round((float)_readData.TryGetValueD29("弧源2电压"), 1, MidpointRounding.AwayFromZero);
            ds.B3Curr = (float)Math.Round((float)_readData.TryGetValueD29("弧源3电流"), 1, MidpointRounding.AwayFromZero);
            ds.B3Volt = (float)Math.Round((float)_readData.TryGetValueD29("弧源3电压"), 1, MidpointRounding.AwayFromZero);
            ds.B4Curr = (float)Math.Round((float)_readData.TryGetValueD29("弧源4电流"), 1, MidpointRounding.AwayFromZero);
            ds.B4Volt = (float)Math.Round((float)_readData.TryGetValueD29("弧源4电压"), 1, MidpointRounding.AwayFromZero);
            ds.B5Curr = (float)Math.Round((float)_readData.TryGetValueD29("弧源5电流"), 1, MidpointRounding.AwayFromZero);
            ds.B5Volt = (float)Math.Round((float)_readData.TryGetValueD29("弧源5电压"), 1, MidpointRounding.AwayFromZero);
            ds.B6Curr = (float)Math.Round((float)_readData.TryGetValueD29("弧源6电流"), 1, MidpointRounding.AwayFromZero);
            ds.B6Volt = (float)Math.Round((float)_readData.TryGetValueD29("弧源6电压"), 1, MidpointRounding.AwayFromZero);
            ds.B7Curr = (float)Math.Round((float)_readData.TryGetValueD29("弧源7电流"), 1, MidpointRounding.AwayFromZero);
            ds.B7Volt = (float)Math.Round((float)_readData.TryGetValueD29("弧源7电压"), 1, MidpointRounding.AwayFromZero);
            ds.B8Curr = (float)Math.Round((float)_readData.TryGetValueD29("弧源8电流"), 1, MidpointRounding.AwayFromZero);
            ds.B8Volt = (float)Math.Round((float)_readData.TryGetValueD29("弧源8电压"), 1, MidpointRounding.AwayFromZero);
            ds.BiasVolt = (ushort)_readData.TryGetValueD29("实际电压PYXQ");
            ds.BiasCurr = (float)Math.Round((float)_readData.TryGetValueD19("实际电流PYXQ"), 1, MidpointRounding.AwayFromZero);
            ds.Pluse1Curr = (float)Math.Round((float)_readData.TryGetValueD29("脉冲电源1电流"), 1, MidpointRounding.AwayFromZero);
            ds.Pluse1KHz = (ushort)_readData.TryGetValueD29("脉冲电源1频率");
            ds.Pluse1Duty = (float)Math.Round((float)_readData.TryGetValueD29("脉冲电源1占空比"), 1, MidpointRounding.AwayFromZero);
            ds.Pluse2Curr = (float)Math.Round((float)_readData.TryGetValueD29("脉冲电源2电流"), 1, MidpointRounding.AwayFromZero);
            ds.Pluse2KHz = (ushort)_readData.TryGetValueD29("脉冲电源2频率");
            ds.Pluse2Duty = (float)Math.Round((float)_readData.TryGetValueD29("脉冲电源2占空比"), 1, MidpointRounding.AwayFromZero);
            ds.CoilCurr = (float)Math.Round((float)_readData.TryGetValueD29("线圈总电流"), 1, MidpointRounding.AwayFromZero);
            ds.CoilVolt = (float)Math.Round((float)_readData.TryGetValueD29("线圈总电压"), 1, MidpointRounding.AwayFromZero);
            ds.Penning = (string)_readData.TryGetValueD19("Penning压力");
            ds.CDG100 = (string)_readData.TryGetValueD19("薄膜规100D压力");
            ds.Pirani1 = (string)_readData.TryGetValueD19("管道压力");
            ds.Pirani2 = (string)_readData.TryGetValueD19("腔体Pirani2压力");
            ds.Water1 = (float)Math.Round((float)_readData.TryGetValueD29("腔体水流量"), 1, MidpointRounding.AwayFromZero);
            ds.Water2 = (float)Math.Round((float)_readData.TryGetValueD29("靶1和4水流量"), 1, MidpointRounding.AwayFromZero);
            ds.Water3 = (float)Math.Round((float)_readData.TryGetValueD29("靶2和3水流量"), 1, MidpointRounding.AwayFromZero);
            ds.Water4 = (float)Math.Round((float)_readData.TryGetValueD29("靶5和6水流量"), 1, MidpointRounding.AwayFromZero);
            ds.Water5 = (float)Math.Round((float)_readData.TryGetValueD29("靶7和8水流量"), 1, MidpointRounding.AwayFromZero);
            ds.Water6 = (float)Math.Round((float)_readData.TryGetValueD29("靶座水流量"), 1, MidpointRounding.AwayFromZero);
            ds.Water7 = (float)Math.Round((float)_readData.TryGetValueD29("电源水流量"), 1, MidpointRounding.AwayFromZero);
            ds.Water8 = (float)Math.Round((float)_readData.TryGetValueD29("分子泵水流量"), 1, MidpointRounding.AwayFromZero);
            ds.Water9 = (float)Math.Round((float)_readData.TryGetValueD29("罗茨泵水流量"), 1, MidpointRounding.AwayFromZero);
            return ds;
        }
    }
}
