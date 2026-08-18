using System;
using HslCommunication;
using HslCommunication.Core;

namespace HYSD
{
    public class TCRecipeData : IDataTransfer
    {
        // 欧姆龙 FINS/TCP 必须使用 ReverseWordTransform
        public IByteTransform ByteTransform { get; set; } = new ReverseWordTransform();

        // ========== 属性定义 ==========

        public int ID { get; set; }
        public ushort UpHeat { get; set; }      // 上温度
        public ushort DnHeat { get; set; }      // 下温度
        public ushort Rotation { get; set; }    // 转速

        public bool MFC1SW { get; set; }        // MFC1开关
        public ushort MFC1SV { get; set; }      // MFC1值
        public bool MFC2SW { get; set; }        // MFC2开关
        public ushort MFC2SV { get; set; }      // MFC2值
        public bool MFC3SW { get; set; }        // MFC3开关
        public ushort MFC3SV { get; set; }      // MFC3值
        public bool MFC4SW { get; set; }        // MFC4开关
        public ushort MFC4SV { get; set; }      // MFC4值

        public ushort CoolTemp { get; set; }    // 冷却温度
        public ushort CTime { get; set; }       // 时间

        public bool BiasSW { get; set; }        // 偏压开关
        public ushort BiasKHz { get; set; }     // 偏压频率
        public ushort BiasVolt { get; set; }    // 偏压电压

        public bool ARC1SW { get; set; }        // ARC1开关
        public ushort ARC1SV { get; set; }      // ARC1值
        public bool ARC2SW { get; set; }        // ARC2开关
        public ushort ARC2SV { get; set; }      // ARC2值
        public bool ARC3SW { get; set; }        // ARC3开关
        public ushort ARC3SV { get; set; }      // ARC3值
        public bool ARC4SW { get; set; }        // ARC4开关
        public ushort ARC4SV { get; set; }      // ARC4值
        public bool ARC5SW { get; set; }        // ARC5开关
        public ushort ARC5SV { get; set; }      // ARC5值
        public bool ARC6SW { get; set; }        // ARC6开关
        public ushort ARC6SV { get; set; }      // ARC6值

        public bool CoilSW { get; set; }        // 线圈开关
        public ushort BiasDuty { get; set; }    // 偏压占空比
        public ushort BiasThe { get; set; }     // 偏压电流阈值
        public ushort CoilH { get; set; }       // 线圈高电流
        public ushort CoilT0 { get; set; }      // 线圈T0
        public ushort CoilT1 { get; set; }      // 线圈T1
        public ushort CoilL { get; set; }       // 线圈低电流
        public ushort CoilT2 { get; set; }      // 线圈T2
        public ushort CoilT3 { get; set; }      // 线圈T3

        public bool PF { get; set; }            // 压力/流量控制

        public float CDG100DSV { get; set; }    // 薄膜规设定
        public float LowTemp { get; set; }      // 大于低温设定值
        public float HighTemp { get; set; }     // 小于高温设定值
        public float Pressure { get; set; }     // 小于真空压力
        public float Cool { get; set; }         // 小于冰水机温度值

        // ========== 内存大小计算 ==========
        // 25个ushort(25字) + 13个bool(13字) + 5个float(10字) = 48 个字
        public ushort ReadCount => 48;

