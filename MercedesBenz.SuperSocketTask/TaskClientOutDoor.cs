using MercedesBenz.DataBase;
using MercedesBenz.Infrastructure;
using MercedesBenz.Models;
using MercedesBenz.SuperSocketTask.ClientCode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MercedesBenz.SuperSocketTask
{
    public class TaskClientOutDoor : BaseClientTask
    {

        public override void MessageAnalysis(byte[] messageData)
        {
            List<byte[]> byteList = BytePackagedis.AnalysisByte(messageData);
            foreach (var messageitem in byteList)
            {
                if (messageitem.Length < 15)
                    break;
                if (messageitem[7] == 0x03)
                {
                    int DoorOutStatus = messageitem[10];
                    if (DoorOutStatus == 1)
                    {
                        SystemTaskDispose.DoorInfoArray[DoorType.Out].DoorStatus = DoorStatus.Open;
                    }
                    else
                    {
                        SystemTaskDispose.DoorInfoArray[DoorType.Out].DoorStatus = DoorStatus.Close;
                    }
                    int Buffer4 = messageitem[12];
                    int Buffer5 = messageitem[14];
                    SystemTaskDispose.DoorInfoArray[DoorType.Out].UpdateDateTime = UTC.ConvertDateTimeLong(DateTime.Now);
                    SystemTaskDispose.DoorInfoArray[DoorType.Out].updateDateInfoTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    SystemTaskDatabase.Instance.UpdateBufferStatus(4, Buffer4 == 1 ? 1 : 0);
                    SystemTaskDatabase.Instance.UpdateBufferStatus(5, Buffer5 == 1 ? 1 : 0);
                    var Log = string.Join(" ", messageitem.Select(s => s.ToString("X2")));
                    Log4NetHelper.WritedoorLog($"Form {base.iPEndPoint.Address}:{iPEndPoint.Port}：{Log}");
                }
            }
        }


        public override void ClientTaskRun()
        {
            SystemTaskDispose.Send(IPType.OutSite, GroupMessage.QueryOutSite());

            if (SystemConfiguration.IsRunDoor)
            {
                DoorTask();
            }
            if (SystemConfiguration.OutDoorON == 1)
            {
                SendCommand(1);
            }
            else if (SystemConfiguration.OutDoorON == 2)
            {
                SendCommand(2);
            }
            //状态1分钟未更新默认门为关闭状态
            if (UTC.ConvertDateTimeLong(DateTime.Now) - SystemTaskDispose.DoorInfoArray[DoorType.Out].UpdateDateTime > 10)
            {
                SystemTaskDatabase.Instance.UpdateBufferStatus(4, 1);
                SystemTaskDatabase.Instance.UpdateBufferStatus(5, 1);
                SystemTaskDispose.DoorInfoArray[DoorType.Out].DoorStatus = DoorStatus.Close;
                throw new Exception("10秒未收到出库PLC状态更新消息，已强制更改门状态为关闭，缓存位状态为已占用");
            }
        }


        //发送指令
        private void SendCommand(int command)
        {
            SystemTaskDispose.Send(IPType.OutSite, GroupMessage.writeSiteOutPLC(command));
            Log4NetHelper.WriteTaskLog($"出库手动操作，指令command:{command}");
            OperateIniTool.OperateIniWrite(0, "Service", "OutDoorNO");
        }

        private void DoorTask()
        {
            foreach (var agvNumber in SystemTaskDispose.agvInfoList.Keys)
            {
                var info = SystemTaskDispose.agvInfoList[agvNumber];
                var orderInfo = SystemTaskDatabase.Instance.QueryOrderCarnoinfo(agvNumber);  //根据车号查询任务订单
                var StationList = SystemTaskDatabase.Instance.QuerySiteConfiguration();
                var OriginSite1 = StationList.FirstOrDefault(p => p.station_number == 1);
                var OriginSite2 = StationList.FirstOrDefault(p => p.station_number == 6);
                if (OriginSite1 != null && OriginSite2 != null)
                {
                    if (orderInfo != null && info.CarIptype == IPType.weight)
                    {
                        if (info.ThisStation == 166 || info.ThisStation == 155)
                        {
                            Log4NetHelper.WriteTaskLog($"AGV到达出库门控请求范围当前站点{info.ThisStation}，当前任务编号：{orderInfo.order_ordernumber}");
                            if (orderInfo.order_type == OrderTaskType.OriginTask && orderInfo.order_magic == 9999 && orderInfo.order_getSite == OriginSite1.station_agvSite.ToString())
                            {
                                SendCommand(1);
                                Log4NetHelper.WriteTaskLog($"出库请求开门，当前任务编号：{orderInfo.order_ordernumber}");
                            }
                            else if (orderInfo.order_type == OrderTaskType.OriginTask && orderInfo.order_magic == 9999 && orderInfo.order_getSite == OriginSite2.station_agvSite.ToString())
                            {
                                SendCommand(2);
                                Log4NetHelper.WriteTaskLog($"出库请求关门，当前任务编号：{orderInfo.order_ordernumber}");
                            }
                            else if (orderInfo.order_type == OrderTaskType.Out && orderInfo.order_magic == 2)
                            {
                                SendCommand(1);
                                Log4NetHelper.WriteTaskLog($"出库请求开门，当前任务编号：{orderInfo.order_ordernumber}");
                            }
                            else if (orderInfo.order_type == OrderTaskType.Out && orderInfo.order_magic == 1)
                            {
                                SendCommand(2);
                                Log4NetHelper.WriteTaskLog($"出库请求关门，当前任务编号：{orderInfo.order_ordernumber}");
                            }
                            else if (orderInfo.order_type == OrderTaskType.In && orderInfo.order_magic == 1)
                            {
                                SendCommand(1);
                                Log4NetHelper.WriteTaskLog($"出库请求开门，当前任务编号：{orderInfo.order_ordernumber}");
                            }
                            else
                            {
                                SendCommand(1);
                                Log4NetHelper.WriteTaskLog($"出库请求开门，当前任务编号：{orderInfo.order_ordernumber}");
                            }
                        }
                        else if (info.ThisStation == 154)
                        {
                            Log4NetHelper.WriteTaskLog($"AGV到达出库门控请求范围当前站点{info.ThisStation}，当前任务编号：{orderInfo.order_ordernumber}");
                            if (orderInfo.order_type == OrderTaskType.OriginTask && orderInfo.order_magic == 9999 && orderInfo.order_getSite == OriginSite1.station_agvSite.ToString())
                            {
                                SendCommand(2);
                                Log4NetHelper.WriteTaskLog($"出库请求关门，当前任务编号：{orderInfo.order_ordernumber}");
                            }
                            else if (orderInfo.order_type == OrderTaskType.OriginTask && orderInfo.order_magic == 9999 && orderInfo.order_getSite == OriginSite2.station_agvSite.ToString())
                            {
                                SendCommand(1);
                                Log4NetHelper.WriteTaskLog($"出库请求开门，当前任务编号：{orderInfo.order_ordernumber}");
                            }
                            else if (orderInfo.order_type == OrderTaskType.Out && orderInfo.order_magic == 2)
                            {
                                SendCommand(2);
                                Log4NetHelper.WriteTaskLog($"出库请求关门，当前任务编号：{orderInfo.order_ordernumber}");
                            }
                            else if (orderInfo.order_type == OrderTaskType.In && orderInfo.order_magic == 1)
                            {
                                SendCommand(2);
                                Log4NetHelper.WriteTaskLog($"出库请求关门，当前任务编号：{orderInfo.order_ordernumber}");
                            }
                            else if (orderInfo.order_type == OrderTaskType.Out && orderInfo.order_magic == 1)
                            {
                                SendCommand(1);
                                Log4NetHelper.WriteTaskLog($"出库请求开门，当前任务编号：{orderInfo.order_ordernumber}");
                            }
                            else
                            {
                                SendCommand(1);
                                Log4NetHelper.WriteTaskLog($"出库请求开门，当前任务编号：{orderInfo.order_ordernumber}");
                            }
                        }
                    }
                }
            }
        }
    }
}
