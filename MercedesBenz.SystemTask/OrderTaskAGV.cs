using MercedesBenz.Infrastructure;
using MercedesBenz.Models;
using System.Collections.Generic;
using System.Collections.Concurrent;
using MercedesBenz.DataBase;
using System.Linq;
using System;
using MercedesBenz.SystemTask.Server.Base;

namespace MercedesBenz.SystemTask
{
    public class OrderTaskAGV : BaseTcpClientServer
    {
        public OrderTaskAGV(IPType type) : base(type)
        { }

        public override void MessageAnalysis(byte[] mes, IPType type)
        {
            List<byte[]> byteList = BytePackagedis.AnalysisByte(mes);
            foreach (var message in byteList)
            {
                if (message.Length < 8)
                    break;
                var StationList = SystemTaskDatabase.Instance.QuerySiteConfiguration();
                if (message[7] == 0x10)
                {
                    if (message.Length < 35)
                        break;
                    agvInfo info = new agvInfo();
                    info.agvNumber = message[14]; //agv编号
                    info.voltage = (message[15] << 8 | message[16]) / 10; //电压
                    info.NavigationStatus = (navigation)(message[17] | message[18]); //导航状态
                    info.ThisStation = (message[19] << 8 | message[20]); //当前战点
                    info.TargetStation = (message[21] << 8 | message[22]); //目标战点
                    info.taskStatus = (TaskStatus)message[23]; //上/下料完成状态
                    info.rollStatus = message[24]; //滚筒状态
                    info.ErrorCode = message[25] << 8 | message[26]; //错误码
                    byte[] Byte_X = { message[27], message[28], message[29], message[30] };
                    byte[] Byte_Y = { message[31], message[32], message[33], message[34] };
                    info.location_X = BitConverter.ToSingle(Byte_X.Reverse().ToArray(), 0); //X轴
                    info.location_Y = BitConverter.ToSingle(Byte_Y.Reverse().ToArray(), 0); //Y轴
                    if (TaskDispose.Instance.agvInfoList.ContainsKey(info.agvNumber))
                    {
                        if (info.ThisStation == 0)
                        {
                            info.ThisStation = TaskDispose.Instance.agvInfoList[info.agvNumber].ThisStation;
                        }
                        info.CarIptype = TaskDispose.Instance.agvInfoList[info.agvNumber].CarIptype;
                        info.UpdateStopDateTime = TaskDispose.Instance.agvInfoList[info.agvNumber].UpdateStopDateTime;
                        info.UpdateDateTime = UTC.ConvertDateTimeLong(DateTime.Now);
                        info.StopRunStatus = TaskDispose.Instance.agvInfoList[info.agvNumber].StopRunStatus;
                        TaskDispose.Instance.agvInfoList[info.agvNumber] = info;
                    }
                    if (TaskDispose.Instance.agvInfoList.ContainsKey(info.agvNumber))
                    {
                        var agvinfo = TaskDispose.Instance.agvInfoList[info.agvNumber];
                        var mesg = GroupMessage.responsePLCInfo(message);
                        TaskDispose.Instance.SendServer(IPType.agvserver, mesg, agvinfo.CarIptype);
                    }
                }
                else if (message[7] == 0x03)
                {
                    //所有订单
                    List<OrderTask> _orderTasksAll = SystemTaskDatabase.Instance.QueryOrderAGV();
                    var agvInfo = TaskDispose.Instance.agvInfoList.Values.FirstOrDefault(p => p.CarIptype == type);
                    if (agvInfo != null)
                    {
                        int agvNumber = agvInfo.agvNumber;
                        var orderInfo = SystemTaskDatabase.Instance.QueryOrderCarnoinfo(agvNumber);  //根据车号查询任务订单
                        var bufferStatus = SystemTaskDatabase.Instance.QueryBufffer().OrderBy(p => p.Buff_number);

                        #region 门状态检测防止碰撞

                        //门状态检测防止碰撞
                        if (agvInfo.ThisStation == 166 || agvInfo.ThisStation == 155)
                        {
                            var DoorInfo = TaskDispose.Instance.DoorInfoArray[DoorType.Out];
                            var OriginSite = StationList.FirstOrDefault(p => p.station_number == 1);
                            var OriginSite1 = StationList.FirstOrDefault(p => p.station_number == 3);
                            if (OriginSite != null && OriginSite1 != null)
                            {
                                if ((orderInfo.order_type == OrderTaskType.OriginTask && orderInfo.order_magic == 9999 && orderInfo.order_getSite == OriginSite.station_agvSite.ToString())
                                    || (orderInfo.order_type == OrderTaskType.Out && orderInfo.order_magic == 2) || (orderInfo.order_type == OrderTaskType.In && orderInfo.order_magic == 1))
                                {
                                    if (DoorInfo.DoorStatus == DoorStatus.Close)
                                    {
                                        agvInfo.agvRunStop = 1;
                                    }
                                }
                            }
                        }
                        else if (agvInfo.ThisStation == 154)
                        {
                            var DoorInfo = TaskDispose.Instance.DoorInfoArray[DoorType.Out];
                            var OriginSite = StationList.FirstOrDefault(p => p.station_number == 6);
                            if (OriginSite != null)
                            {
                                if (orderInfo.order_type == OrderTaskType.OriginTask && orderInfo.order_magic == 9999 && orderInfo.order_getSite == OriginSite.station_agvSite.ToString()
                                || (orderInfo.order_type == OrderTaskType.Out && orderInfo.order_magic == 1))
                                {
                                    if (DoorInfo.DoorStatus == DoorStatus.Close)
                                    {
                                        agvInfo.agvRunStop = 1;
                                    }
                                }
                            }
                        }
                        else if (agvInfo.ThisStation == 184 || agvInfo.ThisStation == 44)
                        {
                            var DoorInfo = TaskDispose.Instance.DoorInfoArray[DoorType.In];
                            if (DoorInfo.DoorStatus == DoorStatus.Close)
                            {
                                agvInfo.agvRunStop = 1;
                            }
                        }

                        #endregion

                        #region AGV避让

                        if (agvInfo.ThisStation == 309 || agvInfo.ThisStation == 342)
                        {
                            bool IsRun = false;
                            foreach (var agvInfoitem in TaskDispose.Instance.agvInfoList.Values)
                            {
                                if (agvInfoitem.ThisStation == 154 || agvInfoitem.ThisStation == 155 || agvInfoitem.ThisStation == 22 || agvInfoitem.ThisStation == 85 || agvInfoitem.ThisStation == 86 || agvInfoitem.ThisStation == 87|| agvInfoitem.ThisStation == 181)
                                {
                                    IsRun = true;
                                }
                            }
                            if (IsRun)
                            {
                                agvInfo.agvRunStop = 1;
                            }
                        }

                        #region NDC避让

                        //所有NDC订单   
                        List<OrderTask> _orderTasksNDCAll = SystemTaskDatabase.Instance.QueryOrderDNC();
                        //任务分配
                        var NDCInCar1 = _orderTasksNDCAll.Where(p => p.order_type == OrderTaskType.In && p.order_magic == 1);
                        //取货完成订单
                        var NDCInCar2 = _orderTasksNDCAll.Where(p => p.order_type == OrderTaskType.In && p.order_magic == 10 && (UTC.ConvertDateTimeLong(DateTime.Now) - p.order_utctime < 6000));
                        if (NDCInCar1.Count() > 0 || NDCInCar2.Count() > 0)
                        {
                            if (agvInfo.ThisStation == 13 || agvInfo.ThisStation == 184)
                            {
                                agvInfo.agvRunStop = 1;
                            }
                        }
                        var NDCInCar3 = _orderTasksNDCAll.Where(p => p.order_type == OrderTaskType.Out && p.order_magic != -1 && p.order_status != -1);
                        if (NDCInCar3.Count() > 0 && (orderInfo.order_getSite == "185" && orderInfo.order_magic == 9999))
                        {
                            if (agvInfo.ThisStation == 190 || agvInfo.ThisStation == 154)
                            {
                                agvInfo.agvRunStop = 1;
                            }
                        }
                        #endregion

                        #endregion

                        if (orderInfo == null)
                        {
                            //重载agv任务调度
                            if (agvInfo.CarIptype == IPType.agvNumer1)
                            {
                                //待执行订单
                                List<OrderTask> NotexecuteTask = _orderTasksAll.Where(p => p.order_magic == -1 && p.order_type != OrderTaskType.Platform).OrderByDescending(p => p.order_priority).ToList();
                                if (NotexecuteTask.Count() > 0)
                                {
                                    int Index = new Random().Next(0, NotexecuteTask.Count() - 1);
                                    var awaitexecuteTask = NotexecuteTask[Index];

                                    //入库任务  取货站台位置确定 放货站台位置待定 需根据托盘大小查询是否有可用缓存位 取得agv站台号
                                    if (awaitexecuteTask.order_type == OrderTaskType.In)
                                    {
                                        int boxType = 1;
                                        if (awaitexecuteTask.order_boxId.Contains("F"))
                                        {
                                            //托盘大小
                                            boxType = awaitexecuteTask.order_boxId.Remove(0, 1).Substring(0, 1).ToInt();
                                        }
                                        else
                                        {
                                            //托盘大小
                                            boxType = awaitexecuteTask.order_boxId.Substring(0, 1).ToInt();
                                        }
                                        boxType = 1;
                                        //查询入库任务托盘大小并查询是否有可用货位
                                        var bufferList = bufferStatus.Where(p => p.Buff_type == boxType && p.Buff_status == 0 && p.Buff_number < 4);
                                        foreach (var item in bufferList)
                                        {
                                            awaitexecuteTask.order_putSite = item.Buff_Site.ToString(); //取得放货托盘agv站台号
                                            if (SystemTaskDatabase.Instance.UpdateputSite(awaitexecuteTask))
                                            {
                                                byte[] awaitTask = GroupMessage.agvTask(awaitexecuteTask, message, 0, agvNumber);
                                                //更新阶段并更新agv车号
                                                if (SystemTaskDatabase.Instance.UpdatemagicCarNo(1, awaitexecuteTask.order_ordernumber, agvNumber))
                                                {
                                                    TaskDispose.Instance.SendServer(IPType.agvserver, awaitTask, agvInfo.CarIptype);
                                                }
                                            }
                                            break;
                                        }
                                    }
                                    //出库任务  出库任务由NDC出库任务完成自动生成 取货点确定 不在缓存位查询处理
                                    else if (awaitexecuteTask.order_type == OrderTaskType.Out)
                                    {
                                        var bufferList = bufferStatus.Where(p => p.Buff_status == 1 && p.Buff_number > 3 && p.Buff_Site == awaitexecuteTask.order_getSite.ToInt());
                                        foreach (var item in bufferList)
                                        {
                                            if (SystemTaskDatabase.Instance.UpdateBufferStatus(item.Buff_number, 0)) //更新库存状态空
                                            {
                                                byte[] awaitTask = GroupMessage.agvTask(awaitexecuteTask, message, 0, agvNumber);
                                                if (SystemTaskDatabase.Instance.UpdatemagicCarNo(1, awaitexecuteTask.order_ordernumber, agvNumber))
                                                {
                                                    TaskDispose.Instance.SendServer(IPType.agvserver, awaitTask, agvInfo.CarIptype);
                                                }
                                            }
                                            break;
                                        }   
                                    }
                                    agvInfo.UpdateStopDateTime = 0;
                                }
                                else //无任务
                                {
                                    var Originsite = SystemTaskDatabase.Instance.Queryagvsite(agvNumber);

                                    if (Originsite != null)
                                    {
                                        bufferStatus.Where(p => p.Buff_number < 4).ToList().ForEach(p =>
                                        {
                                            if (p.Buff_Site == agvInfo.ThisStation)
                                            {
                                                SystemTaskDatabase.Instance.InsertOriginTask(new OrderTask() { order_carno = agvNumber, order_getSite = Originsite.site_agvsite.ToString() });
                                            }
                                        });

                                        StationList.ForEach(p =>
                                        {
                                            if (p.station_agvSite == agvInfo.ThisStation && p.station_number != 2)
                                            {
                                                if (agvInfo.ThisStation == Originsite.site_agvsite)
                                                {
                                                    byte[] awaitTask = GroupMessage.agvTask(new OrderTask(), message, 0, agvNumber);
                                                    TaskDispose.Instance.SendServer(IPType.agvserver, awaitTask, agvInfo.CarIptype);
                                                }
                                                else
                                                {
                                                    SystemTaskDatabase.Instance.InsertOriginTask(new OrderTask() { order_carno = agvNumber, order_getSite = Originsite.site_agvsite.ToString() });
                                                }
                                            }
                                            else if (p.station_agvSite == agvInfo.ThisStation && p.station_number == 2)
                                            {
                                                if (agvInfo.UpdateStopDateTime == 0)
                                                {   
                                                    //站点172默认等待10分钟(防止分拣未完成需做回库处理)
                                                    agvInfo.UpdateStopDateTime = UTC.ConvertDateTimeLong(DateTime.Now);
                                                }
                                                var time = agvInfo.UpdateDateTime - agvInfo.UpdateStopDateTime;
                                                if (agvInfo.UpdateDateTime - agvInfo.UpdateStopDateTime > 600)
                                                {
                                                    agvInfo.UpdateStopDateTime = 0;
                                                    SystemTaskDatabase.Instance.InsertOriginTask(new OrderTask() { order_carno = agvNumber, order_getSite = Originsite.site_agvsite.ToString() });
                                                }
                                            }
                                        });
                                    }
                                }
                            }
                            //轻载agv任务调度
                            else if (agvInfo.CarIptype == IPType.light)
                            {
                                var NotexecuteTask = _orderTasksAll.Where(p => p.order_magic == -1 && p.order_type == OrderTaskType.Platform).OrderByDescending(p => p.order_priority).ToList();
                                if (NotexecuteTask.Count() > 0)
                                {
                                    int Index = new Random().Next(0, NotexecuteTask.Count() - 1);
                                    var awaitexecuteTask = NotexecuteTask[Index];
                                    byte[] awaitTask = GroupMessage.agvTask(awaitexecuteTask, message, 0, agvNumber);
                                    if (SystemTaskDatabase.Instance.UpdatemagicCarNo(1, awaitexecuteTask.order_ordernumber, agvNumber))
                                    {
                                        TaskDispose.Instance.SendServer(IPType.agvserver, awaitTask, agvInfo.CarIptype);
                                    }
                                }
                                else
                                {
                                    var Originsite = SystemTaskDatabase.Instance.Queryagvsite(agvNumber);
                                    if (Originsite != null)
                                    {
                                        StationList.ForEach(p =>
                                        {
                                            if (agvInfo.ThisStation == p.station_agvSite)
                                            {
                                                if (Originsite.site_agvsite == agvInfo.ThisStation)
                                                {
                                                    byte[] awaitTask = GroupMessage.agvTask(new OrderTask(), message, 0, agvNumber);
                                                    TaskDispose.Instance.SendServer(IPType.agvserver, awaitTask, agvInfo.CarIptype);
                                                }
                                                else
                                                {
                                                    SystemTaskDatabase.Instance.InsertOriginTask(new OrderTask() { order_carno = agvNumber, order_getSite = Originsite.site_agvsite.ToString() });
                                                }
                                            }
                                        });
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (orderInfo.order_magic == 1) //阶段一前往取货点
                            {
                                //目标站点=取货站台 当前站点=取货站台  上料状态==上料完成
                                if (agvInfo.TargetStation == orderInfo.order_getSite.ToInt() && agvInfo.ThisStation == orderInfo.order_getSite.ToInt() && agvInfo.taskStatus == TaskStatus.UPmaterial)
                                {
                                    if (SystemTaskDatabase.Instance.UpdatemagicCarNo(2, orderInfo.order_ordernumber, agvNumber))
                                    {
                                        byte[] awaitTask = GroupMessage.agvTask(orderInfo, message, 0, agvNumber);
                                        TaskDispose.Instance.SendServer(IPType.agvserver, awaitTask, agvInfo.CarIptype);
                                    }
                                }
                                else
                                {
                                    SendTaskByte(agvInfo, orderInfo, message);
                                }
                            }
                            else if (orderInfo.order_magic == 2)//阶段二前往放货点
                            {
                                if (agvInfo.TargetStation == orderInfo.order_putSite.ToInt() && agvInfo.ThisStation == orderInfo.order_putSite.ToInt() && agvInfo.taskStatus == TaskStatus.Downmaterial)
                                {
                                    if (orderInfo.order_type == OrderTaskType.In)
                                    {
                                        var buffer = bufferStatus.FirstOrDefault(p => p.Buff_Site == orderInfo.order_putSite.ToInt());
                                        if (buffer != null)
                                        {
                                            boxAnnounceTaskmsg announceTaskmsg = new boxAnnounceTaskmsg()
                                            {
                                                areaCode = orderInfo.order_arecore,
                                                boxId = orderInfo.order_boxId,
                                                s_station = buffer.Buff_ndcSite.ToString(),
                                                location = orderInfo.order_putLocation,
                                                priority = orderInfo.order_priority,
                                                sourceCode = orderInfo.order_sourceCode,
                                                wmsId = orderInfo.order_wmsId,
                                            };
                                            if (SystemTaskDatabase.Instance.UpdateBufferStatus(buffer.Buff_number, 1))//更新缓存区状态
                                            {
                                                //生成NDC入库搬运订单
                                                if (SystemTaskDatabase.Instance.AutomationInsertNDC(announceTaskmsg, OrderType.NDC, OrderTaskType.In))
                                                {
                                                    SystemTaskDatabase.Instance.DeleteOrder_history(orderInfo);
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        SystemTaskDatabase.Instance.DeleteOrder_history(orderInfo);
                                    }
                                }
                                else
                                {
                                    SendTaskByte(agvInfo, orderInfo, message);
                                }
                            }
                            else if (orderInfo.order_magic == 9999)//原点任务
                            {
                                if (orderInfo.order_getSite == agvInfo.ThisStation.ToString())
                                {
                                    SystemTaskDatabase.Instance.DeleteOrder_history(orderInfo);
                                }
                                else
                                {
                                    SendTaskByte(agvInfo, orderInfo, message);
                                }
                            }
                            agvInfo.UpdateStopDateTime = 0;
                        }
                    }
                }
                var Log = string.Join(" ", message.Select(s => s.ToString("X2")));
                Log4NetHelper.WriteagvLog(Log);
            }
        }


        #region 位置查询

        //入库缓存位范围
        private bool InIsRun()
        {
            bool Run = false;
            foreach (var agvNumber in TaskDispose.Instance.agvInfoList.Keys)
            {
                var info = TaskDispose.Instance.agvInfoList[agvNumber];
                if (info.location_X >= 14 && info.location_X <= 23 && info.location_Y >= 24 && info.location_Y <= 45)
                {
                    Run = true;
                }
            }
            return Run;
        }

        #endregion

        /// <summary>
        /// 向agv发送任务信息
        /// </summary>
        /// <param name="agvInfo">agv状态</param>
        /// <param name="orderInfo">订单信息</param>
        /// <param name="message">agv数据帧</param>
        private void SendTaskByte(agvInfo agvInfo, OrderTask orderInfo, byte[] message)
        {
            byte[] awaitTask;
            if (agvInfo.agvRunStop == 1 && agvInfo.NavigationStatus == navigation.This)
            {
                awaitTask = GroupMessage.agvTask(new OrderTask() { }, message, 1, agvInfo.agvNumber); //暂停导航
            }
            else if (agvInfo.agvRunStop == 1 && agvInfo.NavigationStatus == navigation.suspend)
            {
                awaitTask = GroupMessage.agvTask(new OrderTask() { }, message, 1, agvInfo.agvNumber);//暂停导航
            }
            else if (agvInfo.agvRunStop == 0 && agvInfo.NavigationStatus == navigation.suspend)
            {
                awaitTask = GroupMessage.agvTask(orderInfo, message, 2, agvInfo.agvNumber); //继续导航
            }
            else
            {
                awaitTask = GroupMessage.agvTask(orderInfo, message, 0, agvInfo.agvNumber); //默认导航
            }
            TaskDispose.Instance.SendServer(IPType.agvserver, awaitTask, agvInfo.CarIptype);
        }


        /// <summary>
        /// 后台任务
        /// </summary>
        public override void ServerTaskRun()
        {
            #region NDC订单生成

            if (!InIsRun()) //入库口无AGV存在将NDC入库订单，插入订单表
            {
                var bufferNDCTask = SystemTaskDatabase.Instance.QueryBufferOrderDNC();
                if (bufferNDCTask.Count() > 0)
                {
                    bufferNDCTask.ForEach(p =>
                    {
                        SystemTaskDatabase.Instance.EstablishNDCTask(p);
                    });
                }
            }
            #endregion

            foreach (var agvInfoitem in TaskDispose.Instance.agvInfoList.Values)
            {
                var orderInfo = SystemTaskDatabase.Instance.QueryOrderCarnoinfo(agvInfoitem.agvNumber);
                var carInfo = new tb_carinfo_model();
                carInfo.Info_number = agvInfoitem.agvNumber;
                bool Iserror;
                carInfo.Info_errormes = ErrorCodeMessage(agvInfoitem.ErrorCode, out Iserror);
                carInfo.Info_Iserror = Iserror ? 0 : 1;
                if (orderInfo != null)
                {
                    carInfo.Info_status = "执行任务中";
                }
                else
                {
                    carInfo.Info_status = "无任务";
                }
                carInfo.Info_voltage = 26.5;
                carInfo.Info_ThisSite = agvInfoitem.ThisStation;
                carInfo.Info_Ischarge = 0;
                carInfo.Info_cartype = "AGV";
                SystemTaskDatabase.Instance.tb_carinfo_Update(carInfo);
            }
            //Console.WriteLine($"Thread：{System.Threading.Thread.CurrentThread.ManagedThreadId}");
        }

        #region 报警信息

        public string ErrorCodeMessage(int ErrorCode, out bool IsError)
        {
            string ErrorText = string.Empty;
            IsError = false;
            if ((ErrorCode & 0x01) == 1)
            {
                ErrorText = "急停触发";
            }
            else if (((ErrorCode & 0x02) >> 1) == 1)
            {
                ErrorText = "右驱动马达异常";
            }
            else if (((ErrorCode & 0x04) >> 2) == 1)
            {
                ErrorText = "左驱动马达异常";
            }
            else if (((ErrorCode & 0x08) >> 3) == 1)
            {
                ErrorText = "滚筒过流";
            }
            else if (((ErrorCode & 0x10) >> 4) == 1)
            {
                ErrorText = "夹具电机过流";
            }
            else if (((ErrorCode & 0x20) >> 5) == 1)
            {
                ErrorText = "触边碰撞";
            }
            else if (((ErrorCode & 0x40) >> 6) == 1)
            {
                ErrorText = "安全继电器报警";
            }
            else if (((ErrorCode & 0x80) >> 7) == 1)
            {
                ErrorText = "控制器急停";
            }
            else if (((ErrorCode & 0x100) >> 8) == 1)
            {
                ErrorText = "控制器严重错误";
            }
            else if (((ErrorCode & 0x200) >> 9) == 1)
            {
                ErrorText = "控制错误";
            }
            else if (((ErrorCode & 0x400) >> 10) == 1)
            {
                ErrorText = "控制器报警";
            }
            else if (((ErrorCode & 0x800) >> 11) == 1)
            {
                ErrorText = "夹具传感器故障";
            }
            else if (((ErrorCode & 0x1000) >> 12) == 1)
            {
                ErrorText = "滚筒电机故障";
            }
            else if (((ErrorCode & 0x2000) >> 13) == 1)
            {
                ErrorText = "夹具电机故障";
            }
            else
            {
                IsError = true;
                ErrorText = "正常";
            }
            return ErrorText;
        }

        #endregion
    }
}
