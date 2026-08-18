using System;

namespace HYSD
{
    /// <summary>
    /// 配方运行监控服务接口。
    /// 在后台订阅 IReadDataService.DataUpdated 事件，检测"涂层数据记录位"上升沿，
    /// 将配方运行数据（TCData）写入当前指定的批次数据库（SQLite）。
    /// 该服务独立于页面生命周期运行——即使 HistoryData 画面未显示，数据仍会被记录。
    /// </summary>
    public interface IRecipeMonitorService
    {
        /// <summary>当前是否正在监控（已订阅事件且数据库就绪）</summary>
        bool IsMonitoring { get; }

        /// <summary>当前监控写入的批次数据库文件完整路径；未启动时为 null</summary>
        string CurrentDbPath { get; }

        /// <summary>当前监控的批次名称；未启动时为 null</summary>
        string CurrentBatchName { get; }

        /// <summary>
        /// 启动监控，将配方数据写入指定批次数据库。
        /// 若当前已在监控其他数据库，会先停止旧的再启动新的。
        /// </summary>
        /// <param name="dbPath">批次数据库文件完整路径</param>
        /// <param name="batchName">批次名称（用于日志与状态展示）</param>
        void Start(string dbPath, string batchName);

        /// <summary>停止监控，释放数据库连接</summary>
        void Stop();
    }
}
