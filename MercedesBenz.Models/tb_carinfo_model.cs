using System;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace MercedesBenz.Models
{
    /*代码自动生成工具自动生成,不要在这里写代码，否则会被自动覆盖哦 */

	/// <summary> 
    /// 版 本 V1.0.1 代码快速生成工具 
    /// 创 建：代码生成工具 
    /// 日 期：2019/8/23 14:17:54 
    /// 描 述：tb_carinfo_dal 实体 
    /// </summary> 
    public partial class tb_carinfo_model
    {
        
        /// <summary>
        /// ID主键
        /// </summary>
        public int Info_ID { get; set; }
        
        /// <summary>
        /// 编号
        /// </summary>
        public int Info_number { get; set; }
        
        /// <summary>
        /// 类型
        /// </summary>
        public string Info_cartype { get; set; }
        
        /// <summary>
        /// 电压
        /// </summary>
        public double Info_voltage { get; set; }
        
        /// <summary>
        /// 报警信息
        /// </summary>
        public string Info_errormes { get; set; }
        
        /// <summary>
        /// 任务状态
        /// </summary>
        public string Info_status { get; set; }
        
        /// <summary>
        /// 是否充电
        /// </summary>
        public int Info_Ischarge { get; set; }
        
        /// <summary>
        /// 是否报警
        /// </summary>
        public int Info_Iserror { get; set; }
        
        /// <summary>
        /// 当前站点
        /// </summary>
        public int Info_ThisSite { get; set; }
        
    }
}