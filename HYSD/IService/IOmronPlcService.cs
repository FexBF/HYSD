using HslCommunication;
using System;

public interface IOmronPlcService
{
    // 基础类型读取
    OperateResult<bool> ReadBool(string address);
    OperateResult<short> ReadInt16(string address);
    OperateResult<ushort> ReadUInt16(string address);
    OperateResult<int> ReadInt32(string address);
    OperateResult<uint> ReadUInt32(string address);
    OperateResult<float> ReadFloat(string address);
    OperateResult<double> ReadDouble(string address);
    OperateResult<string> ReadString(string address, int length);

    // 数组类型读取
    OperateResult<bool[]> ReadBoolArray(string address, int length);
    OperateResult<short[]> ReadInt16Array(string address, int length);
    OperateResult<ushort[]> ReadUInt16Array(string address, int length);
    OperateResult<int[]> ReadInt32Array(string address, int length);
    OperateResult<float[]> ReadFloatArray(string address, int length);
    OperateResult<byte[]> ReadByteArray(string address, int length);
    OperateResult<T> ReadCustomer<T>(string address) where T : IDataTransfer, new();
    // 基础类型写入
    OperateResult Write(string address, bool value);
    OperateResult Write(string address, short value);
    OperateResult Write(string address, ushort value);
    OperateResult Write(string address, int value);
    OperateResult Write(string address, uint value);
    OperateResult Write(string address, float value);
    OperateResult Write(string address, double value);
    OperateResult Write(string address, string value);

    // 数组类型写入
    OperateResult Write(string address, ushort[] values);
    OperateResult Write(string address, int[] values);
    OperateResult Write(string address, float[] values);
    OperateResult WriteCustomer<T>(string address, T data) where T : IDataTransfer, new();
    bool IsConnected { get; }
}