        // ========== 反序列化 (PLC -> C#) ==========
        public void ParseSource(byte[] Content)
        {
            int index = 0;

            UpHeat = ByteTransform.TransUInt16(Content, index); index += 2;
            DnHeat = ByteTransform.TransUInt16(Content, index); index += 2;
            Rotation = ByteTransform.TransUInt16(Content, index); index += 2;

            MFC1SW = ByteTransform.TransUInt16(Content, index) != 0; index += 2;
            MFC1SV = ByteTransform.TransUInt16(Content, index); index += 2;
            MFC2SW = ByteTransform.TransUInt16(Content, index) != 0; index += 2;
            MFC2SV = ByteTransform.TransUInt16(Content, index); index += 2;
            MFC3SW = ByteTransform.TransUInt16(Content, index) != 0; index += 2;
            MFC3SV = ByteTransform.TransUInt16(Content, index); index += 2;
            MFC4SW = ByteTransform.TransUInt16(Content, index) != 0; index += 2;
            MFC4SV = ByteTransform.TransUInt16(Content, index); index += 2;

            CoolTemp = ByteTransform.TransUInt16(Content, index); index += 2;
            CTime = ByteTransform.TransUInt16(Content, index); index += 2;

            BiasSW = ByteTransform.TransUInt16(Content, index) != 0; index += 2;
            BiasKHz = ByteTransform.TransUInt16(Content, index); index += 2;
            BiasVolt = ByteTransform.TransUInt16(Content, index); index += 2;

            ARC1SW = ByteTransform.TransUInt16(Content, index) != 0; index += 2;
            ARC1SV = ByteTransform.TransUInt16(Content, index); index += 2;
            ARC2SW = ByteTransform.TransUInt16(Content, index) != 0; index += 2;
            ARC2SV = ByteTransform.TransUInt16(Content, index); index += 2;
            ARC3SW = ByteTransform.TransUInt16(Content, index) != 0; index += 2;
            ARC3SV = ByteTransform.TransUInt16(Content, index); index += 2;
            ARC4SW = ByteTransform.TransUInt16(Content, index) != 0; index += 2;
            ARC4SV = ByteTransform.TransUInt16(Content, index); index += 2;
            ARC5SW = ByteTransform.TransUInt16(Content, index) != 0; index += 2;
            ARC5SV = ByteTransform.TransUInt16(Content, index); index += 2;
            ARC6SW = ByteTransform.TransUInt16(Content, index) != 0; index += 2;
            ARC6SV = ByteTransform.TransUInt16(Content, index); index += 2;

            CoilSW = ByteTransform.TransUInt16(Content, index) != 0; index += 2;
            BiasDuty = ByteTransform.TransUInt16(Content, index); index += 2;
            BiasThe = ByteTransform.TransUInt16(Content, index); index += 2;
            CoilH = ByteTransform.TransUInt16(Content, index); index += 2;
            CoilT0 = ByteTransform.TransUInt16(Content, index); index += 2;
            CoilT1 = ByteTransform.TransUInt16(Content, index); index += 2;
            CoilL = ByteTransform.TransUInt16(Content, index); index += 2;
            CoilT2 = ByteTransform.TransUInt16(Content, index); index += 2;
            CoilT3 = ByteTransform.TransUInt16(Content, index); index += 2;

            PF = ByteTransform.TransUInt16(Content, index) != 0; index += 2;

            CDG100DSV = ByteTransform.TransSingle(Content, index); index += 4;
            LowTemp = ByteTransform.TransSingle(Content, index); index += 4;
            HighTemp = ByteTransform.TransSingle(Content, index); index += 4;
            Pressure = ByteTransform.TransSingle(Content, index); index += 4;
            Cool = ByteTransform.TransSingle(Content, index); index += 4;
        }

