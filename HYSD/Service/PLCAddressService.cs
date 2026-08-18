using ExcelDataReader;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HYSD
{
    public class PLCAddressService : IPLCAddressService
    {
        private readonly string _filePath;
        // ★ 启动加速：缓存 Excel 读取结果。
        // 原代码每次 ReadSheet() 都重新打开文件解析所有 sheet，
        // 启动时被调用 9 次（ReadDataService 6 + FormMain 1 + Alarm 1 + JK 1），
        // 每次数十~数百毫秒，合计可能数秒。缓存后只读一次。
        private DataSet _cache;
        private readonly object _lock = new object();

        public PLCAddressService(string filepath)
        {
            _filePath = filepath;
        }

        public DataSet ReadSheet()
        {
            // 快速路径：已缓存直接返回（无锁）
            if (_cache != null) return _cache;

            lock (_lock)
            {
                if (_cache != null) return _cache; // 双重检查

                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                using (var stream = File.OpenRead(_filePath))
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    var conf = new ExcelDataSetConfiguration
                    {
                        ConfigureDataTable = _ => new ExcelDataTableConfiguration
                        {
                            UseHeaderRow = true
                        }
                    };
                    _cache = reader.AsDataSet(conf);
                }
                return _cache;
            }
        }

        public Dictionary<string, string> GetAddressMapping(DataSet dataSet, int index, string key, string value)
        {
            return dataSet.Tables[index].AsEnumerable()
                .Where(row => row[key] != null && row[value] != null)
                .ToDictionary(
                    row => row[key].ToString().Trim(),
                    row => row[value].ToString().Trim()
                );
        }
    }
}
