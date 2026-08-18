using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HYSD
{
    // ================= 主表：配方基本信息 =================
    [SugarTable("HeatRecipeMain")]
    public class HeatRecipeMain
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true,IsNullable = false)] // 主键，自增
        public int RecipeID { get; set; }


        [SugarColumn(Length = 50, IsNullable = false, UniqueGroupNameList = new[] { "Idx_ HeatRecipe_Name" })]
        public string RecipeName { get; set; }

        // 【核心】导航属性：告诉 SqlSugar，通过 RecipeID 和子表关联
        // OneToMany 代表一对多
        [Navigate(NavigateType.OneToMany, nameof(HeatRecipeDataSet.RecipeID))]
        public List<HeatRecipeDataSet> DataSets { get; set; }
    }

    // ================= 子表：配方数据集 =================
    [SugarTable("HeatRecipeDataSet")]
    public class HeatRecipeDataSet
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true, IsNullable = false)] // 主键，自增
        public int ID { get; set; }

        // 外键：关联主表的 RecipeID
        [SugarColumn(IsNullable = false)]
        public int RecipeID { get; set; }

        [SugarColumn(IsNullable = false)]
        public int SetIndex { get; set; } // 组号 1-15
        /// <summary>
        /// 上温度
        /// </summary>
        public ushort UpHeat { get; set; }  // 上温度
        /// <summary>
        /// 下温度
        /// </summary>
        public ushort DnHeat { get; set; }  // 下温度
        /// <summary>
        /// 转速
        /// </summary>
        public ushort Rotation { get; set; } // 转速
        /// <summary>
        /// MFC2开关
        /// </summary>
        public bool MFC2SW { get; set; } // MFC2开关
        /// <summary>
        /// MFC2值
        /// </summary>
        public ushort MFC2SV { get; set; } // MFC2值
        /// <summary>
        /// MFC3开关
        /// </summary>
        public bool MFC3SW { get; set; } // MFC3开关
        /// <summary>
        /// MFC3值
        /// </summary>
        public ushort MFC3SV { get; set; } // MFC3值
        /// <summary>
        /// ARC7开关
        /// </summary>
        public bool ARC7SW { get; set; } // ARC7开关
        /// <summary>
        /// ARC7值
        /// </summary>
        public ushort ARC7SV { get; set; } // ARC7值
        /// <summary>
        /// ARC8开关
        /// </summary>
        public bool ARC8SW { get; set; } // ARC8开关
        /// <summary>
        /// ARC8值
        /// </summary>
        public ushort ARC8SV { get; set; } // ARC8值
        /// <summary>
        /// 冷却温度
        /// </summary>
        public ushort CoolTemp { get; set; } // 冷却温度
        /// <summary>
        /// 时间
        /// </summary>
        public ushort HTime { get; set; } // 时间
        /// <summary>
        /// 大于低温设定值
        /// </summary>
        public float LowTemp { get; set; } // 大于低温设定值
        /// <summary>
        /// 小于高温设定值
        /// </summary>
        public float HighTemp { get; set; }// 小于高温设定值
        /// <summary>
        /// 小于真空压力
        /// </summary>
        public float Pressure { get; set; } // 小于真空压力
        /// <summary>
        /// 小于冰水机温度值
        /// </summary>
        public float Cool { get; set; }//小于冰水机温度值
    }


    // ================= 主表：配方基本信息 =================
    [SugarTable("QtksRecipeMain")]
    public class QtksRecipeMain
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true, IsNullable = false)] // 主键，自增
        public int RecipeID { get; set; }


        [SugarColumn(Length = 50, IsNullable = false, UniqueGroupNameList = new[] { "Idx_ QtksRecipe_Name" })]
        public string RecipeName { get; set; }

        // 【核心】导航属性：告诉 SqlSugar，通过 RecipeID 和子表关联
        // OneToMany 代表一对多
        [Navigate(NavigateType.OneToMany, nameof(QtksRecipeDataSet.RecipeID))]
        public List<QtksRecipeDataSet> DataSets { get; set; }
    }

    // ================= 子表：配方数据集 =================
    [SugarTable("QtksRecipeDataSet")]
    public class QtksRecipeDataSet
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true, IsNullable = false)] // 主键，自增
        public int ID { get; set; }

        // 外键：关联主表的 RecipeID
        [SugarColumn(IsNullable = false)]
        public int RecipeID { get; set; }

        [SugarColumn(IsNullable = false)]
        public int SetIndex { get; set; } // 组号 1-15
        /// <summary>
        /// 上温度
        /// </summary>
        public ushort UpHeat { get; set; }  // 上温度
        /// <summary>
        /// 下温度
        /// </summary>
        public ushort DnHeat { get; set; }  // 下温度
        /// <summary>
        /// 转速
        /// </summary>
        public ushort Rotation { get; set; } // 转速
        /// <summary>
        /// H2开关
        /// </summary>
        public bool MFC2SW { get; set; } // MFC2开关
        /// <summary>
        /// H2值
        /// </summary>
        public ushort MFC2SV { get; set; } // MFC2值
        /// <summary>
        /// Ar开关
        /// </summary>
        public bool MFC3SW { get; set; } // MFC3开关
        /// <summary>
        /// Ar值
        /// </summary>
        public ushort MFC3SV { get; set; } // MFC3值
        /// <summary>
        /// 冷却温度
        /// </summary>
        public ushort CoolTemp { get; set; } // 冷却温度
        /// <summary>
        /// 时间
        /// </summary>
        public ushort ATime { get; set; } // 时间
        /// <summary>
        /// 偏压开关
        /// </summary>
        public bool BiasSW { get; set; }//偏压开关
        /// <summary>
        /// 偏压频率
        /// </summary>
        public ushort BiasKHz { get; set; }//偏压频率
        /// <summary>
        /// 偏压占空比
        /// </summary>
        public ushort BiasDuty { get; set; }//偏压占空比
        /// <summary>
        /// 偏压电流阈值
        /// </summary>
        public ushort BiasThe { get; set; }//偏压电流阈值
        /// <summary>
        /// 偏压电压
        /// </summary>
        public ushort BiasVolt { get; set; }//偏压电压
        /// <summary>
        /// ARC7开关
        /// </summary>
        public bool ARC7SW { get; set; } // ARC7开关
        /// <summary>
        /// ARC7值
        /// </summary>
        public ushort ARC7SV { get; set; } // ARC7值
        /// <summary>
        /// ARC8开关
        /// </summary>
        public bool ARC8SW { get; set; } // ARC8开关
        /// <summary>
        /// ARC8值
        /// </summary>
        public ushort ARC8SV { get; set; } // ARC8值
        /// <summary>
        /// 脉冲1开关
        /// </summary>
        public bool Pluse1SW { get; set; } // 脉冲1开关
        /// <summary>
        /// 脉冲1电流设定
        /// </summary>
        public ushort Pluse1Curr { get; set; }//脉冲1电流设定
        /// <summary>
        /// 脉冲1开通时间
        /// </summary>
        public ushort Pluse1ONtime { get; set; }//脉冲1开通时间
        /// <summary>
        /// 脉冲1关断时间
        /// </summary>
        public ushort Pluse1OFFtime { get; set; }//脉冲1关断时间
        /// <summary>
        /// 脉冲2开关
        /// </summary>
        public bool Pluse2SW { get; set; } // 脉冲2开关
        /// <summary>
        /// 脉冲2电流设定
        /// </summary>
        public ushort Pluse2Curr { get; set; }//脉冲2电流设定
        /// <summary>
        /// 脉冲2开通时间
        /// </summary>
        public ushort Pluse2ONtime { get; set; }//脉冲2开通时间
        /// <summary>
        /// 脉冲2关断时间
        /// </summary>
        public ushort Pluse2OFFtime { get; set; }//脉冲2关断时间
        /// <summary>
        /// 大于低温设定值
        /// </summary>
        public float LowTemp { get; set; } // 大于低温设定值
        /// <summary>
        /// 小于高温设定值
        /// </summary>
        public float HighTemp { get; set; }// 小于高温设定值
        /// <summary>
        /// 小于真空压力
        /// </summary>
        public float Pressure { get; set; } // 小于真空压力
        /// <summary>
        /// 小于冰水机温度值
        /// </summary>
        public float Cool { get; set; }//小于冰水机温度值
    }

    // ================= 主表：配方基本信息 =================
    [SugarTable("TCRecipeMain")]
    public class TCRecipeMain
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true, IsNullable = false)] // 主键，自增
        public int RecipeID { get; set; }


        [SugarColumn(Length = 50, IsNullable = false, UniqueGroupNameList = new[] { "Idx_ TCRecipe_Name" })]
        public string RecipeName { get; set; }

        // 【核心】导航属性：告诉 SqlSugar，通过 RecipeID 和子表关联
        // OneToMany 代表一对多
        [Navigate(NavigateType.OneToMany, nameof(TCRecipeDataSet.RecipeID))]
        public List<TCRecipeDataSet> DataSets { get; set; }
    }

    // ================= 子表：配方数据集 =================
    [SugarTable("TCRecipeDataSet")]
    public class TCRecipeDataSet
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true, IsNullable = false)] // 主键，自增
        public int ID { get; set; }

        // 外键：关联主表的 RecipeID
        [SugarColumn(IsNullable = false)]
        public int RecipeID { get; set; }

        [SugarColumn(IsNullable = false)]
        public int SetIndex { get; set; } // 组号 1-30
        /// <summary>
        /// 上温度
        /// </summary>
        public ushort UpHeat { get; set; }  // 上温度
        /// <summary>
        /// 下温度
        /// </summary>
        public ushort DnHeat { get; set; }  // 下温度
        /// <summary>
        /// 转速
        /// </summary>
        public ushort Rotation { get; set; } // 转速
        /// <summary>
        ///N2开关
        /// </summary>
        public bool MFC1SW { get; set; } // MFC1开关
        /// <summary>
        /// N2值
        /// </summary>
        public ushort MFC1SV { get; set; } // MFC1值
        /// <summary>
        /// H2开关
        /// </summary>
        public bool MFC2SW { get; set; } // MFC2开关
        /// <summary>
        /// H2值
        /// </summary>
        public ushort MFC2SV { get; set; } // MFC2值
        /// <summary>
        /// Ar开关
        /// </summary>
        public bool MFC3SW { get; set; } // MFC3开关
        /// <summary>
        /// Ar值
        /// </summary>
        public ushort MFC3SV { get; set; } // MFC3值
        /// <summary>
        /// X开关
        /// </summary>
        public bool MFC4SW { get; set; } // MFC4开关
        /// <summary>
        /// X值
        /// </summary>
        public ushort MFC4SV { get; set; } // MFC4值
        /// <summary>
        /// 冷却温度
        /// </summary>
        public ushort CoolTemp { get; set; } // 冷却温度
        /// <summary>
        /// 时间
        /// </summary>
        public ushort CTime { get; set; } // 时间
        /// <summary>
        /// 偏压开关
        /// </summary>
        public bool BiasSW { get; set; }//偏压开关
        /// <summary>
        /// 偏压频率
        /// </summary>
        public ushort BiasKHz { get; set; }//偏压频率
        /// <summary>
        /// 偏压电压
        /// </summary>
        public ushort BiasVolt { get; set; }//偏压电压
        /// <summary>
        /// ARC1开关
        /// </summary>
        public bool ARC1SW { get; set; } // ARC1开关
        /// <summary>
        /// ARC1值
        /// </summary>
        public ushort ARC1SV { get; set; } // ARC1值
        /// <summary>
        /// ARC2开关
        /// </summary>
        public bool ARC2SW { get; set; } // ARC2开关
        /// <summary>
        /// ARC2值
        /// </summary>
        public ushort ARC2SV { get; set; } // ARC2值
        /// <summary>
        /// ARC3开关
        /// </summary>
        public bool ARC3SW { get; set; } // ARC3开关
        /// <summary>
        /// ARC3值
        /// </summary>
        public ushort ARC3SV { get; set; } // ARC3值
        /// <summary>
        /// ARC4开关
        /// </summary>
        public bool ARC4SW { get; set; } // ARC4开关
        /// <summary>
        /// ARC4值
        /// </summary>
        public ushort ARC4SV { get; set; } // ARC4值
        /// <summary>
        /// ARC5开关
        /// </summary>
        public bool ARC5SW { get; set; } // ARC5开关
        /// <summary>
        /// ARC5值
        /// </summary>
        public ushort ARC5SV { get; set; } // ARC5值
        /// <summary>
        /// ARC6开关
        /// </summary>
        public bool ARC6SW { get; set; } // ARC6开关
        /// <summary>
        /// ARC6值
        /// </summary>
        public ushort ARC6SV { get; set; } // ARC6值
        /// <summary>
        /// 线圈开关
        /// </summary>
        public bool CoilSW { get; set; }//线圈开关
        /// <summary>
        /// 偏压占空比
        /// </summary>
        public ushort BiasDuty { get; set; }//偏压占空比
        /// <summary>
        /// 偏压电流阈值
        /// </summary>
        public ushort BiasThe { get; set; }//偏压电流阈值
        /// <summary>
        /// 线圈高电流
        /// </summary>
        public ushort CoilH { get; set; }//线圈高电流
        /// <summary>
        /// 线圈T0
        /// </summary>
        public ushort CoilT0 { get; set; }//线圈T0
        /// <summary>
        /// 线圈T1
        /// </summary>
        public ushort CoilT1 { get; set; }//线圈T1
        /// <summary>
        /// 线圈低电流
        /// </summary>
        public ushort CoilL { get; set; }//线圈低电流
        /// <summary>
        /// 线圈T2
        /// </summary>
        public ushort CoilT2 { get; set; }//线圈T2
        /// <summary>
        /// 线圈T3
        /// </summary>
        public ushort CoilT3 { get; set; }//线圈T3
        /// <summary>
        /// 压力/流量控制
        /// </summary>
        public bool PF { get; set; }//压力/流量控制
        /// <summary>
        /// 薄膜规设定
        /// </summary>
        public float CDG100DSV { get; set; }//薄膜规设定
        /// <summary>
        /// 大于低温设定值
        /// </summary>
        public float LowTemp { get; set; } // 大于低温设定值
        /// <summary>
        /// 小于高温设定值
        /// </summary>
        public float HighTemp { get; set; }// 小于高温设定值
        /// <summary>
        /// 小于真空压力
        /// </summary>
        public float Pressure { get; set; } // 小于真空压力
        /// <summary>
        /// 小于冰水机温度值
        /// </summary>
        public float Cool { get; set; }//小于冰水机温度值
    }
}
