using MercedesBenz.Models;
using MercedesBenz.Infrastructure;
using System.Text;
using System;
using System.Linq;
using System.Collections.Generic;

namespace MercedesBenz.SuperSocketTask
{
    public class GroupMessage
    {
        private static Dictionary<int, int> mesIndex = new Dictionary<int, int>(); //心跳计数
        static GroupMessage()
        {
            for (int i = 1; i < 6; i++)
            {
                mesIndex.Add(i, 0);
            }
        }

        #region NDC消息

        /// <summary>
        /// 组Q消息
        /// </summary>
        /// <param name="_orderTask"></param>
        /// <returns></returns>
        public static byte[] GroupMessage_Q(OrderTask _orderTasks, int IkeyMax, int IkeyMin)
        {
            byte[] Q_mes = new byte[22];
            Q_mes[0] = 0x87; Q_mes[1] = 0xCD;
            Q_mes[2] = 0x00; Q_mes[3] = 0x08;
            Q_mes[4] = 0x00; Q_mes[5] = 0x0E;
            Q_mes[6] = 0x00; Q_mes[7] = 0x01;
            Q_mes[8] = 0x00; Q_mes[9] = 0x71;
            Q_mes[10] = 0x00; Q_mes[11] = 0x0A;
            Q_mes[12] = 0x01; Q_mes[13] = 0x00;
            Q_mes[14] = 0x00; Q_mes[15] = 0x01;
            Q_mes[16] = 0x00; Q_mes[17] = 0x00;
            Q_mes[18] = 0x00; Q_mes[19] = 0x00;
            Q_mes[20] = 0x00; Q_mes[21] = 0x00;
            Q_mes[13] = 0x80; //优先级
            Q_mes[16] = Convert.ToByte(new Random().Next(0, 255)); //ikey 高位随机

            if (IkeyMin > 1)    //判断最小ikey重复利用
            {
                Q_mes[17] = Convert.ToByte(IkeyMin - 1); //ikey大于1  则减1
            }
            else
            {
                Q_mes[17] = Convert.ToByte(IkeyMax + 1); //ikey 最大ikey加1
            }
            string Fetchdot = string.Empty;
            string Putdot = string.Empty;
            if (_orderTasks.order_getLocation != null && _orderTasks.order_putLocation != null)
            {
                Fetchdot = _orderTasks.order_getLocation;
                Putdot = _orderTasks.order_putLocation;
                if (_orderTasks.order_type != OrderTaskType.In)
                {
                    Fetchdot = GoodsAllocationAnalysis(_orderTasks.order_getLocation.Trim()); //取货位置
                }
                if (_orderTasks.order_type != OrderTaskType.Out)
                {
                    Putdot = GoodsAllocationAnalysis(_orderTasks.order_putLocation.Trim()); //放货位置
                }
            }
            if (!string.IsNullOrEmpty(Fetchdot) && !string.IsNullOrEmpty(Putdot))
            {
                Q_mes[18] = Convert.ToByte((Convert.ToInt32(Fetchdot) & 0xFF00) >> 8); //取货点
                Q_mes[19] = Convert.ToByte(Convert.ToInt32(Fetchdot) & 0x00FF);  //取货点
                Q_mes[20] = Convert.ToByte((Convert.ToInt32(Putdot) & 0xFF00) >> 8); //放货点
                Q_mes[21] = Convert.ToByte(Convert.ToInt32(Putdot) & 0x00FF); //放货点
            }
            return Q_mes;
        }

        /// <summary>
        /// 解析货位为NDC可识别货位
        /// </summary>
        /// <param name="Location"></param>
        public static string GoodsAllocationAnalysis(string Location)
        {
            char[] charText;
            if (Location.Contains("F"))
            {
                charText = Location.Remove(0, 1).PadLeft(8, '0').ToCharArray();
            }
            else
            {
                charText = Location.PadLeft(8, '0').ToCharArray();
            }
            string locationChar = string.Empty;
            if (Convert.ToInt32(charText[0].ToString() + charText[1].ToString()) < 15)
            {
                locationChar = ((Convert.ToInt32(charText[2].ToString() + charText[3].ToString()) * 2 + Convert.ToInt32(charText[6].ToString() + charText[7].ToString())) - 2).ToString().PadLeft(2, '0');
            }
            else
            {
                locationChar = ((Convert.ToInt32(charText[2].ToString() + charText[3].ToString()) * 3 + Convert.ToInt32(charText[6].ToString() + charText[7].ToString())) - 3).ToString().PadLeft(2, '0');
            }
            string Sitelocation = charText[0].ToString() + charText[1].ToString() + locationChar + Convert.ToInt32(charText[4].ToString() + charText[5].ToString());
            return Sitelocation;
        }

