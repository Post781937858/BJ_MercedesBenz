using SuperSocket.SocketBase.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MercedesBenz.SuperSocketTask.ServerCode
{
    public class ServerRequestInfo : IRequestInfo 
    {
        public ServerRequestInfo(string key, string BodyText, byte[] BodyByte)
        {
            this.BodyText = BodyText;
            this.BodyByte = BodyByte;
            this.Key = key;
        }

        public string Key
        {
            get; set;
        }

        /// <summary>
        /// 请求原始数据帧
        /// </summary>
        public byte[] BodyByte { get; set; }

        /// <summary>
        /// 请求数据帧文本
        /// </summary>
        public string BodyText { get; set; }
    }
}
