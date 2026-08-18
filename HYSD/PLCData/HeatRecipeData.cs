using HslCommunication;
using HslCommunication.Core;
using System;
using System.ComponentModel;

namespace HYSD
{
    public class HeatRecipeData : IDataTransfer
    {
        // 欧姆龙 FINS/TCP 必须使用 ReverseWordTransform
        public IByteTransform ByteTransform { get; set; } = new ReverseWordTransform();

        // ========== 你的属性 ==========
        public int HeatRecipeID { get; set; }
        public ushort UpHeat { get; set; }  // 上温度
        public ushort DnHeat { get; set; }  // 下温度
        public ushort Rotation { get; set; } // 转速

        // 注意：欧姆龙结构体中 BOOL 占 1 个字 (2字节)
        public bool MFC2SW { get; set; } // MFC2开关
        public ushort MFC2SV { get; set; } // MFC2值

        public bool MFC3SW { get; set; } // MFC3开关
        public ushort MFC3SV { get; set; } // MFC3值

        public bool ARC7SW { get; set; } // ARC7开关
        public ushort ARC7SV { get; set; } // ARC7值

        public bool ARC8SW { get; set; } // ARC8开关
        public ushort ARC8SV { get; set; } // ARC8值

        public ushort CoolTemp { get; set; } // 冷却温度
        public ushort HTime { get; set; } // 时间

        public float LowTemp { get; set; } // 大于低温设定值
        public float HighTemp { get; set; }// 小于高温设定值
        public float Pressure { get; set; } // 小于真空压力
        public float Cool { get; set; }//小于冰水机温度值

        // ========== 内存大小计算 ==========
        // 计算原则: ushort=1字, bool(欧姆龙结构体)=1字, float=2字
        // UpHeat(1) + DnHeat(1) + Rotation(1) + MFC2SW(1) + MFC2SV(1) + 
        // MFC3SW(1) + MFC3SV(1) + ARC7SW(1) + ARC7SV(1) + ARC8SW(1) + 
        // ARC8SV(1) + CoolTemp(1) + HTime(1) + LowTemp(2) + HighTemp(2) + 
        // Pressure(2) + Cool(2) = 21 个字
        public ushort ReadCount => 21;

        // ========== 反序列化 (PLC -> C#) ==========
        public void ParseSource(byte[] Content)
        {
            int index = 0;
            UpHeat = ByteTransform.TransUInt16(Content, index); index += 2;
            DnHeat = ByteTransform.TransUInt16(Content, index); index += 2;
            Rotation = ByteTransform.TransUInt16(Content, index); index += 2;

            // bool 按一个 ushort 读取，大于0则为 true
            MFC2SW = ByteTransform.TransUInt16(Content, index) != 0; index += 2;
            MFC2SV = ByteTransform.TransUInt16(Content, index); index += 2;

            MFC3SW = ByteTransform.TransUInt16(Content, index) != 0; index += 2;
            MFC3SV = ByteTransform.TransUInt16(Content, index); index += 2;

            ARC7SW = ByteTransform.TransUInt16(Content, index) != 0; index += 2;
            ARC7SV = ByteTransform.TransUInt16(Content, index); index += 2;

            ARC8SW = ByteTransform.TransUInt16(Content, index) != 0; index += 2;
            ARC8SV = ByteTransform.TransUInt16(Content, index); index += 2;

            CoolTemp = ByteTransform.TransUInt16(Content, index); index += 2;
            HTime = ByteTransform.TransUInt16(Content, index); index += 2;

            // float 占 2 个字 (4个字节)
            LowTemp = ByteTransform.TransSingle(Content, index); index += 4;
            HighTemp = ByteTransform.TransSingle(Content, index); index += 4;
            Pressure = ByteTransform.TransSingle(Content, index); index += 4;
            Cool = ByteTransform.TransSingle(Content, index); index += 4;
        }

        // ========== 序列化 (C# -> PLC) ==========
        public byte[] ToSource()
        {
            byte[] buffer = new byte[ReadCount * 2]; // 21字 * 2 = 42字节
            int index = 0;

            ByteTransform.TransByte(UpHeat).CopyTo(buffer, index); index += 2;
            ByteTransform.TransByte(DnHeat).CopyTo(buffer, index); index += 2;
            ByteTransform.TransByte(Rotation).CopyTo(buffer, index); index += 2;

            // bool 写入 PLC 时，转换为 1 或 0 的 ushort
            ByteTransform.TransByte((ushort)(MFC2SW ? 1 : 0)).CopyTo(buffer, index); index += 2;
            ByteTransform.TransByte(MFC2SV).CopyTo(buffer, index); index += 2;

            ByteTransform.TransByte((ushort)(MFC3SW ? 1 : 0)).CopyTo(buffer, index); index += 2;
            ByteTransform.TransByte(MFC3SV).CopyTo(buffer, index); index += 2;

            ByteTransform.TransByte((ushort)(ARC7SW ? 1 : 0)).CopyTo(buffer, index); index += 2;
            ByteTransform.TransByte(ARC7SV).CopyTo(buffer, index); index += 2;

            ByteTransform.TransByte((ushort)(ARC8SW ? 1 : 0)).CopyTo(buffer, index); index += 2;
            ByteTransform.TransByte(ARC8SV).CopyTo(buffer, index); index += 2;

            ByteTransform.TransByte(CoolTemp).CopyTo(buffer, index); index += 2;
            ByteTransform.TransByte(HTime).CopyTo(buffer, index); index += 2;

            ByteTransform.TransByte(LowTemp).CopyTo(buffer, index); index += 4;
            ByteTransform.TransByte(HighTemp).CopyTo(buffer, index); index += 4;
            ByteTransform.TransByte(Pressure).CopyTo(buffer, index); index += 4;
            ByteTransform.TransByte(Cool).CopyTo(buffer, index); index += 4;

            return buffer;
        }
    }

}