        /// <summary>
        /// 组M消息 更新局部参数
        /// </summary>
        /// <param name="S_mes"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public static byte[] GroupMessage_M(byte[] S_mes, Func<byte[], byte[]> func)
        {
            byte[] M_message = { 0x87, 0xCD, 0x00, 0x08, 0x00, 0x0A, 0x00, 0x01, 0x00, 0x6D, 0x00, 0x06, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00 };
            M_message[12] = S_mes[12];  //index
            M_message[13] = S_mes[13];  //index
            return func.Invoke(M_message);
        }

        /// <summary>
        /// 删除任务
        /// </summary>
        /// <param name="S_mes"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public static byte[] DeleteTask(int Index)
        {
            byte[] M_message = { 0x87, 0xCD, 0x00, 0x08, 0x00, 0x06, 0x00, 0x01, 0x00, 0x6E, 0x00, 0x02, 0x00, 0x00 };
            M_message[12] = Convert.ToByte((Index & 0xFF00) >> 8);  //index
            M_message[13] = Convert.ToByte(Convert.ToInt32(Index) & 0x00FF);   //index
            return M_message;
        }


        #endregion

        public static int TaskboxType(string Location)
        {
            int boxtype = 0;
            if (Location.Contains("F"))
            {
                boxtype = Location.Remove(0, 1).PadLeft(8, '0').Substring(0, 2).ToInt() > 15 ? 2 : 1;
            }
            else
            {
                boxtype = Location.PadLeft(8, '0').Substring(0, 2).ToInt() > 15 ? 2 : 1;
            }
            return boxtype;
        }

        #region WCS消息

        /// <summary>
        /// 组WCS状态更新消息
        /// </summary>
        /// <param name="_orderTask"></param>
        /// <param name="_status"></param>
        /// <returns></returns>
        public static UpdateOrderTask GroupMessage_WCS(OrderTask _orderTask, int _status)
        {
            UpdateOrderTask updateOrder = new UpdateOrderTask()
            {
                areaCode = SystemConfiguration.arecore,
                id = new Random().Next(1, 100),  //ID是否固定待定
                messageName = "updateMovement",
                boxId = _orderTask.order_boxId,
                wmsId = _orderTask.order_wmsId,
                sourceCode = SystemConfiguration.sourceCode,
                state = _status
            };
            //if (_orderTask.order_type == OrderTaskType.In) //入库
            //{
            //    updateOrder.position = new UpdateTaskPosition()
            //    {
            //        e_location = _orderTask.order_putLocation
            //    };
            //}
            //else if (_orderTask.order_type == OrderTaskType.Out) //出库
            //{
            //    updateOrder.position = new UpdateTaskPosition()
            //    {
            //        s_location = _orderTask.order_getLocation,
            //        e_location=_orderTask.order_putLocation
            //    };
            //}
            //else if (_orderTask.order_type == OrderTaskType.Interior) //库内搬运
            //{
            updateOrder.location = new UpdateTaskPosition()
            {
                s_level = 0,
                e_level = 0,
                r_level = 0,
                r_location = "",
                s_location = _orderTask.order_getLocation,
                e_location = _orderTask.order_putLocation
            };
            //}

            return updateOrder;
        }

        /// <summary>
        /// 实体转Byte[]
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Message"></param>
        /// <returns></returns>
        public static byte[] MessageToByte<T>(T Message) where T : class
        {
            return Encoding.Default.GetBytes(Message.ToJson() + "\n");
        }


        #endregion

        #region AGV消息

        /// <summary>
        /// 组AGV任务消息
        /// </summary>
        /// <param name="_orderTasks"></param>
        /// <param name="agvNumber">AGV编号</param>
        /// <param name="TaskType">任务类型</param>
        /// <returns></returns>
        public static byte[] GroupMessage_AGV(OrderTask _orderTasks, int agvNumber, int TaskType)
        {
            byte[] message_agv = new byte[20];
            message_agv[0] = 0x00;
            message_agv[1] = 0x00;
            message_agv[2] = 0x00;
            message_agv[3] = 0x00;
            message_agv[5] = 0x09;
            message_agv[6] = 0x00;
            message_agv[7] = 0x10;   //功能码
            message_agv[8] = 0x00;
            message_agv[9] = 0x14;   //寄存器地址
            message_agv[10] = 0x00;
            message_agv[11] = 0x04;   //寄存器数量
            message_agv[12] = 0x08;   //数据长度
            message_agv[13] = 0x00;   //启动和方向
            message_agv[14] = Convert.ToByte(agvNumber); //agv编号
            message_agv[15] = 0x00;  //扫描区域
            message_agv[16] = 0x00;  //速度
            message_agv[17] = 0x00;  // N/A
            message_agv[18] = 0x00;  //任务类型
            message_agv[19] = 0x00;  //出发地
            message_agv[20] = 0x00;  //目的地
            return message_agv;
        }

