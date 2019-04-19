using System.Collections.Generic;
using System.Linq;

namespace MercedesBenz.Infrastructure
{
    public class BytePackagedis
    {
        private static List<byte> severbyt = new List<byte>();

        /// <summary>
        /// 粘包 断包处理
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static List<byte[]> AnalysisByte(byte[] message)
        {
            List<byte[]> byteList = new List<byte[]>();
            int Msglength = message.Length;
            int byteLength = 25;
            if (Msglength == byteLength)
            {
                if (message[0] == 0x01 && message[1] == 0x03 && message[2] == 0x14) //找到数据帧头
                {
                    byteList.Add(message);
                }
                severbyt.Clear();
            }
            else if (Msglength < byteLength)
            {
                for (int i = 0; i < message.Length; i++)
                {
                    severbyt.Add(message[i]);
                }
                if (severbyt.Count() == byteLength)
                {
                    byteList.Add(severbyt.ToArray());
                    severbyt.Clear();
                }
                else if (severbyt.Count() > byteLength)
                {
                    severbyt.Clear();
                }
            }
            else if (Msglength > byteLength)
            {
                for (int i = 0; i < Msglength; i++)
                {
                    if (message[i] == 0x01 && message[i + 1] == 0x03 && message[i + 2] == 0x14) //找到数据帧头
                    {
                        byte[] mesbyte = new byte[byteLength];
                        int index = i;
                        for (int s = 0; s < byteLength; s++)
                        {
                            if ((Msglength - 1) - index >= 3) //防止断包索引超出
                            {
                                if (message[index] == 0x01 && message[index + 1] == 0x03 && message[index + 2] == 0x14 && index != i) //断包跳出
                                    break;
                            }
                            if ((Msglength - 1) >= index)
                            {
                                mesbyte[s] = message[index];
                                index++;
                            }
                        }
                        byteList.Add(mesbyte);
                    }
                    if (i == 0 && severbyt.Count() > 0)
                    {
                        byte[] mesbyte = new byte[byteLength];
                        int index = 0;
                        for (int m = 0; m < severbyt.Count(); m++)
                        {
                            mesbyte[index] = severbyt[m];
                            index++;
                        }
                        for (int q = 0; q < byteLength - severbyt.Count(); q++)
                        {
                            if (index < byteLength)
                            {
                                mesbyte[index] = message[q];
                                index++;
                            }
                        }
                        byteList.Add(mesbyte);
                    }
                }
                severbyt.Clear();
            }
            return byteList;
        }
    }
}
