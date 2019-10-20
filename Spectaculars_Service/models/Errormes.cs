using System;

namespace BJ_MercedesBenz_Spectaculars
{
    public class Errormes : BaseMessage
    {
        /// <summary>
        /// 异常设备ID
        /// </summary>
        public string equipment { get; set; }

        /// <summary>
        /// 异常消息
        /// </summary>
        public string errormessage { get; set; }

        /// <summary>
        /// 错误代码
        /// </summary>
        public int errorcore { get; set; }

    }
}
