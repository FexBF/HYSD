using HslCommunication.Core;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Navigation;

namespace HYSD
{
    public static class BytesMapper
    {
        private static readonly Regex AddrRegex = new Regex(@"^(D|W|H|C)(\d+)(?:\.(\d+))?$", RegexOptions.IgnoreCase);

        /// <summary>
        /// 预编译的标签描述符。把"每秒重复解析字符串"的工作提前到启动时做一次。
        /// </summary>
        public sealed class TagEntry
        {
            public string Name;
            public int ByteOffset;
            public int TypeSize;
            public Type DataType;
            public int CustomLength;
            public int BitIndex; // bool 类型用，-1 表示非位
        }

        /// <summary>
        /// 在程序启动时调用一次，把 tagDefs 解析成可复用的 TagEntry 列表。
        /// 后续每秒只需遍历 TagEntry 做字节提取，零字符串分配、零正则匹配。
        /// </summary>
        public static List<TagEntry> BuildCache(string baseAddress, Dictionary<string, string> tagDefs)
        {
            var cache = new List<TagEntry>();

            var baseMatch = AddrRegex.Match(baseAddress);
            if (!baseMatch.Success) throw new ArgumentException("基准地址格式错误");
            string baseArea = baseMatch.Groups[1].Value.ToUpper();
            int baseIndex = int.Parse(baseMatch.Groups[2].Value);

            foreach (var kvp in tagDefs)
            {
                string tagName = kvp.Key;
                string tagConfig = kvp.Value;

                string[] parts = tagConfig.Split(new[] { ',', '|' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2) continue;

                string tagAddress = parts[0].Trim();
                string typeName = parts[1].Trim().ToLower();

                int customLength = 0;
                if (parts.Length >= 3)
                {
                    int.TryParse(parts[2].Trim(), out customLength);
                }

                Type dataType = ParseType(typeName);

                var tagMatch = AddrRegex.Match(tagAddress);
                if (!tagMatch.Success) continue;

                string tagArea = tagMatch.Groups[1].Value.ToUpper();
                if (tagArea != baseArea) continue;

                int tagIndex = int.Parse(tagMatch.Groups[2].Value);
                int byteOffset = (tagIndex - baseIndex) * 2;
                int typeSize = GetTypeByteSize(dataType, customLength);
                int bitIndex = tagMatch.Groups[3].Success ? int.Parse(tagMatch.Groups[3].Value) : -1;

                cache.Add(new TagEntry
                {
                    Name = tagName,
                    ByteOffset = byteOffset,
                    TypeSize = typeSize,
                    DataType = dataType,
                    CustomLength = customLength,
                    BitIndex = bitIndex
                });
            }
            return cache;
        }

        /// <summary>
        /// 将连续的字节数组，按照字典规则映射为 变量名-值 的新字典
        /// </summary>
        public static Dictionary<string, object> Map(byte[] buffer, string baseAddress, Dictionary<string, string> tagDefs)
        {
            var result = new Dictionary<string, object>();

            var baseMatch = AddrRegex.Match(baseAddress);
            if (!baseMatch.Success) throw new ArgumentException("基准地址格式错误");
            string baseArea = baseMatch.Groups[1].Value.ToUpper();
            int baseIndex = int.Parse(baseMatch.Groups[2].Value);

            var transform = new RegularByteTransform();
            transform.DataFormat = HslCommunication.Core.DataFormat.CDAB;

            foreach (var kvp in tagDefs)
            {
                string tagName = kvp.Key;
                string tagConfig = kvp.Value;

                // 🌟 修改：支持第三个可选参数（字符串长度）
                // 格式示例："D100,Int16" 或 "D200,String,10"
                string[] parts = tagConfig.Split(new[] { ',', '|' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2) continue;

                string tagAddress = parts[0].Trim();
                string typeName = parts[1].Trim().ToLower();

                // 尝试解析第三个参数作为长度（主要给 String 用）
                int customLength = 0;
                if (parts.Length >= 3)
                {
                    int.TryParse(parts[2].Trim(), out customLength);
                }

                Type dataType = ParseType(typeName);

                var tagMatch = AddrRegex.Match(tagAddress);
                if (!tagMatch.Success) continue;

                string tagArea = tagMatch.Groups[1].Value.ToUpper();
                if (tagArea != baseArea) continue;

                int tagIndex = int.Parse(tagMatch.Groups[2].Value);
                int byteOffset = (tagIndex - baseIndex) * 2;

                // 🌟 修改：传入 customLength，以便 GetTypeByteSize 计算字符串大小
                int typeSize = GetTypeByteSize(dataType, customLength);

                if (byteOffset < 0 || (byteOffset + typeSize) > buffer.Length)
                {
                    result[tagName] = null;
                    continue;
                }

                try
                {
                    object value = null;

                    if (dataType == typeof(bool))
                    {
                        int bitIndex = tagMatch.Groups[3].Success ? int.Parse(tagMatch.Groups[3].Value) : 0;
                        ushort wordVal = transform.TransUInt16(buffer, byteOffset);
                        value = (wordVal & (1 << bitIndex)) != 0;
                    }
                    else if (dataType == typeof(short))
                        value = transform.TransInt16(buffer, byteOffset);
                    else if (dataType == typeof(ushort))
                        value = transform.TransUInt16(buffer, byteOffset);
                    else if (dataType == typeof(int))
                        value = transform.TransInt32(buffer, byteOffset);
                    else if (dataType == typeof(uint))
                        value = transform.TransUInt32(buffer, byteOffset);
                    else if (dataType == typeof(float))
                        value = transform.TransSingle(buffer, byteOffset);
                    else if (dataType == typeof(double))
                        value = transform.TransDouble(buffer, byteOffset);
                    // 🌟 新增：String 类型的解析逻辑
                    else if (dataType == typeof(string))
                    {
                        if (customLength > 0)
                        {
                            // 提取指定长度的字节数组
                            byte[] strBytes = new byte[customLength];
                            Array.Copy(buffer, byteOffset, strBytes, 0, customLength);

                            // 🌟 核心修复：欧姆龙字符串高低字节互换！
                            // 因为 FINS 传输高字节在前，而欧姆龙低字节存前字符，导致每2个字符颠倒
                            for (int i = 0; i < strBytes.Length - 1; i += 2)
                            {
                                byte temp = strBytes[i];
                                strBytes[i] = strBytes[i + 1];
                                strBytes[i + 1] = temp;
                            }

                            // 交换完之后再转成字符串，顺序就完全正确了
                            string rawStr = Encoding.ASCII.GetString(strBytes);

                            // 工控经典去尾操作：去掉末尾的空字符 \0 和 空格
                            value = rawStr.TrimEnd('\0', ' ');
                        }
                        else
                        {
                            value = string.Empty;
                        }
                    }
                    result[tagName] = value;
                }
                catch
                {
                    result[tagName] = null;
                }
            }

            return result;
        }

        /// <summary>
        /// 使用预编译缓存的高性能映射版本。
        /// 每秒调用时走这个，避免重复 Split/正则/类型解析。
        /// </summary>
        public static Dictionary<string, object> MapCached(byte[] buffer, List<TagEntry> cache)
        {
            var result = new Dictionary<string, object>(cache.Count);
            var transform = new RegularByteTransform();
            transform.DataFormat = HslCommunication.Core.DataFormat.CDAB;

            for (int i = 0; i < cache.Count; i++)
            {
                var entry = cache[i];
                if (entry.ByteOffset < 0 || (entry.ByteOffset + entry.TypeSize) > buffer.Length)
                {
                    result[entry.Name] = null;
                    continue;
                }

                try
                {
                    object value = null;
                    var dt = entry.DataType;

                    if (dt == typeof(bool))
                    {
                        int bitIndex = entry.BitIndex >= 0 ? entry.BitIndex : 0;
                        ushort wordVal = transform.TransUInt16(buffer, entry.ByteOffset);
                        value = (wordVal & (1 << bitIndex)) != 0;
                    }
                    else if (dt == typeof(short))
                        value = transform.TransInt16(buffer, entry.ByteOffset);
                    else if (dt == typeof(ushort))
                        value = transform.TransUInt16(buffer, entry.ByteOffset);
                    else if (dt == typeof(int))
                        value = transform.TransInt32(buffer, entry.ByteOffset);
                    else if (dt == typeof(uint))
                        value = transform.TransUInt32(buffer, entry.ByteOffset);
                    else if (dt == typeof(float))
                        value = transform.TransSingle(buffer, entry.ByteOffset);
                    else if (dt == typeof(double))
                        value = transform.TransDouble(buffer, entry.ByteOffset);
                    else if (dt == typeof(string))
                    {
                        int len = entry.CustomLength;
                        if (len > 0)
                        {
                            byte[] strBytes = new byte[len];
                            Array.Copy(buffer, entry.ByteOffset, strBytes, 0, len);
                            for (int j = 0; j < strBytes.Length - 1; j += 2)
                            {
                                byte temp = strBytes[j];
                                strBytes[j] = strBytes[j + 1];
                                strBytes[j + 1] = temp;
                            }
                            string rawStr = Encoding.ASCII.GetString(strBytes);
                            value = rawStr.TrimEnd('\0', ' ');
                        }
                        else
                        {
                            value = string.Empty;
                        }
                    }
                    result[entry.Name] = value;
                }
                catch
                {
                    result[entry.Name] = null;
                }
            }
            return result;
        }

        private static Type ParseType(string typeName)
        {
            switch (typeName)
            {
                case "bool": case "boolean": case "bit": return typeof(bool);
                case "short": case "int16": return typeof(short);
                case "ushort": case "uint16": case "word": return typeof(ushort);
                case "int": case "int32": return typeof(int);
                case "uint": case "uint32": case "dword": return typeof(uint);
                case "float": case "single": case "real": return typeof(float);
                case "double": case "lreal": return typeof(double);
                // 🌟 新增：识别 String 类型
                case "string": case "str": case "text": return typeof(string);
                default: return typeof(short);
            }
        }

        // 🌟 修改：增加 customLength 参数，用于计算字符串占用字节
        private static int GetTypeByteSize(Type type, int customLength = 0)
        {
            if (type == typeof(bool)) return 2;
            if (type == typeof(short) || type == typeof(ushort)) return 2;
            if (type == typeof(int) || type == typeof(uint) || type == typeof(float)) return 4;
            if (type == typeof(double) || type == typeof(long)) return 8;

            // 🌟 新增：字符串的大小等于你配置的长度
            if (type == typeof(string)) return customLength > 0 ? customLength : 2;

            return 2;
        }
    }

    public class ReadDataService : IReadDataService
    {
        private readonly ConcurrentDictionary<string, object> _dataPoolD10 = new ConcurrentDictionary<string, object>();
        private readonly ConcurrentDictionary<string, object> _dataPoolD15 = new ConcurrentDictionary<string, object>();
        private readonly ConcurrentDictionary<string, object> _dataPoolD19 = new ConcurrentDictionary<string, object>();
        private readonly ConcurrentDictionary<string, object> _dataPoolD29 = new ConcurrentDictionary<string, object>();
        private readonly ConcurrentDictionary<string, object> _dataPoolW = new ConcurrentDictionary<string, object>();
        private readonly ConcurrentDictionary<string, object> _dataPoolC = new ConcurrentDictionary<string, object>();
        private readonly IOmronPlcService _plc;
        private readonly ILogger _logger;
        private readonly IPLCAddressService _address;
        private readonly Dictionary<string, string> _addressMappingD10, _addressMappingD15, _addressMappingD19, _addressMappingD29, _addressMappingW, _addressMappingC;
        private CancellationTokenSource _cts;
        // ★ 修复：被后台线程写、被 UI 线程读，必须 volatile 保证可见性，否则 UI 可能读到脏值导致误判 PLC 未连接
        private volatile bool ReadisRunning;

        // ★ 事件驱动：记录上一次连接状态，仅当发生跃迁时才触发 ConnectionChanged，避免重复通知
        private bool _lastConnected;

        /// <summary>PLC 数据刷新完成事件 —— 每轮读取成功后触发</summary>
        public event EventHandler DataUpdated;

        /// <summary>PLC 连接状态变化事件（true=已连接, false=断开）</summary>
        public event EventHandler<bool> ConnectionChanged;

        /// <summary>当前 PLC 是否已连接</summary>
        public bool IsConnected => _plc != null && _plc.IsConnected;

        public ReadDataService(IOmronPlcService plc, ILogger logger, IPLCAddressService address)
        {
            _plc = plc;
            _logger = logger;
            _address = address;
            try
            {
                _addressMappingD10 = _address.GetAddressMapping(_address.ReadSheet(), 14, "Name", "Address");
                _addressMappingD15 = _address.GetAddressMapping(_address.ReadSheet(), 7, "Name", "Address");
                _addressMappingD19 = _address.GetAddressMapping(_address.ReadSheet(), 8, "Name", "Address");
                _addressMappingD29 = _address.GetAddressMapping(_address.ReadSheet(), 9, "Name", "Address");
                _addressMappingW = _address.GetAddressMapping(_address.ReadSheet(), 10, "Name", "Address");
                _addressMappingC = _address.GetAddressMapping(_address.ReadSheet(), 11, "Name", "Address");
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(0);
            }
        }

        public void Start()
        {
            ReadLoop();
        }

        private  void ReadLoop()
        {
            _cts = new CancellationTokenSource();
            Task.Run(() =>
            {
                while (!_cts.Token.IsCancellationRequested)
                {
                    DoWork();
                    Thread.Sleep(500); // 每秒读取一次
                }
            }, _cts.Token);

        }

        private void DoWork()
        {
            try
            {
                if (_plc != null && _plc.IsConnected)
                {
                    #region D10
                    var resultD10 = _plc.ReadByteArray("D10000", 5);
                    if (resultD10.IsSuccess)
                    {
                        byte[] bytesD10 = resultD10.Content;
                        var realDataD10 = BytesMapper.Map(bytesD10, "D10000", _addressMappingD10);

                        foreach (var kvp in realDataD10)
                        {
                            _dataPoolD10[kvp.Key] = kvp.Value;
                        }
                    }
                    #endregion
                    #region D15
                    var resultD15 = _plc.ReadByteArray("D15000", 10);
                    if (resultD15.IsSuccess)
                    {
                        byte[] bytesD15 = resultD15.Content;
                        var realDataD15 = BytesMapper.Map(bytesD15, "D15000", _addressMappingD15);

                        foreach (var kvp in realDataD15)
                        {
                            _dataPoolD15[kvp.Key] = kvp.Value;
                        }
                    }
                    #endregion
                    #region D19
                    var resultD19 = _plc.ReadByteArray("D19000", 160);
                    if (resultD19.IsSuccess)
                    {
                        byte[] bytesD19 = resultD19.Content;
                        var realDataD19 = BytesMapper.Map(bytesD19, "D19000", _addressMappingD19);

                        foreach (var kvp in realDataD19)
                        {
                            _dataPoolD19[kvp.Key] = kvp.Value;
                        }
                    }
                    #endregion
                    #region D29
                    var resultD29 = _plc.ReadByteArray("D29600", 386);
                    if (resultD29.IsSuccess)
                    {
                        byte[] bytes = resultD29.Content;
                        var realDataD29 = BytesMapper.Map(bytes, "D29600", _addressMappingD29);

                        foreach (var kvp in realDataD29)
                        {
                            _dataPoolD29[kvp.Key] = kvp.Value; // 比如 _dataPoolD29["Temperature"] = 2500
                        }
                    }
                    #endregion
                    #region W
                    var resultW = _plc.ReadByteArray("W8.00", 493);
                    if (resultW.IsSuccess)
                    {
                        byte[] bytesW = resultW.Content;
                        var realDataW = BytesMapper.Map(bytesW, "W8.00", _addressMappingW);
                        foreach (var kvp in realDataW)
                        {
                            _dataPoolW[kvp.Key] = kvp.Value;
                        }
                    }
                    #endregion
                    #region C
                    var resultC = _plc.ReadByteArray("C0.00", 250);
                    if (resultC.IsSuccess)
                    {
                        byte[] bytesC = resultC.Content;
                        var realDataC = BytesMapper.Map(bytesC, "C0.00", _addressMappingC);
                        foreach (var kvp in realDataC)
                        {
                            _dataPoolC[kvp.Key] = kvp.Value;
                        }
                    }
                    #endregion

                    ReadisRunning = true;
                }
                else
                {
                    ReadisRunning = false;
                }

                // ★ 事件驱动：无论连接与否，每轮读取周期结束都通知订阅方刷新 UI。
                // 断线时各页面的 DoWork() 会走 else 分支处理断线状态
                // （如 Alarm 页加入"PLC已断开连接"报警、Vacuum 页重置阀门状态等），
                // 从而实现断线实时报警。各页面 DoWork 内部均有 _plc.IsConnected 守卫，
                // 不会在断线时显示陈旧数据。
                RaiseDataUpdated();

                // ★ 事件驱动：检测连接状态跃迁，仅在变化时通知（避免每秒重复触发）
                RaiseConnectionChangedIfChanged();
            }
            catch (Exception ex)
            {
                _logger.Error($"读取数据时发生异常: {ex.Message}");
                // 异常时也检测一次连接状态，便于 UI 及时反映断线
                RaiseConnectionChangedIfChanged();
            }
        }


        /// <summary>安全触发 DataUpdated 事件（容忍无订阅方）</summary>
        private void RaiseDataUpdated()
        {
            try
            {
                DataUpdated?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                _logger.Debug($"DataUpdated 订阅方抛出异常: {ex.Message}");
            }
        }

        /// <summary>仅当连接状态发生跃迁时触发 ConnectionChanged</summary>
        private void RaiseConnectionChangedIfChanged()
        {
            bool now = IsConnected;
            if (now == _lastConnected) return;
            _lastConnected = now;
            try
            {
                ConnectionChanged?.Invoke(this, now);
            }
            catch (Exception ex)
            {
                _logger.Debug($"ConnectionChanged 订阅方抛出异常: {ex.Message}");
            }
        }

        public void Stop()
        {
            ReadisRunning = false;
            _cts?.Cancel();
        }

        /// <summary>
        /// 供外部 UI 调用的取数据接口
        /// </summary>
        public object TryGetValueD29(string tagName)
        {
            object value;
            _dataPoolD29.TryGetValue(tagName, out value);
            return value;
        }

        public object TryGetValueD19(string tagName)
        {
            object value;
            _dataPoolD19.TryGetValue(tagName, out value);
            return value;
        }

        public object TryGetValueD15(string tagName)
        {
            object value;
            _dataPoolD15.TryGetValue(tagName, out value);
            return value;
        }

        public object TryGetValueW(string tagName)
        {
            object value;
            _dataPoolW.TryGetValue(tagName, out value);
            return value;
        }

        public object TryGetValueC(string tagName)
        {
            object value;
            _dataPoolC.TryGetValue(tagName, out value);
            return value;
        }

        public object TryGetValueD10(string tagName)
        {
            object value;
            _dataPoolD10.TryGetValue(tagName, out value);
            return value;
        }

        public bool isRunning => ReadisRunning;
    }
}

