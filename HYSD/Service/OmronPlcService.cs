using HslCommunication;
using HslCommunication.Profinet.Omron;
using System;
using System.Threading;

public class OmronPlcService : IOmronPlcService
{
    private readonly OmronFinsNet _plc;
    private volatile bool _isConnected = false;

    // ★ 新增：上次尝试连接的时间戳，用于节流
    private long _lastConnectAttemptTicks = 0;
    // ★ 重连最小间隔（Tick），默认 5 秒
    private const long ReconnectIntervalTicks = 5L * TimeSpan.TicksPerSecond;

    private static readonly string[] NetworkErrorKeywords = { "连接", "超时", "异常", "断开", "Connection", "无法" };
    private readonly object _connectLock = new object();

    public OmronPlcService(string plcIp, int plcPort)
    {
        _plc = new OmronFinsNet(plcIp, plcPort);
        _plc.SA1 = 0x02;
        _plc.DA1 = 0x01;
        _plc.ReceiveTimeOut = 1000;   // ★ 3s → 1s，断线检测更快
    }

    public bool IsConnected => _isConnected;

    private OperateResult<T> SafeExecute<T>(Func<OperateResult<T>> action)
    {
        if (!EnsureConnected())
            return new OperateResult<T>("PLC未连接");

        var result = action();
        if (!result.IsSuccess)
            CheckAndResetConnection(result.Message);
        return result;
    }

    private OperateResult SafeExecute(Func<OperateResult> action)
    {
        if (!EnsureConnected())
            return new OperateResult("PLC未连接");
        var result = action();
        if (!result.IsSuccess)
            CheckAndResetConnection(result.Message);
        return result;
    }

    // ★ 关键改造：带节流的 EnsureConnected
    private bool EnsureConnected()
    {
        if (_isConnected) return true;

        lock (_connectLock)
        {
            if (_isConnected) return true;

            // ★ 节流：距离上次尝试不足 5 秒就直接返回 false，不再 ConnectServer
            var nowTicks = DateTime.UtcNow.Ticks;
            if (nowTicks - _lastConnectAttemptTicks < ReconnectIntervalTicks)
                return false;
            _lastConnectAttemptTicks = nowTicks;

            try
            {
                var connectResult = _plc.ConnectServer();
                _isConnected = connectResult.IsSuccess;
                return _isConnected;
            }
            catch
            {
                _isConnected = false;
                return false;
            }
        }
    }

    private void CheckAndResetConnection(string errorMessage)
    {
        if (string.IsNullOrEmpty(errorMessage)) return;
        foreach (var keyword in NetworkErrorKeywords)
        {
            if (errorMessage.Contains(keyword))
            {
                _isConnected = false;
                try { _plc.ConnectClose(); } catch { }
                break;
            }
        }
    }


    // ==================== 读操作 ====================

    public OperateResult<bool> ReadBool(string address) => SafeExecute(() => _plc.ReadBool(address));
    public OperateResult<short> ReadInt16(string address) => SafeExecute(() => _plc.ReadInt16(address));
    public OperateResult<ushort> ReadUInt16(string address) => SafeExecute(() => _plc.ReadUInt16(address));
    public OperateResult<int> ReadInt32(string address) => SafeExecute(() => _plc.ReadInt32(address));
    public OperateResult<uint> ReadUInt32(string address) => SafeExecute(() => _plc.ReadUInt32(address));
    public OperateResult<float> ReadFloat(string address) => SafeExecute(() => _plc.ReadFloat(address));
    public OperateResult<double> ReadDouble(string address) => SafeExecute(() => _plc.ReadDouble(address));
    public OperateResult<string> ReadString(string address, int length) => SafeExecute(() => _plc.ReadString(address, (ushort)length));

    public OperateResult<bool[]> ReadBoolArray(string address, int length) => SafeExecute(() => _plc.ReadBool(address, (ushort)length));
    public OperateResult<short[]> ReadInt16Array(string address, int length) => SafeExecute(() => _plc.ReadInt16(address, (ushort)length));
    public OperateResult<ushort[]> ReadUInt16Array(string address, int length) => SafeExecute(() => _plc.ReadUInt16(address, (ushort)length));
    public OperateResult<int[]> ReadInt32Array(string address, int length) => SafeExecute(() => _plc.ReadInt32(address, (ushort)length));
    public OperateResult<float[]> ReadFloatArray(string address, int length) => SafeExecute(() => _plc.ReadFloat(address, (ushort)length));
    public OperateResult<byte[]> ReadByteArray(string address, int length) => SafeExecute(() => _plc.Read(address, (ushort)length));

    // 👇 👇 👇 新增：自定义数据类型批量读取 👇 👇 👇
    public OperateResult<T> ReadCustomer<T>(string address) where T : IDataTransfer, new()
    {
        return SafeExecute(() => _plc.ReadCustomer<T>(address));
    }

    // ==================== 写操作 ====================
    // 👇 注意这里，现在调用的是下面那个没有 <T> 的 SafeExecute 方法了，完美匹配！

    public OperateResult Write(string address, bool value) => SafeExecute(() => _plc.Write(address, value));
    public OperateResult Write(string address, short value) => SafeExecute(() => _plc.Write(address, value));
    public OperateResult Write(string address, ushort value) => SafeExecute(() => _plc.Write(address, value));
    public OperateResult Write(string address, int value) => SafeExecute(() => _plc.Write(address, value));
    public OperateResult Write(string address, uint value) => SafeExecute(() => _plc.Write(address, value));
    public OperateResult Write(string address, float value) => SafeExecute(() => _plc.Write(address, value));
    public OperateResult Write(string address, double value) => SafeExecute(() => _plc.Write(address, value));
    public OperateResult Write(string address, string value) => SafeExecute(() => _plc.Write(address, value));

    public OperateResult Write(string address, ushort[] values) => SafeExecute(() => _plc.Write(address, values));
    public OperateResult Write(string address, int[] values) => SafeExecute(() => _plc.Write(address, values));
    public OperateResult Write(string address, float[] values) => SafeExecute(() => _plc.Write(address, values));

    // 👇 👇 👇 新增：自定义数据类型批量写入 👇 👇 👇
    public OperateResult WriteCustomer<T>(string address, T data) where T : IDataTransfer, new()
    {
        return SafeExecute(() => _plc.WriteCustomer(address, data));
    }
}
