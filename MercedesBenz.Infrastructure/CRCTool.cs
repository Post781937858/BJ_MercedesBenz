using TIPSCRCClass;

namespace MercedesBenz.Infrastructure
{
    public static class CRCTool
    {
        private static TIPSDll tIPSDll = new TIPSDll();

        /// <summary>
        /// 添加CRC校验
        /// </summary>
        /// <param name="bStrs"></param>
        /// <returns></returns>
        public static byte[] CalCrc(this byte[] bStrs)
        {
            int count = bStrs.Length;
            byte[] bStr = bStrs;
            byte[] getByte = new byte[count];
            for (int i = 0; i < getByte.Length - 2; i++)
            {
                getByte[i] = bStr[i];
            }
            int[] crc = tIPSDll.AGV_CRC16(getByte, getByte.Length - 2);
            bStr[bStr.Length - 2] = (byte)crc[1];
            bStr[bStr.Length - 1] = (byte)crc[0];
            return bStr;
        }

        /// <summary>
        /// CRC校验
        /// </summary>
        /// <param name="bStr"></param>
        /// <returns></returns>
        public static bool CheckStrCrc(this byte[] bStr)
        {
            int[] crc = tIPSDll.AGV_CRC16(bStr, bStr.Length - 2);
            return (bStr[bStr.Length - 2] == (byte)crc[1] && bStr[bStr.Length - 1] == (byte)crc[0]);
        }
    }
}
