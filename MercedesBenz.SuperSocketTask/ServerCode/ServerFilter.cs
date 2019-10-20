using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MercedesBenz.SuperSocketTask.ServerCode
{
   public class ServerFilter : ServerReceiveFilter<ServerRequestInfo>
    {
        /// <summary>
        /// 重写方法
        /// </summary>
        /// <param name="readBuffer">过滤之后的数据缓存</param>
        /// <param name="offset">数据起始位置</param>
        /// <param name="length">数据缓存长度</param>
        /// <returns></returns>
        protected override ServerRequestInfo ProcessMatchedRequest(byte[] readBuffer, int offset, int length)
        {
            // Encoding.UTF8.GetString(readBuffer, offset, length)
            //返回构造函数指定的数据格式
            return new ServerRequestInfo(Guid.NewGuid().ToString(), Encoding.Default.GetString(readBuffer), readBuffer);
        }
    }
}
