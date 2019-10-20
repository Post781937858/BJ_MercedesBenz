using SuperSocket.ProtoBase;

namespace MercedesBenz.SuperSocketTask.ClientCode
{
    public class BaseIPackageInfo : IPackageInfo
    {
        public BaseIPackageInfo(byte[] BodyByte, string BodyText)
        {
            this.BodyByte = BodyByte;
            this.BodyText = BodyText;
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
