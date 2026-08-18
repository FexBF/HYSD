using System;
using HslCommunication;
using HslCommunication.Core;

namespace HYSD
{
    public class QtksRecipeData : IDataTransfer
    {
        public IByteTransform ByteTransform { get; set; } = new ReverseWordTransform();

        // ========== 属性 ==========
        public int ID { get; set; }
        public ushort UpHeat { get; set; }
        public ushort DnHeat { get; set; }
        public ushort Rotation { get; set; }
        public bool MFC2SW { get; set; }
        public ushort MFC2SV { get; set; }
        public bool MFC3SW { get; set; }
        public ushort MFC3SV { get; set; }
        public ushort CoolTemp { get; set; }
        public ushort ATime { get; set; }
        public bool BiasSW { get; set; }
        public ushort BiasKHz { get; set; }
        public ushort BiasDuty { get; set; }
        public ushort BiasThe { get; set; }
        public ushort BiasVolt { get; set; }
        public bool ARC7SW { get; set; }
        public ushort ARC7SV { get; set; }
        public bool ARC8SW { get; set; }
        public ushort ARC8SV { get; set; }
        public bool Pluse1SW { get; set; }
        public ushort Pluse1Curr { get; set; }
        public ushort Pluse1ONtime { get; set; }
        public ushort Pluse1OFFtime { get; set; }
        public bool Pluse2SW { get; set; }
        public ushort Pluse2Curr { get; set; }
        public ushort Pluse2ONtime { get; set; }
        public ushort Pluse2OFFtime { get; set; }
        public float LowTemp { get; set; }
        public float HighTemp { get; set; }
        public float Pressure { get; set; }
        public float Cool { get; set; }

        // ========== 重点：34 个字 ==========
        public ushort ReadCount => 34;

        // ========== 反序列化 (PLC -> C#) ==========
        public void ParseSource(byte[] Content)
        {
            int index = 0;

            UpHeat = ByteTransform.TransUInt16(Content, index); index += 2;
            DnHeat = ByteTransform.TransUInt16(Content, index); index += 2;
            Rotation = ByteTransform.TransUInt16(Content, index); index += 2;

            MFC2SW = ByteTransform.TransUInt16(Content, index) != 0; index += 2;
            MFC2SV = ByteTransform.TransUInt16(Content, index); index += 2;

            MFC3SW = ByteTransform.TransUInt16(Content, index) != 0; index += 2;
            MFC3SV = ByteTransform.TransUInt16(Content, index); index += 2;

            CoolTemp = ByteTransform.TransUInt16(Content, index); index += 2;
            ATime = ByteTransform.TransUInt16(Content, index); index += 2;

            BiasSW = ByteTransform.TransUInt16(Content, index) != 0; index += 2;
            BiasKHz = ByteTransform.TransUInt16(Content, index); index += 2;
            BiasDuty = ByteTransform.TransUInt16(Content, index); index += 2;
            BiasThe = ByteTransform.TransUInt16(Content, index); index += 2;
            BiasVolt = ByteTransform.TransUInt16(Content, index); index += 2;

            ARC7SW = ByteTransform.TransUInt16(Content, index) != 0; index += 2;
            ARC7SV = ByteTransform.TransUInt16(Content, index); index += 2;

            ARC8SW = ByteTransform.TransUInt16(Content, index) != 0; index += 2;
            ARC8SV = ByteTransform.TransUInt16(Content, index); index += 2;

            Pluse1SW = ByteTransform.TransUInt16(Content, index) != 0; index += 2;
            Pluse1Curr = ByteTransform.TransUInt16(Content, index); index += 2;
            Pluse1ONtime = ByteTransform.TransUInt16(Content, index); index += 2;
            Pluse1OFFtime = ByteTransform.TransUInt16(Content, index); index += 2;

            Pluse2SW = ByteTransform.TransUInt16(Content, index) != 0; index += 2;
            Pluse2Curr = ByteTransform.TransUInt16(Content, index); index += 2;
            Pluse2ONtime = ByteTransform.TransUInt16(Content, index); index += 2;
            Pluse2OFFtime = ByteTransform.TransUInt16(Content, index); index += 2;

            LowTemp = ByteTransform.TransSingle(Content, index); index += 4;
            HighTemp = ByteTransform.TransSingle(Content, index); index += 4;
            Pressure = ByteTransform.TransSingle(Content, index); index += 4;
            Cool = ByteTransform.TransSingle(Content, index); index += 4;
        }

