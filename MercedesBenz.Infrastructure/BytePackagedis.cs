using System;
using System.Collections.Generic;
using System.Linq;

namespace MercedesBenz.Infrastructure
{
    public class BytePackagedis
    {
        /// <summary>
        /// PLC Modbus解包 
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static List<byte[]> AnalysisByte(byte[] message)
        {
            List<byte[]> ListByte = new List<byte[]>();
            if (message.Length < 8)
            {
                return ListByte;
            }
            for (int i = 0; i < message.Length; i++)
            {
                if (message.Length - i >= 8)
                {
                    if (message[i + 2] == 0x00 && message[i + 3] == 0x00 && message[i + 7] == 0x03)
                    {
                        int index = i;
                        int byteLength = 6 + message[i + 5];
                        byte[] byt_message = new byte[byteLength];
                        if (message.Length - index >= byteLength)
                        {
                            for (int s = 0; s < byteLength; s++)
                            {
                                byt_message[s] = message[index];
                                index++;
                            }
                            ListByte.Add(byt_message);
                        }
                    }
                    else if (message[i + 2] == 0x00 && message[i + 3] == 0x00 && message[i + 7] == 0x10)
                    {
                        int index = i;
                        int byteLength = 6 + message[i + 5];
                        byte[] byt_message = new byte[byteLength];
                        if (message.Length - index >= byteLength)
                        {
                            for (int s = 0; s < byteLength; s++)
                            {
                                byt_message[s] = message[index];
                                index++;
                            }
                            ListByte.Add(byt_message);
                        }
                    }
                }
            }
            return ListByte;
        }

        public static List<byte[]> AnalysisSwitchByte(byte[] message)
        {
            List<byte[]> ListByte = new List<byte[]>();
            if (message.Length < 8)
            {
                return ListByte;
            }
            for (int i = 0; i < message.Length; i++)
            {
                if (message.Length - i >= 8)
                {
                    if (message[i] == 0x68 && message[i + 1] == 0x00 && message[i + 2] == 0x00)
                    {
                        int index = i;
                        int byteLength = 4 + message[i + 3] + 1;
                        byte[] byt_message = new byte[byteLength];
                        if (message.Length - index >= byteLength)
                        {
                            for (int s = 0; s < byteLength; s++)
                            {
                                byt_message[s] = message[index];
                                index++;
                            }
                            ListByte.Add(byt_message);
                        }
                    }
                }
            }
            return ListByte;
        }


        /// <summary>
        /// NDC解包
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static List<byte[]> Unbind_Byte(byte[] message)
        {
            List<byte[]> ListByte = new List<byte[]>();
            if (message.Length < 8)
            {
                return ListByte;
            }
            for (int i = 0; i < message.Length; i++)
            {
                if (message.Length - i >= 8)
                {
                    if (message[i] == 0x87 && message[i + 1] == 0xCD && message[i + 3] == 0x08 && message[i + 7] == 0x01)
                    {
                        int index = i;
                        int byteLength = 8 + message[i + 5];
                        byte[] byt_message = new byte[byteLength];
                        if (message.Length - index >= byteLength)
                        {
                            for (int s = 0; s < byteLength; s++)
                            {
                                byt_message[s] = message[index];
                                index++;
                            }
                            ListByte.Add(byt_message);
                        }
                    }
                }
            }
            return ListByte;
        }
    }
}
