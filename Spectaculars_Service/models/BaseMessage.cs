using System;

namespace BJ_MercedesBenz_Spectaculars
{
    /// <summary>
    /// 消息基类
    /// </summary>
    public class BaseMessage
    {
        /// <summary>
        /// 主键
        /// </summary>
        public int id { get; set; }

        /// <summary>
        /// 消息类型
        /// </summary>
        public string messageName { get; set; }

    }
}