        // ========== 序列化 (C# -> PLC) ==========
        public byte[] ToSource()
        {
            byte[] buffer = new byte[ReadCount * 2]; // 34字 * 2 = 68字节
            int index = 0;

            ByteTransform.TransByte(UpHeat).CopyTo(buffer, index); index += 2;
            ByteTransform.TransByte(DnHeat).CopyTo(buffer, index); index += 2;
            ByteTransform.TransByte(Rotation).CopyTo(buffer, index); index += 2;

            ByteTransform.TransByte((ushort)(MFC2SW ? 1 : 0)).CopyTo(buffer, index); index += 2;
            ByteTransform.TransByte(MFC2SV).CopyTo(buffer, index); index += 2;

            ByteTransform.TransByte((ushort)(MFC3SW ? 1 : 0)).CopyTo(buffer, index); index += 2;
            ByteTransform.TransByte(MFC3SV).CopyTo(buffer, index); index += 2;

            ByteTransform.TransByte(CoolTemp).CopyTo(buffer, index); index += 2;
            ByteTransform.TransByte(ATime).CopyTo(buffer, index); index += 2;

            ByteTransform.TransByte((ushort)(BiasSW ? 1 : 0)).CopyTo(buffer, index); index += 2;
            ByteTransform.TransByte(BiasKHz).CopyTo(buffer, index); index += 2;
            ByteTransform.TransByte(BiasDuty).CopyTo(buffer, index); index += 2;
            ByteTransform.TransByte(BiasThe).CopyTo(buffer, index); index += 2;
            ByteTransform.TransByte(BiasVolt).CopyTo(buffer, index); index += 2;

            ByteTransform.TransByte((ushort)(ARC7SW ? 1 : 0)).CopyTo(buffer, index); index += 2;
            ByteTransform.TransByte(ARC7SV).CopyTo(buffer, index); index += 2;

            ByteTransform.TransByte((ushort)(ARC8SW ? 1 : 0)).CopyTo(buffer, index); index += 2;
            ByteTransform.TransByte(ARC8SV).CopyTo(buffer, index); index += 2;

            ByteTransform.TransByte((ushort)(Pluse1SW ? 1 : 0)).CopyTo(buffer, index); index += 2;
            ByteTransform.TransByte(Pluse1Curr).CopyTo(buffer, index); index += 2;
            ByteTransform.TransByte(Pluse1ONtime).CopyTo(buffer, index); index += 2;
            ByteTransform.TransByte(Pluse1OFFtime).CopyTo(buffer, index); index += 2;

            ByteTransform.TransByte((ushort)(Pluse2SW ? 1 : 0)).CopyTo(buffer, index); index += 2;
            ByteTransform.TransByte(Pluse2Curr).CopyTo(buffer, index); index += 2;
            ByteTransform.TransByte(Pluse2ONtime).CopyTo(buffer, index); index += 2;
            ByteTransform.TransByte(Pluse2OFFtime).CopyTo(buffer, index); index += 2;

            ByteTransform.TransByte(LowTemp).CopyTo(buffer, index); index += 4;
            ByteTransform.TransByte(HighTemp).CopyTo(buffer, index); index += 4;
            ByteTransform.TransByte(Pressure).CopyTo(buffer, index); index += 4;
            ByteTransform.TransByte(Cool).CopyTo(buffer, index); index += 4;

            return buffer;
        }
    }

}