        /// <summary>
        /// 组AGV信息查询消息
        /// </summary>
        /// <returns></returns>
        public static byte[] GroupMessage_agvQuery()
        {
            byte[] Query = new byte[12];
            Query[0] = 0x00;
            Query[1] = 0x00;
            Query[2] = 0x00;
            Query[3] = 0x00;
            Query[5] = 0x06;
            Query[6] = 0x00;
            Query[7] = 0x03;   //功能码
            Query[8] = 0x00;
            Query[9] = 0x00;   //寄存器地址
            Query[10] = 0x00;
            Query[11] = 0x0A;   //寄存器数量
            return Query;
        }


        /// <summary>
        /// 写请求响应
        /// </summary>
        /// <returns></returns>
        public static byte[] responsePLCInfo(byte[] message)
        {
            byte[] responsePLC = { message[0], message[1], 0x00, 0x00, 0x00, 0x06, message[6], 0x10, message[8], message[9], message[10], message[11] };
            //ConsoleLogHelper.WriteSucceedLog(string.Join(" ", responsePLC.ToList().Select(s => s.ToString("X2"))));
            return responsePLC;
        }

        private static  int Hear = 0;

        public static byte[] Heartbeat(byte[] message)
        {
            byte[] byteinfo = new byte[10];
            byteinfo[0] = message[0];
            byteinfo[1] = message[1];
            byteinfo[2] = 0x00;
            byteinfo[3] = 0x00;
            byteinfo[4] = 0x00;
            byteinfo[5] = 0x04; //从此位开始后数据帧长度
            byteinfo[6] = 0xFF;
            byteinfo[7] = 0x03;    //功能码
            byteinfo[8] = 0x01;   //数据长度
            if (Hear == 0)
            {
                byteinfo[9] = 0x00;
                Hear = 1;
            }
            else
            {
                byteinfo[9] = 0x01;
                Hear = 0;
            }
            return byteinfo;
        }

       

        public static byte[] agvTask(OrderTask orderTask, byte[] message, int StopStatus,int agvNumber)
        {
            byte[] agvOrderTask = new byte[24];
            agvOrderTask[0] = message[0];
            agvOrderTask[1] = message[1];
            agvOrderTask[2] = 0x00;
            agvOrderTask[3] = 0x00;
            agvOrderTask[4] = 0x00;
            agvOrderTask[5] = 0x12; //从此位开始后数据帧长度
            agvOrderTask[6] = 0xFF;
            agvOrderTask[7] = 0x03;    //功能码
            agvOrderTask[8] = 0x0F;   //数据长度

            if (mesIndex[agvNumber] == 0)
            {
                agvOrderTask[9] = 0x00;
                mesIndex[agvNumber] = 1;
            }
            else
            {
                agvOrderTask[9] = 0x01;
                mesIndex[agvNumber] = 0;
            }
            agvOrderTask[10] = 0x01;

            string boxType = string.Empty;
            if (!string.IsNullOrEmpty(orderTask.order_boxId))
            {
                if (orderTask.order_boxId.Contains("F"))
                {
                    boxType = orderTask.order_boxId.Trim().Remove(0, 1).Substring(0, 1);
                }
                else
                {
                    boxType = orderTask.order_boxId.Trim().Substring(0, 1);
                }
            }
            if (orderTask.order_boxId != null && !string.IsNullOrEmpty(orderTask.order_boxId) && boxType == "1")
            {
                agvOrderTask[10] = 0x01;   //托盘尺寸
            }
            else if (orderTask.order_boxId != null && !string.IsNullOrEmpty(orderTask.order_boxId) && boxType == "2")
            {
                agvOrderTask[10] = 0x02;   //托盘尺寸
            }
            if (orderTask.order_magic == -1 || orderTask.order_magic == 1 || orderTask.order_magic == 9999) //先取后放 | 原点任务
            {
                agvOrderTask[11] = Convert.ToByte((orderTask.order_getSite.ToInt() & 0xFF00) >> 8);
                agvOrderTask[12] = Convert.ToByte(orderTask.order_getSite.ToInt() & 0x00FF);
            }
            else
            {
                agvOrderTask[11] = Convert.ToByte((orderTask.order_putSite.ToInt() & 0xFF00) >> 8);
                agvOrderTask[12] = Convert.ToByte(orderTask.order_putSite.ToInt() & 0x00FF);
            }
            agvOrderTask[14] = 0x00; //上/下料OK

            if (StopStatus == 1)
            {
                agvOrderTask[13] = 0x01; //  1暂停
            }
            else if (StopStatus == 2)
            {
                agvOrderTask[13] = 0x02; // 2继续
            }
            else
            {
                agvOrderTask[13] = 0x00; // 2继续
            }
            if (!string.IsNullOrEmpty(orderTask.order_boxId))
            {
                var boxstr = Encoding.Default.GetBytes(orderTask.order_boxId);
                agvOrderTask[15] = 0x00;  //条码
                agvOrderTask[16] = 0x00;  //条码
                agvOrderTask[17] = 0x00;  //条码
                agvOrderTask[18] = 0x00;  //条码
                agvOrderTask[19] = 0x00;  //条码
                agvOrderTask[20] = 0x00;  //条码
                agvOrderTask[21] = 0x00;  //条码
                agvOrderTask[22] = 0x00;  //条码
                agvOrderTask[23] = 0x00;  //条码

                int s = 15;
                for (int i = 0; i < boxstr.Length; i++)
                {
                    if (s > agvOrderTask.Length)
                        break;
                    agvOrderTask[s] = boxstr[i];
                    s++;
                }
            }
            //ConsoleLogHelper.WriteSucceedLog(string.Join(" ", agvOrderTask.ToList().Select(p => p.ToString("X2"))));
            return agvOrderTask;
        }