        // ========== 序列化 (C# -> PLC) ==========
        public byte[] ToSource()
        {
            byte[] buffer = new byte[ReadCount * 2]; // 48字 * 2 = 96字节
            int index = 0;

            ByteTransform.TransByte(UpHeat).CopyTo(buffer, index); index += 2;
            ByteTransform.TransByte(DnHeat).CopyTo(buffer, index); index += 2;
            ByteTransform.TransByte(Rotation).CopyTo(buffer, index); index += 2;

            ByteTransform.TransByte((ushort)(MFC1SW ? 1 : 0)).CopyTo(buffer, index); index += 2;
            ByteTransform.TransByte(MFC1SV).CopyTo(buffer, index); index += 2;
            ByteTransform.TransByte((ushort)(MFC2SW ? 1 : 0)).CopyTo(buffer, index); index += 2;
            ByteTransform.TransByte(MFC2SV).CopyTo(buffer, index); index += 2;
            ByteTransform.TransByte((ushort)(MFC3SW ? 1 : 0)).CopyTo(buffer, index); index += 2;
            ByteTransform.TransByte(MFC3SV).CopyTo(buffer, index); index += 2;
            ByteTransform.TransByte((ushort)(MFC4SW ? 1 : 0)).CopyTo(buffer, index); index += 2;
            ByteTransform.TransByte(MFC4SV).CopyTo(buffer, index); index += 2;

            ByteTransform.TransByte(CoolTemp).CopyTo(buffer, index); index += 2;
            ByteTransform.TransByte(CTime).CopyTo(buffer, index); index += 2;

            ByteTransform.TransByte((ushort)(BiasSW ? 1 : 0)).CopyTo(buffer, index); index += 2;
            ByteTransform.TransByte(BiasKHz).CopyTo(buffer, index); index += 2;
            ByteTransform.TransByte(BiasVolt).CopyTo(buffer, index); index += 2;

            ByteTransform.TransByte((ushort)(ARC1SW ? 1 : 0)).CopyTo(buffer, index); index += 2;
            ByteTransform.TransByte(ARC1SV).CopyTo(buffer, index); index += 2;
            ByteTransform.TransByte((ushort)(ARC2SW ? 1 : 0)).CopyTo(buffer, index); index += 2;
            ByteTransform.TransByte(ARC2SV).CopyTo(buffer, index); index += 2;
            ByteTransform.TransByte((ushort)(ARC3SW ? 1 : 0)).CopyTo(buffer, index); index += 2;
            ByteTransform.TransByte(ARC3SV).CopyTo(buffer, index); index += 2;
            ByteTransform.TransByte((ushort)(ARC4SW ? 1 : 0)).CopyTo(buffer, index); index += 2;
            ByteTransform.TransByte(ARC4SV).CopyTo(buffer, index); index += 2;
            ByteTransform.TransByte((ushort)(ARC5SW ? 1 : 0)).CopyTo(buffer, index); index += 2;
            ByteTransform.TransByte(ARC5SV).CopyTo(buffer, index); index += 2;
            ByteTransform.TransByte((ushort)(ARC6SW ? 1 : 0)).CopyTo(buffer, index); index += 2;
            ByteTransform.TransByte(ARC6SV).CopyTo(buffer, index); index += 2;

            ByteTransform.TransByte((ushort)(CoilSW ? 1 : 0)).CopyTo(buffer, index); index += 2;
            ByteTransform.TransByte(BiasDuty).CopyTo(buffer, index); index += 2;
            ByteTransform.TransByte(BiasThe).CopyTo(buffer, index); index += 2;
            ByteTransform.TransByte(CoilH).CopyTo(buffer, index); index += 2;
            ByteTransform.TransByte(CoilT0).CopyTo(buffer, index); index += 2;
            ByteTransform.TransByte(CoilT1).CopyTo(buffer, index); index += 2;
            ByteTransform.TransByte(CoilL).CopyTo(buffer, index); index += 2;
            ByteTransform.TransByte(CoilT2).CopyTo(buffer, index); index += 2;
            ByteTransform.TransByte(CoilT3).CopyTo(buffer, index); index += 2;

            ByteTransform.TransByte((ushort)(PF ? 1 : 0)).CopyTo(buffer, index); index += 2;

            ByteTransform.TransByte(CDG100DSV).CopyTo(buffer, index); index += 4;
            ByteTransform.TransByte(LowTemp).CopyTo(buffer, index); index += 4;
            ByteTransform.TransByte(HighTemp).CopyTo(buffer, index); index += 4;
            ByteTransform.TransByte(Pressure).CopyTo(buffer, index); index += 4;
            ByteTransform.TransByte(Cool).CopyTo(buffer, index); index += 4;

            return buffer;
        }
    }

}
