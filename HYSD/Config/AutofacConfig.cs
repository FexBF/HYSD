using Autofac;
using Serilog;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace HYSD
{
    public class AutofacConfig
    {
        public static IContainer Container { get; private set; }
        // ★ 修复：原为 HashSet<AlarmData>，被多线程并发读写非线程安全。
        // 改为 ConcurrentAlarmSet，内部用 lock 保护，API 兼容原有调用。
        public static readonly ConcurrentAlarmSet _alarmDatas = new ConcurrentAlarmSet();
        public static bool isHeatChange = false;
        public static bool isQtksChange = false;
        public static bool isTCRPChange = false;
        public static void Register()
        {
            var builder = new ContainerBuilder();

            // 1. 配置 Serilog
            var logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs", "log-.txt");

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug() // 设置最低日志级别
                                      // 注册自定义的节流过滤器：5分钟内相同的错误只记录第一次
                .Filter.With(new ThrottlingFilter(TimeSpan.FromMinutes(60)))
                .WriteTo.Console()    // 输出到控制台
                .WriteTo.File(
                    path: logFilePath,
                    rollingInterval: RollingInterval.Day, // 每天生成一个文件
                    retainedFileCountLimit: 30,           // 保留最近30天的日志
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
                )
                .CreateLogger();

            // 2. 注册 ILogger (单例)
            builder.RegisterInstance(Log.Logger).As<ILogger>().SingleInstance();

            // 3. 注册 SqlSugarClient
            builder.Register(c =>
            {
                var logger = c.Resolve<ILogger>(); // 从容器获取日志实例

                var db = new SqlSugarClient(new ConnectionConfig()
                {
                    ConnectionString = "Data Source=./HYSD.db",
                    DbType = DbType.Sqlite,
                    IsAutoCloseConnection = true,
                    InitKeyType = InitKeyType.Attribute
                });

                // 将 SqlSugar 的 SQL 日志桥接到 Serilog
                db.Aop.OnLogExecuting = (sql, pars) =>
                {
                    // 使用 Debug 级别记录 SQL，避免生产环境日志过多
                    logger.Debug("SQL Executing: {Sql}\nParameters: {@Pars}",
                        sql,
                        pars.ToDictionary(p => p.ParameterName, p => p.Value));
                };

                // 捕获 SqlSugar 异常并记录错误日志
                db.Aop.OnError = (exp) =>
                {
                    logger.Error(exp.InnerException, "SqlSugar Error: {Sql}", exp.Sql);
                };

                db.Ado.ExecuteCommand("PRAGMA journal_mode=WAL;");     // 开启WAL
                db.Ado.ExecuteCommand("PRAGMA synchronous=NORMAL;");   // 降低同步级别，WAL下NORMAL足够安全且大幅提升写性能
                db.Ado.ExecuteCommand("PRAGMA foreign_keys=ON;");      // 开启外键约束（SQLite默认关闭）

                return db;
            }).As<SqlSugarClient>().InstancePerDependency();

            // 4. 注册主窗体 (如果需要构造函数注入)
            builder.RegisterType<FormMain>().AsSelf();
            builder.RegisterType<Vacuum>().AsSelf().SingleInstance();
            builder.RegisterType<HPower>().AsSelf().SingleInstance();
            builder.RegisterType<PYXQ>().AsSelf().SingleInstance();
            builder.RegisterType<McPower>().AsSelf().SingleInstance();
            builder.RegisterType<Air>().AsSelf().SingleInstance();
            builder.RegisterType<Alarm>().AsSelf().SingleInstance();
            builder.RegisterType<JK>().AsSelf().SingleInstance();
            builder.RegisterType<ChooseRp>().AsSelf().SingleInstance();
            builder.RegisterType<HeatRecipe>().AsSelf().SingleInstance();
            builder.RegisterType<QtksRecipe>().AsSelf().SingleInstance();
            builder.RegisterType<TCRP>().AsSelf().SingleInstance();
            builder.RegisterType<HistoryData>().AsSelf().SingleInstance();
            builder.RegisterType<Water>().AsSelf().SingleInstance();
            builder.RegisterType<SetUp>().AsSelf().SingleInstance();
            builder.RegisterType<PLCData>().AsSelf().SingleInstance();
            builder.RegisterType<PLCHeatRecipeData>().AsSelf().SingleInstance();
            builder.RegisterType<PLCQtksRecipeData>().AsSelf().SingleInstance();
            builder.RegisterType<PLCTCRecipeData>().AsSelf().SingleInstance();

            // 从配置文件读取
            string plcIp = ConfigurationManager.AppSettings["PlcIp"] ?? "192.168.250.1";
            int plcPort = int.Parse(ConfigurationManager.AppSettings["PlcPort"] ?? "9600");
            builder.RegisterType<OmronPlcService>()
                .As<IOmronPlcService>()
                .SingleInstance()
                .WithParameter("plcIp", plcIp)
                 .WithParameter("plcPort", plcPort);

            builder.RegisterType<ColorService>().As<IColorService>().SingleInstance();

            string filePath = AppDomain.CurrentDomain.BaseDirectory + "PLC\\Address.xlsx";
            if (!File.Exists(filePath))
            {
                MessageBox.Show($"PLC地址文件未找到: {filePath}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Log.Logger.Error("PLC地址文件未找到: {FilePath}", filePath);
                System.Environment.Exit(0);
            }
            builder.RegisterType<PLCAddressService>().As<IPLCAddressService>().SingleInstance().WithParameter("filepath", filePath);
            builder.RegisterType<ReadDataService>().As<IReadDataService>().SingleInstance();
            // ★★★ 新增：注册配方监控服务（单例，全局唯一监控实例）★★★
            // 该服务订阅 IReadDataService.DataUpdated 事件，在后台检测"涂层数据记录位"上升沿，
            // 将 TCData 写入当前批次对应的 SQLite 数据库。
            builder.RegisterType<RecipeMonitorService>()
                .As<IRecipeMonitorService>()
                .SingleInstance();
            Container = builder.Build();
        }
    }
}