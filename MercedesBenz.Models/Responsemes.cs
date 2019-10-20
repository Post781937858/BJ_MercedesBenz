using System;

namespace MercedesBenz.Models
{
    /// <summary>
    /// 响应消息
    /// </summary>
    public class Responsemes
    {
        /// <summary>
        /// 主键
        /// </summary>
        public int id { get; set; }

        /// <summary>
        /// 响应状态
        /// </summary>
        public int comm_state { get; set; }

        /// <summary>
        /// 消息类型
        /// </summary>
        public string messageName { get; set; }

        /// <summary>
        /// 响应结果
        /// </summary>
        public int result { get; set; }

        /// <summary>
        /// 箱号ID
        /// </summary>
        public string boxId { get; set; }
    }

    public class ResponsemesUpdate: Responsemes
    {
        /// <summary>
        /// 库区编号
        /// </summary>
        public string areaCode { get; set; }

        public string sourceCode { get; set; }
    }

}
