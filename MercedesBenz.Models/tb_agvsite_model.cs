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
    /// 描 述：tb_agvsite_dal 实体 
    /// </summary> 
    public partial class tb_agvsite_model
    {
        
        /// <summary>
        /// ID
        /// </summary>
        public int site_id { get; set; }
        
        /// <summary>
        /// 站点编号
        /// </summary>
        public int site_number { get; set; }
        
        /// <summary>
        /// agv编号
        /// </summary>
        public int site_agvnumber { get; set; }
        
        /// <summary>
        /// agv原点站点
        /// </summary>
        public int site_agvsite { get; set; }
        
    }
}