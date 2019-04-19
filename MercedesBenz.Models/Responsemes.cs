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
        public int Id { get; set; }

        /// <summary>
        /// 响应状态
        /// </summary>
        public int comm_state { get; set; }

        /// <summary>
        /// 响应结果
        /// </summary>
        public int result { get; set; }
    }
}
