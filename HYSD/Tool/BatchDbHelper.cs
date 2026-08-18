using Serilog;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HYSD
{
    /// <summary>
    /// 批次数据库管理工具类（基于 SQLite）。
    /// 负责按批次名称创建 .db 文件、初始化 TCData 表结构、
    /// 提供针对指定批次数据库的 SqlSugarClient 实例，并列出已保存的数据库。
    ///
    /// 为什么用 SQLite 而非 Access：
    /// 1. 项目已集成 System.Data.SQLite.Core，零依赖成本。
    /// 2. 无需安装任何 OLEDB 驱动，32/64 位通吃。
    /// 3. 单文件存储，便于归档、拷贝、分发。
    /// 4. 性能优于 Access，并发写入更稳定。
    /// </summary>
    public static class BatchDbHelper
    {
        /// <summary>
        /// 批次数据库存放目录（程序运行目录下的 Databases 子目录）。
        /// 所有按批次创建的 SQLite 文件都集中存放于此，便于归档与检索。
        /// </summary>
        public static readonly string DbDirectory =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Databases");

        /// <summary>SQLite 文件扩展名</summary>
        private const string DbExtension = ".db";

        /// <summary>确保 Databases 目录存在，不存在则自动创建</summary>
        public static void EnsureDirectory()
        {
            if (!Directory.Exists(DbDirectory))
            {
                Directory.CreateDirectory(DbDirectory);
            }
        }

        /// <summary>
        /// 清理文件名中的非法字符，避免创建文件时失败。
        /// Windows 文件名禁止的字符：\ / : * ? " &lt; &gt; |，统一替换为下划线。
        /// </summary>
        public static string SanitizeFileName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "Untitled_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");

            char[] invalidChars = Path.GetInvalidFileNameChars();
            string safe = name;
            foreach (char c in invalidChars)
            {
                safe = safe.Replace(c, '_');
            }
            // 额外替换 Windows 文件名敏感字符
            foreach (char c in new[] { '\\', '/', ':', '*', '?', '"', '<', '>', '|' })
            {
                safe = safe.Replace(c, '_');
            }
            return safe.Trim();
        }

        /// <summary>
        /// 按批次名称创建 SQLite 数据库文件，并初始化 TCData 表结构。
        /// 若同名文件已存在，则直接复用（不覆盖已有数据），适合断电重启后继续记录同一批次。
        /// </summary>
        /// <param name="batchName">批次名称（会自动清理非法字符）</param>
        /// <returns>数据库文件的完整路径</returns>
        public static string CreateDatabase(string batchName)
        {
            EnsureDirectory();

            string safeName = SanitizeFileName(batchName);
            string dbPath = Path.Combine(DbDirectory, safeName + DbExtension);

            // 文件已存在则直接返回（复用，不覆盖）
            if (File.Exists(dbPath))
            {
                Log.Logger.Information("批次数据库已存在，直接复用: {Path}", dbPath);
                return dbPath;
            }

            // 创建 SQLite 数据库文件 + 初始化表结构
            // SQLite 特性：连接时若文件不存在会自动创建空文件
            var db = new SqlSugarClient(new ConnectionConfig()
            {
                ConnectionString = BuildConnectionString(dbPath),
                DbType = SqlSugar.DbType.Sqlite,
                IsAutoCloseConnection = true,
                InitKeyType = InitKeyType.Attribute
            });

            try
            {
                // 开启 WAL 模式，提升并发写入性能
                db.Ado.ExecuteCommand("PRAGMA journal_mode=WAL;");
                db.Ado.ExecuteCommand("PRAGMA synchronous=NORMAL;");

                // CodeFirst 方式初始化 TCData 表（表已存在则跳过）
                db.CodeFirst.InitTables(typeof(TCData));

                Log.Logger.Information("批次数据库创建成功: {Path}", dbPath);
            }
            finally
            {
                db.Dispose();
            }

            return dbPath;
        }

        /// <summary>
        /// 构建针对指定 SQLite 文件的连接字符串。
        /// 启用 WAL 模式与外键约束，提升并发与数据完整性。
        /// </summary>
        public static string BuildConnectionString(string dbPath)
        {
            // SQLite 连接字符串参数说明：
            // Data Source     : 数据库文件路径
            // Version         : SQLite 版本（固定为3）
            // Pooling         : 启用连接池，减少频繁打开/关闭的开销
            // Journal Mode    : WAL 模式，支持并发读写
            return $"Data Source={dbPath};Version=3;Pooling=True;";
        }

        /// <summary>
        /// 创建并返回针对指定批次数据库的 SqlSugarClient 实例。
        /// 调用方负责在使用完毕后 Dispose。
        /// </summary>
        /// <param name="dbPath">SQLite 数据库文件完整路径</param>
        /// <returns>已配置好的 SqlSugarClient</returns>
        public static SqlSugarClient CreateClient(string dbPath)
        {
            if (!File.Exists(dbPath))
                throw new FileNotFoundException("批次数据库文件不存在", dbPath);

            var db = new SqlSugarClient(new ConnectionConfig()
            {
                ConnectionString = BuildConnectionString(dbPath),
                DbType = SqlSugar.DbType.Sqlite,
                IsAutoCloseConnection = true,
                InitKeyType = InitKeyType.Attribute
            });

            // 开启 WAL 与外键约束
            db.Ado.ExecuteCommand("PRAGMA journal_mode=WAL;");
            db.Ado.ExecuteCommand("PRAGMA synchronous=NORMAL;");
            db.Ado.ExecuteCommand("PRAGMA foreign_keys=ON;");

            return db;
        }

        /// <summary>
        /// 在指定数据库中初始化 TCData 表（CodeFirst 方式，表已存在则跳过）。
        /// </summary>
        public static void InitTables(SqlSugarClient db)
        {
            if (db == null) throw new ArgumentNullException(nameof(db));
            db.CodeFirst.InitTables(typeof(TCData));
        }

        /// <summary>
        /// 列出 Databases 目录下所有已保存的批次数据库文件（.db）。
        /// 按最后修改时间倒序排列（最新的在前）。
        /// </summary>
        /// <returns>文件完整路径列表；目录不存在时返回空列表</returns>
        public static List<string> ListDatabases()
        {
            var result = new List<string>();
            if (!Directory.Exists(DbDirectory))
                return result;

            try
            {
                result = Directory.GetFiles(DbDirectory)
                    .Where(f => f.EndsWith(DbExtension, StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(f => File.GetLastWriteTime(f))
                    .ToList();
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "列出批次数据库文件失败");
            }

            return result;
        }

        /// <summary>
        /// 获取最近一次生成（最后修改时间最新）的批次数据库文件完整路径。
        /// 用于软件启动时自动选中上一次使用的批次，无需用户手动选择。
        /// </summary>
        /// <returns>最新的 .db 文件完整路径；若 Databases 目录不存在或无任何数据库则返回 null</returns>
        public static string GetLatestDatabase()
        {
            try
            {
                var dbs = ListDatabases();   // 已按最后修改时间倒序排列，最新的在前
                if (dbs == null || dbs.Count == 0) return null;

                string latest = dbs[0];
                if (!File.Exists(latest)) return null;

                Log.Logger.Information("找到最近一次生成的批次数据库: {Path}", latest);
                return latest;
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "获取最近批次数据库失败");
                return null;
            }
        }

        /// <summary>获取数据库文件的显示名称（不含扩展名）</summary>
        public static string GetDisplayName(string dbPath)
        {
            if (string.IsNullOrEmpty(dbPath)) return string.Empty;
            return Path.GetFileNameWithoutExtension(dbPath);
        }

        /// <summary>
        /// 获取指定批次数据库中 TCData 表的记录条数。
        /// 用于在下拉框中显示每个批次的记录数，方便用户识别。
        /// </summary>
        public static int GetRecordCount(string dbPath)
        {
            try
            {
                if (!File.Exists(dbPath)) return 0;
                using (var db = CreateClient(dbPath))
                {
                    return db.Queryable<TCData>().Count();
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Debug(ex, "获取批次记录条数失败: {Path}", dbPath);
                return 0;
            }
        }
    }
}
