using System;

namespace MercedesBenz.Models
{
    /// <summary>
    /// 消息基类
    /// </summary>
    public class BaseMessage
    {
        /// <summary>
        /// 主键
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 消息类型
        /// </summary>
        public string messageName { get; set; }

        /// <summary>
        /// 请求唯一标识
        /// </summary>
        public int soleId { get; set; }

    }
}
