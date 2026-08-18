using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HYSD
{
    [SugarTable("TCData")]
    public class TCData
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true, IsNullable = false)] // 主键，自增
        public int ID { get; set; }

        /// <summary>
        /// 时间戳，格式为yyyy-MM-dd HH:mm:ss
        /// </summary>
        public DateTime DateTime { get; set; }

        /// <summary>
        /// 上温度
        /// </summary>
        public float UpHeat { get; set; }

        /// <summary>
        /// 下温度
        /// </summary>
        public float DnHeat { get; set; }

        /// <summary>
        /// 转速
        /// </summary>
        public ushort Rotation { get; set; }

        /// <summary>
        /// N2值
        /// </summary>
        public ushort N2SV { get; set; }

        /// <summary>
        /// H2值
        /// </summary>
        public ushort H2SV { get; set; }

        /// <summary>
        /// Ar值
        /// </summary>
        public ushort ArSV { get; set; }

        /// <summary>
        /// 水温
        /// </summary>
        public float WaterTemp { get; set; }

        /// <summary>
        /// 靶1电流
        /// </summary>
        public float B1Curr { get; set; }

        /// <summary>
        /// 靶1电压
        /// </summary>
        public float B1Volt { get; set; }

        /// <summary>
        /// 靶2电流
        /// </summary>
        public float B2Curr { get; set; }

        /// <summary>
        /// 靶2电压
        /// </summary>
        public float B2Volt { get; set; }

        /// <summary>
        /// 靶3电流
        /// </summary>
        public float B3Curr { get; set; }

        /// <summary>
        /// 靶3电压
        /// </summary>
        public float B3Volt { get; set; }

        /// <summary>
        /// 靶4电流
        /// </summary>
        public float B4Curr { get; set; }

        /// <summary>
        /// 靶4电压
        /// </summary>
        public float B4Volt { get; set; }

        /// <summary>
        /// 靶5电流
        /// </summary>
        public float B5Curr { get; set; }

        /// <summary>
        /// 靶5电压
        /// </summary>
        public float B5Volt { get; set; }

        /// <summary>
        /// 靶6电流
        /// </summary>
        public float B6Curr { get; set; }

        /// <summary>
        /// 靶6电压
        /// </summary>
        public float B6Volt { get; set; }

        /// <summary>
        /// 靶7电流
        /// </summary>
        public float B7Curr { get; set; }

        /// <summary>
        /// 靶7电压
        /// </summary>
        public float B7Volt { get; set; }

        /// <summary>
        /// 靶8电流
        /// </summary>
        public float B8Curr { get; set; }

        /// <summary>
        /// 靶8电压
        /// </summary>
        public float B8Volt { get; set; }

        /// <summary>
        /// 偏压电压
        /// </summary>
        public ushort BiasVolt { get; set; }

        /// <summary>
        /// 偏压电流
        /// </summary>
        public float BiasCurr { get; set; }

        /// <summary>
        /// 脉冲电流1
        /// </summary>
        public float Pluse1Curr { get; set; }

        /// <summary>
        /// 脉冲频率1
        /// </summary>
        public int Pluse1KHz { get; set; }

        /// <summary>
        /// 脉冲占空比1
        /// </summary>
        public float Pluse1Duty { get; set; }

        /// <summary>
        /// 脉冲电流2
        /// </summary>
        public float Pluse2Curr { get; set; }

        /// <summary>
        /// 脉冲频率2
        /// </summary>
        public int Pluse2KHz { get; set; }

        /// <summary>
        /// 脉冲占空比2
        /// </summary>
        public float Pluse2Duty { get; set; }

        /// <summary>
        /// 线圈电流
        /// </summary>
        public float CoilCurr { get; set; }

        /// <summary>
        /// 线圈电压
        /// </summary>
        public float CoilVolt { get; set; }

        /// <summary>
        /// Penning值
        /// </summary>
        public string Penning { get; set; }

        /// <summary>
        /// CDG100值
        /// </summary>
        public string CDG100 { get; set; }

        /// <summary>
        /// Pirani1值
        /// </summary>
        public string Pirani1 { get; set; }

        /// <summary>
        /// Pirani2值
        /// </summary>
        public string Pirani2 { get; set; }

        /// <summary>
        /// 水流量1值
        /// </summary>
        public float Water1 { get; set; }

        /// <summary>
        /// 水流量2值
        /// </summary>
        public float Water2 { get; set; }

        /// <summary>
        /// 水流量3值
        /// </summary>
        public float Water3 { get; set; }

        /// <summary>
        /// 水流量4值
        /// </summary>
        public float Water4 { get; set; }

        /// <summary>
        /// 水流量5值
        /// </summary>
        public float Water5 { get; set; }

        /// <summary>
        /// 水流量6值
        /// </summary>
        public float Water6 { get; set; }

        /// <summary>
        /// 水流量7值
        /// </summary>
        public float Water7 { get; set; }

        /// <summary>
        /// 水流量8值
        /// </summary>
        public float Water8 { get; set; }

        /// <summary>
        /// 水流量9值
        /// </summary>
        public float Water9 { get; set; }
    }
}
