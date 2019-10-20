using MercedesBenz.DataBase;
using MercedesBenz.Infrastructure;
using MercedesBenz.Models;
using MercedesBenz.SuperSocketTask.ClientCode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace MercedesBenz.SuperSocketTask
{
    public class TaskClientNDC : BaseClientTask
    {

        public override void MessageAnalysis(byte[] messageData)
        {
            List<byte[]> byteList = BytePackagedis.Unbind_Byte(messageData);
            foreach (var _byte in byteList)
            {
                switch (_byte[9])
                {
                    case 0x73:
                        Manage_SQ(_byte);  //S消息
                        break;
                    case 0x62:
                        Manage_BQ(_byte);  //B消息
                        break;
                }
                Log4NetHelper.WritendcLog(string.Join(" ", _byte.Select(s => s.ToString("X2"))));
            }
        }

        #region 处理S消息
        /// <summary>
        /// 处理S消息
        /// </summary>
        private void Manage_SQ(byte[] _byte)
        {
            int index = _byte[12] << 8 | _byte[13]; //NDC索引index
            int car_no = _byte[20]; //车号
            int magic = (_byte[16] | _byte[17]).ToString("X2").ToInt(); //阶段                   
            SystemTaskDatabase.Instance.UpdateOrder_order_magic(magic, index, car_no);
            switch (_byte[16] | _byte[17]) //magic状态
            {
                case 0x08: //任务结束
                    {
                        Responses_NDC(_byte, 0x19, 0x01); //改25号参数 为1
                        Responses_WCS(index, car_no, magic);
                    }
                    break;
                case 0x01: //AGV已分配
                case 0x11: //装货成功
                case 0x21: //卸货成功 
                    {
                        Responses_NDC(_byte, 0x19, 0x01); //改25号参数 为1
                        Responses_WCS(index, car_no, magic);
                    }
                    break;
                case 0x10: //装货请求
                case 0x20: //卸货请求 
                    {
                        UpdateLocation(_byte, index);  //10号参数 //为1
                        Responses_WCS(index, car_no, magic);
                    }
                    break;
                case 0x12:  //装货失败-操作超时 
                case 0x13: //装货地址申请（主要用于巷道） 
                case 0x14: //装货失败-检测无货 
                case 0x22: //卸货失败-操作超时 
                case 0x23: //卸货地址申请（主要用于巷道）
                case 0x24: //卸货失败-检测无货 
                case 0x34: //CARWASH 请求卸货地址（AGV 有货无任务） 
                    {
                        Responses_NDC(_byte, 0x19, 0x01); //改25号参数 为1
                    }
                    break;
                case 0x30: //任务被删除 
                case 0x31: //任务意外终止 
                    {
                        CancelTask(index);
                    }
                    break;
                case 0x32: //系统热启动 
                case 0x33: //系统冷启动 
                case 0x50: //命令结束 
                    {
                        Responses_NDC(_byte, 0x19, 0x01); //改25号参数 为1
                        //任务结束
                        SystemTaskDatabase.Instance.UpdateOrder_order_magic(50, index, car_no);
                    }
                    break;
                case 0x60: //任务取消
                    {
                        Responses_NDC(_byte, 0x19, 0x01); //改25号参数 为1
                        Responses_WCS(index, car_no, magic);
                    }
                    break;
                default:
                    {
                        Responses_NDC(_byte, 0x19, 0x01); //改25号参数 为1
                    }
                    break;
            }
        }
        #endregion

        #region 处理B消息

        /// <summary>
        /// 处理B消息
        /// </summary>
        /// <param name="_byte"></param>
        private void Manage_BQ(byte[] _byte)
        {
            int index = _byte[12] << 8 | _byte[13];
            int order_status = _byte[15].ToString("X2").ToInt(); //阶段
            switch (_byte[15])  //status 状态
            {
                case 0x01:  //命令接受 
                    {
                        int ikey = _byte[18] << 8 | _byte[19];
                        SystemTaskDatabase.Instance.UpdateOrder_Index(index, ikey); //更新index并更新status为1
                    }
                    break;
                case 0x14: //取货位不存在 参数值未被接受 
                    {
                        CancelTask(index);
                        SystemTaskDatabase.Instance.UpdateOrder_status(order_status, index);
                    }
                    break;
                case 0x07: //参数确认 
                    {
                        OrderTask _orderTask = SystemTaskDatabase.Instance.QueryOrderDNC(index);
                        if (_orderTask != null)
                        {
                            if (_orderTask.order_cancel == 100 && _orderTask.order_magic != 1)
                            {
                                _orderTask.order_cancel = 0;
                                SystemTaskDatabase.Instance.cancelOrderTask(_orderTask);
                            }
                        }
                    }
                    break;
                case 0x00: //命令拒绝 
                case 0x03: //命令结束 
                case 0x04: //命令取消 
                case 0x09: //优先级错误 
                case 0x0B: //命令缓冲区满 
                case 0x11: //严重错误，命令执行结束 
                case 0x12: //参数删除 
                case 0x13: //参数值接受 
                case 0x19: //取消命令确认 
                case 0x1A: //缺少参数 
                case 0x1B: //IKEY 重复
                case 0x23: //改变命令优先级确认 
                default:
                    {
                        SystemTaskDatabase.Instance.UpdateOrder_status(order_status, index);
                    }
                    break;
            }
        }
        #endregion

        public override void ClientTaskRun()
        {
            //所有订单
            List<OrderTask> _orderTasksAll = SystemTaskDatabase.Instance.QueryOrderDNC();
            if (_orderTasksAll != null && _orderTasksAll.Count() > 0)
            {
                //执行中订单
                List<OrderTask> ExecuteorderTasks = _orderTasksAll.Where(p => p.order_status != -1 && p.order_ikey != -1 && p.order_index != -1).ToList();
                //已发送未回应订单
                List<OrderTask> _SendorderTasks = _orderTasksAll.Where(p => p.order_index == -1 && p.order_ikey != -1 /*&& p.order_status == -1*/).ToList();

                List<OrderTask> NDCTask = _orderTasksAll.Where(p => p.order_ikey != -1).ToList();
                //发送任务
                //if (ExecuteorderTasks.Count() < 1 || _SendorderTasks.Count() < 1)
                if (NDCTask.Count() == 0)
                {
                    //未处理订单
                    List<OrderTask> _orderTasks = _orderTasksAll.Where(p => p.order_magic == -1 && p.order_ikey == -1 && p.order_index == -1).OrderByDescending(p => p.order_priority).ToList();
                    //已处理订单
                    List<OrderTask> _ExecuteorderTasks = _orderTasksAll.Where(p => p.order_magic != -1 || p.order_ikey != -1).ToList();
                    if (_orderTasks.Count() > 0)
                    {
                        _ExecuteorderTasks.ForEach(p => p.order_ikey = p.order_ikey & 0x00FF);
                        int IkeyMax = _ExecuteorderTasks.Count() > 0 ? _ExecuteorderTasks.Max(p => p.order_ikey) : 0; //取低位
                        int IkeyMin = _ExecuteorderTasks.Count() > 0 ? _ExecuteorderTasks.Min(p => p.order_ikey) : 0; //取低位
                        var awaitexecuteTask = _orderTasks.OrderBy(p => p.order_time).FirstOrDefault();
                        if (awaitexecuteTask != null)
                        {
                            var bufferStatus = SystemTaskDatabase.Instance.QueryBufffer();
                            if (awaitexecuteTask.order_type == OrderTaskType.Out) //出库任务
                            {
                                if (!agvIsRunOut())
                                {
                                    int boxType = GroupMessage.TaskboxType(awaitexecuteTask.order_getLocation);
                                    if (!TaskOutagv(boxType, bufferStatus))
                                    {
                                        bufferStatus.Where(p => p.Buff_number > 3 && p.Buff_type == boxType && p.Buff_status == 0).ToList()
                                        .ForEach(p =>
                                        {
                                            awaitexecuteTask.order_putLocation = p.Buff_ndcSite.ToString();
                                            if (SystemTaskDatabase.Instance.Updateorder_putLocation(awaitexecuteTask))
                                            {
                                                outputTask(awaitexecuteTask, IkeyMax, IkeyMin);
                                            }
                                        });
                                    }
                                }
                            }
                            else if (awaitexecuteTask.order_type == OrderTaskType.In) //入库任务
                            {
                                bufferStatus.Where(p => p.Buff_ndcSite == awaitexecuteTask.order_getLocation.ToInt() && p.Buff_status == 1).ToList()
                                    .ForEach(p =>
                                    {
                                        awaitexecuteTask.order_getLocation = p.Buff_ndcSite.ToString();
                                        outputTask(awaitexecuteTask, IkeyMax, IkeyMin);
                                    });
                            }
                            else
                            {
                                //outputTask(awaitexecuteTask, IkeyMax, IkeyMin);
                            }
                        }
                    }
                }
                //发送任务后120秒未回应 重新发送订单(更改ikey为-1)
                _SendorderTasks.ForEach(p =>
                {
                    long ThisUtcTime = UTC.ConvertDateTimeLong(DateTime.Now);
                    if (ThisUtcTime - p.order_utctime >= 180)
                    {
                        SystemTaskDatabase.Instance.UpdateOrder_ikey(-1, p.order_guid);
                    }
                });

                //删除任务
                _orderTasksAll.ForEach(p =>
                {
                    if (p.order_cancel == -1)
                    {
                        SystemTaskDispose.Send(IPType.ndc, GroupMessage.DeleteTask(p.order_index));
                    }
                });

                //删除完成订单（任务完成/任务取消/取货位或放货位不存在）8完成 60取消 20货位不存在
                _orderTasksAll.Where(p => p.order_magic == 8 || p.order_magic == 60 || p.order_status == 20).ToList().ForEach(order =>
                {
                    if (order.order_magic == 8)
                    {
                        if (order.order_type == OrderTaskType.Out) //生成AGV出库任务订单
                        {
                            var bufferStatus = SystemTaskDatabase.Instance.QueryBufffer();
                            var bufferInfo = bufferStatus.FirstOrDefault(p => p.Buff_ndcSite == order.order_putLocation.ToInt());
                            if (bufferInfo != null)
                            {
                                if (SystemTaskDatabase.Instance.UpdateBufferStatus(bufferInfo.Buff_number, 1)) //更新库存状态为5
                                {
                                    var StationInfo = SystemTaskDatabase.Instance.QuerySiteConfiguration().FirstOrDefault(p => p.station_number == 2);
                                    if (StationInfo != null)
                                    {
                                        carryPlatfromTaskmsg platfromTaskmsg = new carryPlatfromTaskmsg()
                                        {
                                            areaCode = order.order_arecore,
                                            boxId = order.order_boxId,
                                            s_station = bufferInfo.Buff_Site.ToString(),
                                            d_station = StationInfo.station_agvSite.ToString(),
                                            wmsId = order.order_wmsId,
                                            priority = order.order_priority,
                                            sourceCode = order.order_sourceCode
                                        };
                                        //原逻辑   //生成AGV出库搬运订单
                                        if (SystemTaskDatabase.Instance.IssueOrderTask(platfromTaskmsg, OrderType.AGV, OrderTaskType.Out))
                                        {
                                            SystemTaskDatabase.Instance.DeleteOrder_history(order);
                                        }
                                        //Task.Delay(42000).ContinueWith((task, Taskmsg) =>
                                        //{
                                        //    SystemTaskDatabase.Instance.IssueOrderTask(Taskmsg as carryPlatfromTaskmsg, OrderType.AGV, OrderTaskType.Out);
                                        //}, platfromTaskmsg);
                                        SystemTaskDatabase.Instance.DeleteOrder_history(order);
                                    }
                                }
                            }
                        }
                        else
                        {
                            SystemTaskDatabase.Instance.DeleteOrder_history(order);
                        }
                    }
                    else
                    {
                        SystemTaskDatabase.Instance.DeleteOrder_history(order);
                    }
                });
            }

            var orderInfo = SystemTaskDatabase.Instance.QueryOrderDNCCarno(1);
            var carInfo = new tb_carinfo_model();
            carInfo.Info_number = 1;
            carInfo.Info_errormes = "正常";
            carInfo.Info_Iserror = 0;
            if (orderInfo.Count() > 0)
            {
                carInfo.Info_status = "执行任务中";
            }
            else
            {
                carInfo.Info_status = "无任务";
            }
            carInfo.Info_voltage = 42.3;
            carInfo.Info_Ischarge = 0;
            carInfo.Info_cartype = "NDC";
            SystemTaskDatabase.Instance.tb_carinfo_Update(carInfo);
        }

        /// <summary>
        /// 出库缓存范围是否有车存在
        /// </summary>
        /// <returns></returns>
        public bool agvIsRunOut()
        {
            bool IsRun = false;
            foreach (var agvInfoitem in SystemTaskDispose.agvInfoList.Values)
            {
                if (agvInfoitem.ThisStation == 155 ||
                    agvInfoitem.ThisStation == 166 ||
                    agvInfoitem.ThisStation == 165 ||
                    agvInfoitem.ThisStation == 188 ||
                    agvInfoitem.ThisStation == 159 ||
                    agvInfoitem.ThisStation == 160 ||
                    agvInfoitem.ThisStation == 164 ||
                    agvInfoitem.ThisStation == 189 ||
                    agvInfoitem.ThisStation == 160 ||
                    agvInfoitem.ThisStation == 163 ||
                    agvInfoitem.ThisStation == 162 ||
                    (agvInfoitem.ThisStation == 185 &&
                    agvInfoitem.NavigationStatus == navigation.This))
                {
                    IsRun = true;
                }
            }
            return IsRun;
        }


        //判断是否存在未取走的托盘
        private bool TaskOutagv(int boxtype, List<BufferInfo> buffers)
        {
            bool task = false;
            List<OrderTask> _orderTasksAll = SystemTaskDatabase.Instance.QueryOrderAGV();
            _orderTasksAll.ForEach(p =>
            {
                var taskBuff = buffers.FirstOrDefault(s => s.Buff_Site == p.order_getSite.ToInt());
                if (taskBuff != null)
                {
                    if ((p.order_genre == OrderType.AGV && p.order_type == OrderTaskType.Out && p.order_magic == -1) && taskBuff.Buff_type == boxtype)
                    {
                        task = true;
                    }
                }
            });
            return task;
        }


        /// <summary>
        /// 生成NDC任务并发送
        /// </summary>
        /// <param name="_orderTasks"></param>
        /// <param name="IkeyMax"></param>
        /// <param name="IkeyMin"></param>
        public void outputTask(OrderTask _orderTasks, int IkeyMax, int IkeyMin)
        {
            byte[] TcpMessage_Q = GroupMessage.GroupMessage_Q(_orderTasks, IkeyMax, IkeyMin);//组Q消息   
                                                                                             //ikey、取货、放货点不为空即认为该消息有效
            if (TcpMessage_Q[17] != 0x00 && TcpMessage_Q[19] != 0x00 && TcpMessage_Q[21] != 0x00)
            {
                int iKey = TcpMessage_Q[16] << 8 | TcpMessage_Q[17];
                SystemTaskDatabase.Instance.UpdateOrder_ikey(iKey, _orderTasks.order_guid);
                SystemTaskDispose.Send(IPType.ndc, TcpMessage_Q);
                Log4NetHelper.WriteTaskLog($"WCS取货位：{_orderTasks.order_getLocation},WCS放货位：{_orderTasks.order_putLocation},NDC取货位：{TcpMessage_Q[18] << 8 | TcpMessage_Q[19]},NDC放货位：{TcpMessage_Q[20] << 8 | TcpMessage_Q[21]}");
            }
        }


        /// <summary>
        /// 状态更新回应WCS
        /// </summary>
        /// <param name="Index"></param>
        /// <param name="car_no"></param>
        /// <returns></returns>
        private void Responses_WCS(int Index, int car_no, int magic)
        {
            OrderTask _orderTask = SystemTaskDatabase.Instance.QueryOrderDNC(Index);
            if ((_orderTask != null && _orderTask.order_carno == car_no) || (_orderTask != null && _orderTask.order_carno == -1))
            {
                SystemTaskDatabase.Instance.UpdateOrder_order_magic(magic, Index, car_no);
                //向WCS回应AGV状态更新
                if (magic == 1)
                {
                    //var message = GroupMessage.GroupMessage_WCS(_orderTask, magic);
                    //TaskDispose.Instance.Send(IPType.wcs, GroupMessage.MessageToByte(message));
                    //SystemTaskDatabase.Instance.InsertMessage(message);
                    //Thread.Sleep(1000);
                    //message = GroupMessage.GroupMessage_WCS(_orderTask, 2);
                    //TaskDispose.Instance.Send(IPType.wcs, GroupMessage.MessageToByte(message));
                    //SystemTaskDatabase.Instance.InsertMessage(message);
                }
                else if (magic == 8)
                {
                    var message = GroupMessage.GroupMessage_WCS(_orderTask, 3);
                    SystemTaskDispose.Send(IPType.wcs, GroupMessage.MessageToByte(message));
                    SystemTaskDatabase.Instance.InsertMessage(message);
                }
                else if (magic == 60)
                {
                    var message = GroupMessage.GroupMessage_WCS(_orderTask, 4);
                    SystemTaskDispose.Send(IPType.wcs, GroupMessage.MessageToByte(message));
                    SystemTaskDatabase.Instance.InsertMessage(message);
                }
                else if (magic == 10 || magic == 20)
                {
                    if (_orderTask.order_type == OrderTaskType.In)
                    {
                        var bufferStatus = SystemTaskDatabase.Instance.QueryBufffer();
                        var bufferInfo = bufferStatus.FirstOrDefault(p => p.Buff_ndcSite == _orderTask.order_getLocation.ToInt());
                        if (bufferInfo != null)
                        {
                            SystemTaskDatabase.Instance.UpdateBufferStatus(bufferInfo.Buff_number, 0);
                        }
                    }
                }
            }

            // 根据车号查询订单
            List<OrderTask> _orderTaskCar = SystemTaskDatabase.Instance.QueryOrderDNCCarno(car_no);
            _orderTaskCar.ForEach(order =>
            {
                if ((order.order_index != Index || order.order_ikey != _orderTask.order_ikey) && order.order_ikey != -1 && order.order_status != -1 && order.order_magic != -1)
                {
                    var message = GroupMessage.GroupMessage_WCS(order, 3);
                    SystemTaskDispose.Send(IPType.wcs, GroupMessage.MessageToByte(message));
                    SystemTaskDatabase.Instance.DeleteOrder_history(order); //移动订单到历史订单
                    SystemTaskDatabase.Instance.InsertMessage(message);
                }
            });
        }

        /// <summary>
        /// 任务被删除/意外中止/库位不存在
        /// </summary>
        private void CancelTask(int Index)
        {
            OrderTask _orderTask = SystemTaskDatabase.Instance.QueryOrderDNC(Index);
            if (_orderTask != null)
            {
                var message = GroupMessage.GroupMessage_WCS(_orderTask, 4);
                SystemTaskDispose.Send(IPType.wcs, GroupMessage.MessageToByte(message));
                SystemTaskDatabase.Instance.DeleteOrder_history(_orderTask); //移动订单到历史订单
                SystemTaskDatabase.Instance.InsertMessage(message);
            }
        }


        /// <summary>
        /// 回应NDC系统 M消息
        /// </summary>
        /// <param name="_byte">数据帧</param>
        /// <param name="parameter">参数</param>
        /// <param name="value">值</param>
        private void Responses_NDC(byte[] _byte, byte parameter, byte value)
        {
            SystemTaskDispose.Send(IPType.ndc, GroupMessage.GroupMessage_M(_byte, mes =>
            {
                mes[15] = parameter; //参数
                mes[17] = value;  //值
                return mes;
            }));
        }

        //更新位置
        private void UpdateLocation(byte[] _byte, int index)
        {
            OrderTask _orderTask = SystemTaskDatabase.Instance.QueryOrderDNC(index);
            if (_orderTask != null)
            {
                if (_orderTask.order_cancel == 100)
                {
                    SystemTaskDispose.Send(IPType.ndc, GroupMessage.GroupMessage_M(_byte, mes =>
                    {
                        mes[15] = 0x0A; //参数
                        mes[17] = 0x02;  //值
                        return mes;
                    }));
                    Thread.Sleep(200);
                    if (_orderTask.order_magic != 10)
                    {
                        var getLocation = _orderTask.order_getLocation.Contains("F") ? _orderTask.order_getLocation.Remove(0, 1) : _orderTask.order_getLocation;
                        Update_SiteNDC(_byte, _orderTask.order_type == OrderTaskType.In ? int.Parse(getLocation) : int.Parse(GroupMessage.GoodsAllocationAnalysis(getLocation)));
                    }
                    else
                    {
                        var putLocation = _orderTask.order_putLocation.Contains("F") ? _orderTask.order_putLocation.Remove(0, 1) : _orderTask.order_putLocation;
                        Update_SiteNDC(_byte, _orderTask.order_type == OrderTaskType.Out ? int.Parse(putLocation) : int.Parse(GroupMessage.GoodsAllocationAnalysis(putLocation)));
                    }
                }
                else
                {
                    SystemTaskDispose.Send(IPType.ndc, GroupMessage.GroupMessage_M(_byte, mes =>
                    {
                        mes[15] = 0x19; //参数
                        mes[17] = 0x01;  //值
                        return mes;
                    }));
                }
            }
            else
            {
                SystemTaskDispose.Send(IPType.ndc, GroupMessage.GroupMessage_M(_byte, mes =>
                {
                    mes[15] = 0x19; //参数
                    mes[17] = 0x01;  //值
                    return mes;
                }));
            }
        }


        /// <summary>
        /// 更新货位
        /// </summary>
        /// <param name="_byte"></param>
        /// <param name="parameter"></param>
        /// <param name="LocationValue"></param>
        private void Update_SiteNDC(byte[] _byte, int LocationValue)
        {
            SystemTaskDispose.Send(IPType.ndc, GroupMessage.GroupMessage_M(_byte, mes =>
            {
                mes[15] = 0x0B; // 更改11号参数为 新的取货地址 
                mes[16] = Convert.ToByte((LocationValue & 0xFF00) >> 8);  //新货位值
                mes[17] = Convert.ToByte(Convert.ToInt32(LocationValue) & 0x00FF);  //新货位值
                return mes;
            }));
        }
    }
}
