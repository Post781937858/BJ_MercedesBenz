using SuperSocket.Facility.Protocol;
using SuperSocket.ProtoBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MercedesBenz.SuperSocketTask.ClientCode
{
    /// <summary>
    /// 过滤器
    /// </summary>
    public class ReceiveFilterClient : IReceiveFilter<BaseIPackageInfo>
    {
        /// <summary>
        /// 数据解析
        /// </summary>
        /// <param name="data">数据缓冲区</param>
        /// <param name="rest">缓冲区剩余数据</param>
        public BaseIPackageInfo Filter(BufferList data, out int rest)
        {
            rest = 0;
            var buffStream = new BufferStream();
            buffStream.Initialize(data);
            byte[] commandData = new byte[data.Total];
            buffStream.Read(commandData, 0, data.Total);
            return new BaseIPackageInfo(commandData, Encoding.Default.GetString(commandData));
        }

        public void Reset()
        {
            NextReceiveFilter = null;
            State = FilterState.Normal;
        }

        public IReceiveFilter<BaseIPackageInfo> NextReceiveFilter { get; protected set; }
        public FilterState State { get; protected set; }
    }
}
