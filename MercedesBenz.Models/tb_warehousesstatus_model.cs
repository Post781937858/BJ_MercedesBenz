using System;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace MercedesBenz.Models
{
    /*代码自动生成工具自动生成,不要在这里写代码，否则会被自动覆盖哦 */

	/// <summary> 
    /// 版 本 V1.0.1 代码快速生成工具 
    /// 创 建：代码生成工具 
    /// 日 期：2019/8/28 15:58:34 
    /// 描 述：tb_warehousesstatus_dal 实体 
    /// </summary> 
    public partial class tb_warehousesstatus_model
    {
        
        /// <summary>
        /// ID
        /// </summary>
        public int wh_Id { get; set; }
        
        /// <summary>
        /// 类型
        /// </summary>
        public string wh_type { get; set; }
        
        /// <summary>
        /// 空库率
        /// </summary>
        public int wh_empty { get; set; }
        
        /// <summary>
        /// 满库率
        /// </summary>
        public int wh_full { get; set; }
        
    }
}