        #region 出库PLC


      

        //查询接驳状态
        public static byte[] QueryOutSite()
        {
            byte[] Query = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x06, 0x00, 0x03, 0x00, 0x00, 0x00, 0x03 };
            return Query;
        }

        private static int index = 0;

        /// <summary>
        /// 写入指令 
        /// </summary>
        /// <param name="Doorstatus">1开门 2关门</param>
        /// <returns></returns>
        public static byte[] writeSiteOutPLC(int Doorstatus)
        {
            byte[] writeInfo = new byte[17];
            writeInfo[0] = 0x00;
            writeInfo[1] = 0x00;
            writeInfo[2] = 0x00;
            writeInfo[3] = 0x00;
            writeInfo[4] = 0x00;
            writeInfo[5] = 0x0B;
            writeInfo[6] = 0x00;
            writeInfo[7] = 0x10;
            writeInfo[8] = 0x00;  //寄存器地址
            writeInfo[9] = 0x03;   //寄存器地址
            writeInfo[10] = 0x00;  //写寄存器数量
            writeInfo[11] = 0x02;  //写寄存器数量
            writeInfo[12] = 0x04; //数据长度
            if (index == 0)
            {
                writeInfo[13] = 0x00;
                index = 1;
            }
            else
            {
                writeInfo[13] = 0x01;
                index = 0;
            }
            
            if (Doorstatus == 1) //开门
            {
                writeInfo[14] = 0x01;
            }
            else if (Doorstatus == 2) //关门
            {
                writeInfo[14] = 0x02;
            }
            writeInfo[15] = 0x00;
            writeInfo[16] = 0x00;
            return writeInfo;
        }

        #endregion

        #region 入库PLC

        //查询接驳状态
        public static byte[] QueryInSite()
        {
            byte[] Query = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x06, 0x00, 0x03, 0x00, 0x00, 0x00, 0x04 };
            return Query;
        }

      private static  int i = 0;
        /// <summary>
        /// 写入指令 
        /// </summary>
        /// <param name="Doorstatus">1开门 2关门</param>
        /// <returns></returns>
        public static byte[] writeSiteInPLC(int Doorstatus)
        {
            byte[] writeInfo = new byte[17];
            writeInfo[0] = 0x00;
            writeInfo[1] = 0x00;
            writeInfo[2] = 0x00;
            writeInfo[3] = 0x00;
            writeInfo[4] = 0x00;
            writeInfo[5] = 0x0B;
            writeInfo[6] = 0x00;
            writeInfo[7] = 0x10;
            writeInfo[8] = 0x00;   //寄存器地址
            writeInfo[9] = 0x04;   //寄存器地址
            writeInfo[10] = 0x00;  //写寄存器数量
            writeInfo[11] = 0x02;  //写寄存器数量
            writeInfo[12] = 0x04;  //数据长度
            if (i == 0)
            {
                writeInfo[13] = 0x00;
                i = 1;
            }
            else
            {
                writeInfo[13] = 0x01;
                i = 0;
            }
            if (Doorstatus == 1) //开门
            {
                writeInfo[14] = 0x01;
            }
            else if (Doorstatus == 2) //关门
            {
                writeInfo[14] = 0x02;
            }
            writeInfo[15] = 0x00;
            writeInfo[16] = 0x00;
            return writeInfo;
        }
        #endregion


        #endregion

        #region 温湿度传感器

        public static byte[] QueryTemperature()
        {
            return new byte[] { 0xFF, 0x03, 0x00, 0x00, 0x00, 0x02, 0xD1, 0xD5 };
        }
        #endregion
    }
}
