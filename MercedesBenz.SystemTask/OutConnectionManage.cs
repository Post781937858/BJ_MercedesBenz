using MercedesBenz.DataBase;
using MercedesBenz.Infrastructure;
using MercedesBenz.Models;
using MercedesBenz.SystemTask.Client.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace MercedesBenz.SystemTask
{
    /// <summary>
    /// 出口缓存位状态及栏杆控制
    /// </summary>
    public class OutConnectionManage : BaseTcpClient
    {
        public OutConnectionManage(IPType type) : base(type)
        { }

        public override int Timelapse { get { return 500; } }

        public override void MessageAnalysis(byte[] mes)
        {
            List<byte[]> byteList = BytePackagedis.AnalysisByte(mes);
            foreach (var messageitem in byteList)
            {
                if (messageitem[7] == 0x03)
                {
                    if (messageitem.Length < 15)
                        break;
                    int DoorOutStatus = messageitem[10];
                    if (DoorOutStatus == 1)
                    {
                        TaskDispose.Instance.DoorInfoArray[DoorType.Out].DoorStatus = DoorStatus.Open;
                    }
                    else
                    {
                        TaskDispose.Instance.DoorInfoArray[DoorType.Out].DoorStatus = DoorStatus.Close;
                    }
                    int Buffer4 = messageitem[12];
                    int Buffer5 = messageitem[14];
                    TaskDispose.Instance.DoorInfoArray[DoorType.Out].UpdateDateTime = UTC.ConvertDateTimeLong(DateTime.Now);
                    SystemTaskDatabase.Instance.UpdateBufferStatus(4, Buffer4 == 1 ? 1 : 0);
                    SystemTaskDatabase.Instance.UpdateBufferStatus(5, Buffer5 == 1 ? 1 : 0);
                }
            }
        }

        public override void ClientTaskRun()
        {
            base.Send(GroupMessage.QueryOutSite());

            //状态1分钟未更新默认门为关闭状态
            if (UTC.ConvertDateTimeLong(DateTime.Now) - TaskDispose.Instance.DoorInfoArray[DoorType.Out].UpdateDateTime > 12000)
            {
                TaskDispose.Instance.DoorInfoArray[DoorType.Out].DoorStatus = DoorStatus.Close;
            }
            if (SystemConfiguration.IsRunDoor)
            {
                DoorTask();
            }
            if (SystemConfiguration.DoorON == 1)
            {
                base.Send(GroupMessage.writeSiteOutPLC(1));
            }
            else if (SystemConfiguration.DoorON == 2)
            {
                base.Send(GroupMessage.writeSiteOutPLC(2));
            }
        }

        private void DoorTask()
        {
            foreach (var agvNumber in TaskDispose.Instance.agvInfoList.Keys)
            {
                var info = TaskDispose.Instance.agvInfoList[agvNumber];
                var orderInfo = SystemTaskDatabase.Instance.QueryOrderCarnoinfo(agvNumber);  //根据车号查询任务订单
                var StationList = SystemTaskDatabase.Instance.QuerySiteConfiguration();
                var OriginSite1 = StationList.FirstOrDefault(p => p.station_number == 1);
                var OriginSite2 = StationList.FirstOrDefault(p => p.station_number == 6);
                if (OriginSite1 != null && OriginSite2 != null)
                {
                    if (orderInfo != null && info.CarIptype == IPType.agvNumer1)
                    {
                        if (info.ThisStation == 166 || info.ThisStation == 155)
                        {
                            Log4NetHelper.WriteTaskLog($"AGV到达出库门控请求范围当前站点{info.ThisStation}，当前任务编号：{orderInfo.order_ordernumber}");
                            if (orderInfo.order_type == OrderTaskType.OriginTask && orderInfo.order_magic == 9999 && orderInfo.order_getSite == OriginSite1.station_agvSite.ToString())
                            {
                                base.Send(GroupMessage.writeSiteOutPLC(1));
                                Log4NetHelper.WriteTaskLog($"出库请求开门，当前任务编号：{orderInfo.order_ordernumber}");
                            }
                            else if (orderInfo.order_type == OrderTaskType.OriginTask && orderInfo.order_magic == 9999 && orderInfo.order_getSite == OriginSite2.station_agvSite.ToString())
                            {
                                base.Send(GroupMessage.writeSiteOutPLC(2));
                                Log4NetHelper.WriteTaskLog($"出库请求关门，当前任务编号：{orderInfo.order_ordernumber}");
                            }
                            else if (orderInfo.order_type == OrderTaskType.Out && orderInfo.order_magic == 2)
                            {
                                base.Send(GroupMessage.writeSiteOutPLC(1));
                                Log4NetHelper.WriteTaskLog($"出库请求开门，当前任务编号：{orderInfo.order_ordernumber}");
                            }
                            else if (orderInfo.order_type == OrderTaskType.Out && orderInfo.order_magic == 1)
                            {
                                base.Send(GroupMessage.writeSiteOutPLC(2));
                                Log4NetHelper.WriteTaskLog($"出库请求关门，当前任务编号：{orderInfo.order_ordernumber}");
                            }
                            else if (orderInfo.order_type == OrderTaskType.In && orderInfo.order_magic == 1)
                            {
                                base.Send(GroupMessage.writeSiteOutPLC(1));
                                Log4NetHelper.WriteTaskLog($"出库请求开门，当前任务编号：{orderInfo.order_ordernumber}");
                            }
                            else
                            {
                                base.Send(GroupMessage.writeSiteOutPLC(1));
                                Log4NetHelper.WriteTaskLog($"出库请求开门，当前任务编号：{orderInfo.order_ordernumber}");
                            }
                        }
                        else if (info.ThisStation == 154)
                        {
                            Log4NetHelper.WriteTaskLog($"AGV到达出库门控请求范围当前站点{info.ThisStation}，当前任务编号：{orderInfo.order_ordernumber}");
                            if (orderInfo.order_type == OrderTaskType.OriginTask && orderInfo.order_magic == 9999 && orderInfo.order_getSite == OriginSite1.station_agvSite.ToString())
                            {
                                base.Send(GroupMessage.writeSiteOutPLC(2));
                                Log4NetHelper.WriteTaskLog($"出库请求关门，当前任务编号：{orderInfo.order_ordernumber}");
                            }
                            else if (orderInfo.order_type == OrderTaskType.OriginTask && orderInfo.order_magic == 9999 && orderInfo.order_getSite == OriginSite2.station_agvSite.ToString())
                            {
                                base.Send(GroupMessage.writeSiteOutPLC(1));
                                Log4NetHelper.WriteTaskLog($"出库请求开门，当前任务编号：{orderInfo.order_ordernumber}");
                            }
                            else if (orderInfo.order_type == OrderTaskType.Out && orderInfo.order_magic == 2)
                            {
                                base.Send(GroupMessage.writeSiteOutPLC(2));
                                Log4NetHelper.WriteTaskLog($"出库请求关门，当前任务编号：{orderInfo.order_ordernumber}");
                            }
                            else if (orderInfo.order_type == OrderTaskType.In && orderInfo.order_magic == 1)
                            {
                                base.Send(GroupMessage.writeSiteOutPLC(2));
                                Log4NetHelper.WriteTaskLog($"出库请求关门，当前任务编号：{orderInfo.order_ordernumber}");
                            }
                            else if (orderInfo.order_type == OrderTaskType.Out && orderInfo.order_magic == 1)
                            {
                                base.Send(GroupMessage.writeSiteOutPLC(1));
                                Log4NetHelper.WriteTaskLog($"出库请求开门，当前任务编号：{orderInfo.order_ordernumber}");
                            }
                            else
                            {
                                base.Send(GroupMessage.writeSiteOutPLC(1));
                                Log4NetHelper.WriteTaskLog($"出库请求开门，当前任务编号：{orderInfo.order_ordernumber}");
                            }
                        }
                    }
                }
            }
        }
    }
}
