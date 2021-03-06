﻿using MercedesBenz.DataBase;
using MercedesBenz.Infrastructure;
using MercedesBenz.Models;
using MercedesBenz.SystemTask.Server.Base;
using System;
using System.Linq;
using System.Text;
using System.Threading;

namespace MercedesBenz.SystemTask
{
    public class OrderTaskServiceWCS : BaseTcpClientServer
    {
        private object Task_lock = new object();

        public OrderTaskServiceWCS(IPType type) : base(type)
        { }

        public override void MessageAnalysis(byte[] mes)
        {
            lock (Task_lock)
            {
                string _str_message = Encoding.Default.GetString(mes).Trim("\0".ToArray());
                string[] str_messageList = _str_message.Replace('\n', '$').Split('$');
                foreach (var str_message in str_messageList)
                {
                    if (str_message.Contains("messageName"))
                    {
                        BaseMessage _baseMessage = str_message.ToObject<BaseMessage>();
                        switch (_baseMessage.messageName)
                        {
                            case "updateMovement": //任务状态更新回应
                                {
                                    ResponsemesUpdate responsemes = str_message.ToObject<ResponsemesUpdate>();
                                    SystemTaskDatabase.Instance.UpdateMessage(responsemes);
                                }
                                break;
                            case "StockOut": //出库任务
                                {
                                    StockOutTaskmsg taskmsg = str_message.ToObject<StockOutTaskmsg>();
                                    if (QueryOrderExists(OrderTaskType.Out, taskmsg, OrderType.NDC))
                                    {
                                        //添加订单
                                        if (SystemTaskDatabase.Instance.IssueOrderTask(taskmsg, OrderType.NDC, OrderTaskType.Out))
                                        {
                                            ResponseWCS(taskmsg, 0);
                                            Thread.Sleep(1000);
                                            //回应订单状态为 status 为0
                                            ResponseUpdateWCS(OrderTaskType.Out, taskmsg);
                                        }
                                        else
                                        {
                                            ResponseWCS(taskmsg, -1);
                                        }
                                    }
                                }
                                break;
                            case "boxAnnounce": //入库任务
                                {
                                    boxAnnounceTaskmsg taskmsg = str_message.ToObject<boxAnnounceTaskmsg>();
                                    if (QueryOrderExists(OrderTaskType.In, taskmsg, OrderType.AGV))
                                    {
                                        var StationInfo = SystemTaskDatabase.Instance.QuerySiteConfiguration().FirstOrDefault(p => p.station_wcsSite == taskmsg.s_station.ToInt());
                                        if (StationInfo != null)
                                        {
                                            taskmsg.s_station = StationInfo.station_agvSite.ToString();
                                            if (SystemTaskDatabase.Instance.IssueOrderTask(taskmsg, OrderType.AGV, OrderTaskType.In)) //添加订单
                                            {
                                                ResponseWCS(taskmsg, 0);
                                                //回应订单状态为 status 为0
                                                Thread.Sleep(1000);
                                                ResponseUpdateWCS(OrderTaskType.In, taskmsg);
                                            }
                                            else
                                            {
                                                ResponseWCS(taskmsg, -1);
                                            }
                                        }
                                        else
                                        {
                                            ResponseWCS(taskmsg, -1);
                                        }
                                    }
                                }
                                break;
                            case "movePosition": //库内搬运
                                {
                                    movePositionTaskmsg taskmsg = str_message.ToObject<movePositionTaskmsg>();
                                    if (QueryOrderExists(OrderTaskType.Interior, taskmsg, OrderType.NDC))
                                    {
                                        if (SystemTaskDatabase.Instance.IssueOrderTask(taskmsg, OrderType.NDC, OrderTaskType.Interior)) //添加订单
                                        {
                                            ResponseWCS(taskmsg, 0);
                                            //回应订单状态为 status 为0 
                                            Thread.Sleep(1000);
                                            ResponseUpdateWCS(OrderTaskType.Interior, taskmsg);
                                        }
                                        else
                                        {
                                            ResponseWCS(taskmsg, -1);
                                        }
                                    }
                                }
                                break;
                            //*****************************需判断任务重复*******************************
                            case "carryPlatfrom": //站台搬运  
                                {
                                    carryPlatfromTaskmsg taskmsg = str_message.ToObject<carryPlatfromTaskmsg>();
                                    var StationList = SystemTaskDatabase.Instance.QuerySiteConfiguration();
                                    var GetStationInfo = StationList.FirstOrDefault(p => p.station_wcsSite == taskmsg.s_station.ToInt());
                                    var PutStationInfo = StationList.FirstOrDefault(p => p.station_wcsSite == taskmsg.d_station.ToInt());
                                    if (GetStationInfo != null && PutStationInfo != null)
                                    {
                                        taskmsg.s_station = GetStationInfo.station_agvSite.ToString();
                                        taskmsg.d_station = PutStationInfo.station_agvSite.ToString();
                                        if (QueryOrderExists(OrderTaskType.Platform, taskmsg, OrderType.AGV))
                                        {
                                            if (SystemTaskDatabase.Instance.IssueOrderTask(taskmsg, OrderType.AGV, OrderTaskType.Platform)) //添加订单
                                            {
                                                ResponseWCS(taskmsg, 0);
                                                //回应订单状态为 status 为0
                                                Thread.Sleep(1000);
                                                ResponseUpdateWCS(OrderTaskType.Platform, taskmsg);
                                            }
                                            else
                                            {
                                                ResponseWCS(taskmsg, -1);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        ResponseWCS(taskmsg, -1);
                                    }
                                }
                                break;

                            case "Door": //门控制
                                {
                                    DoormesInfo taskmsg = str_message.ToObject<DoormesInfo>();
                                    if (taskmsg.door == 1)
                                    {
                                        if (taskmsg.doortype == OrderTaskType.In)
                                            TaskDispose.Instance._BackgroundTcpClient[IPType.InSite].Send(GroupMessage.writeSiteInPLC(1));
                                        else
                                            TaskDispose.Instance._BackgroundTcpClient[IPType.OutSite].Send(GroupMessage.writeSiteOutPLC(1));
                                    }
                                    else
                                    {
                                        if (taskmsg.doortype == OrderTaskType.In)
                                            TaskDispose.Instance._BackgroundTcpClient[IPType.InSite].Send(GroupMessage.writeSiteInPLC(2));
                                        else
                                            TaskDispose.Instance._BackgroundTcpClient[IPType.OutSite].Send(GroupMessage.writeSiteOutPLC(2));
                                    }
                                    ResponseWCS(taskmsg, 0);
                                }
                                break;
                            case "alarmException": //报警回应
                                {


                                }
                                break;
                        }
                        Log4NetHelper.WritewcsLog(str_message);
                        ConsoleLogHelper.WriteInfoLog(str_message);
                    }
                }
                Log4NetHelper.WriteDebugLog(_str_message);
            }
        }

        #region Code 01

        /// <summary>
        ///查询订单是否存在
        /// </summary>
        /// <param name="messageName"></param>
        /// <param name="taskmsg"></param>
        /// <returns></returns>
        private bool QueryOrderExists(OrderTaskType OrderType, BaseTask taskmsg, OrderType CarorderType)
        {
            var orderTask = SystemTaskDatabase.Instance.QueryOrderWMSID(taskmsg.wmsId);
            string order_getLocation = string.Empty;
            string order_putLocation = string.Empty;
            SetSite(out order_getLocation, out order_putLocation, OrderType, taskmsg);
            if ((orderTask != null && orderTask.order_genre == CarorderType) || string.IsNullOrEmpty(order_getLocation) || string.IsNullOrEmpty(order_putLocation) && OrderType != OrderTaskType.Platform)
            {
                ResponseWCS(taskmsg, 2);
                return false;
            }
            else if (OrderType == OrderTaskType.Platform)
            {
                orderTask = SystemTaskDatabase.Instance.QueryOrderWMSID(taskmsg.id.ToString());
                if (orderTask != null)
                {
                    ResponseWCS(taskmsg, 2);
                    return false;
                }
            }
            return true;
        }

        public void ResponseUpdateWCS(OrderTaskType OrderType, BaseTask taskmsg)
        {
            //SetSite(out string _order_getLocation, out string _order_putLocation, OrderType, taskmsg);
            //var messageText = GroupMessage.GroupMessage_WCS(new OrderTask()
            //{
            //    order_arecore = taskmsg.areaCode,
            //    order_type = OrderTaskType.In,
            //    order_boxId = taskmsg.boxId,
            //    order_wmsId = taskmsg.wmsId,
            //    order_getLocation = _order_getLocation,
            //    order_putLocation = _order_putLocation,
            //    order_sourceCode = taskmsg.sourceCode
            //}, 0);
            //TaskDispose.Instance.Send(IPType.wcs, GroupMessage.MessageToByte<UpdateOrderTask>(messageText));
            //SystemTaskDatabase.Instance.InsertMessage(messageText);
        }

        private void SetSite(out string _order_getLocation, out string _order_putLocation, OrderTaskType OrderType, BaseTask taskmsg)
        {
            if (OrderType == OrderTaskType.Out)
            {
                _order_getLocation = (taskmsg as StockOutTaskmsg).location;
                _order_putLocation = (taskmsg as StockOutTaskmsg).d_station;
            }
            else if (OrderType == OrderTaskType.In)
            {
                _order_getLocation = (taskmsg as boxAnnounceTaskmsg).s_station;
                _order_putLocation = (taskmsg as boxAnnounceTaskmsg).location;
            }
            else if (OrderType == OrderTaskType.Interior)
            {
                _order_getLocation = (taskmsg as movePositionTaskmsg).s_location;
                _order_putLocation = (taskmsg as movePositionTaskmsg).d_location;
            }
            else if (OrderType == OrderTaskType.Platform)
            {
                _order_getLocation = (taskmsg as carryPlatfromTaskmsg).s_station;
                _order_putLocation = (taskmsg as carryPlatfromTaskmsg).d_station;
            }
            else
            {
                _order_getLocation = string.Empty;
                _order_putLocation = string.Empty;
            }
        }

        /// <summary>
        /// 任务下发回应
        /// </summary>
        /// <param name="taskmsg"></param>
        private void ResponseWCS(BaseTask taskmsg, int result)
        {
            TaskDispose.Instance.SendServer(IPType.server, GroupMessage.MessageToByte<ResponsemesUpdate>(new ResponsemesUpdate()
            {
                id = taskmsg.id,
                messageName = taskmsg.messageName,
                comm_state = 0,
                sourceCode = taskmsg.sourceCode,
                areaCode = taskmsg.areaCode,
                boxId = taskmsg.boxId,
                result = result,  //任务创建成功
            }), IPType.wcs);
        }
        #endregion

        public override void ServerTaskRun()
        {
            var _updateOrderTask = SystemTaskDatabase.Instance.MessageList();
            _updateOrderTask.ForEach(updatemessage =>
            {
                long ThisUtcTime = UTC.ConvertDateTimeLong(DateTime.Now);
                //mes_Status为-1 且 发送状态更新消息20秒未回应，重新发送并更新时间
                if (updatemessage.mes_Status == -1 && ThisUtcTime - updatemessage.mes_utctime > 20)
                {
                    TaskDispose.Instance.Send(IPType.wcs, GroupMessage.MessageToByte(updatemessage.mes_Data.ToJObject()));
                    SystemTaskDatabase.Instance.UpdateMessageTime(updatemessage);
                }
                //将状态更新消息移动到历史消息
                if (updatemessage.mes_Status == 2)
                {
                    SystemTaskDatabase.Instance.CopyMessage(updatemessage);
                }
            });
        }
    }
}
