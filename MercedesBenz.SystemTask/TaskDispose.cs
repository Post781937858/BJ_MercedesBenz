using MercedesBenz.Infrastructure;
using MercedesBenz.Models;
using MercedesBenz.SystemTask.Client;
using MercedesBenz.SystemTask.Client.Base;
using MercedesBenz.SystemTask.Server.Base;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;

namespace MercedesBenz.SystemTask
{
    /// <summary>
    /// 任务管理
    /// </summary>
    public class TaskDispose
    {
        private TaskDispose() { }

        /// <summary>
        /// TCP客户端列表
        /// </summary>
        public Dictionary<IPType, BaseTcpClient> _BackgroundTcpClient = new Dictionary<IPType, BaseTcpClient>();

        /// <summary>
        /// 所有AGV状态信息
        /// </summary>
        public ConcurrentDictionary<int, agvInfo> agvInfoList = new ConcurrentDictionary<int, agvInfo>();

        /// <summary>
        /// TCP服务端列表
        /// </summary>
        public Dictionary<IPType, BaseTcpClientServer> _BackgroundTcpServer = new Dictionary<IPType, BaseTcpClientServer>();

        /// <summary>
        /// 出/入库门状态
        /// </summary>
        public Dictionary<DoorType, DoorInfo> DoorInfoArray = new Dictionary<DoorType, DoorInfo>();

        //任务单例
        public static TaskDispose Instance { get; } = new TaskDispose();

        public  Action<Action> UIInvok;

        public Action<string> OutputLog;

        /// <summary>
        /// 开启任务
        /// </summary>
        public void TaskStart()
        {
            //添加任务
            try
            {
                _BackgroundTcpClient.Clear();
                agvInfoList.Clear();
                _BackgroundTcpServer.Clear();
                DoorInfoArray.Clear();
                _BackgroundTcpClient.Add(IPType.OutSite, new OutConnectionManage(IPType.OutSite));
                _BackgroundTcpClient.Add(IPType.InSite, new InConnectionManage(IPType.InSite));
                _BackgroundTcpClient.Add(IPType.ndc, new OrderTaskNDC(IPType.ndc));
                _BackgroundTcpClient.Add(IPType.wcs, new OrderTaskClientWCS(IPType.wcs));
                _BackgroundTcpServer.Add(IPType.server, new OrderTaskServiceWCS(IPType.server));
                _BackgroundTcpServer.Add(IPType.agvserver, new OrderTaskAGV(IPType.agvserver));
                var CarServiceConfig = SystemConfiguration.CarService();
                for (int i = 0; i < CarServiceConfig.Count(); i++)
                {
                    if (CarServiceConfig[i].ON)
                        agvInfoList.GetOrAdd(CarServiceConfig[i].CarNumber, new agvInfo() { CarIptype = CarServiceConfig[i].type });
                }
                DoorInfoArray.Add(DoorType.In, new DoorInfo() { DoorStatus = DoorStatus.Close, doorType = DoorType.In });
                DoorInfoArray.Add(DoorType.Out, new DoorInfo() { DoorStatus = DoorStatus.Close, doorType = DoorType.Out });
                _BackgroundTcpClient.Values.ToList().ForEach(p => p.Start()); //启动任务
                _BackgroundTcpServer.Values.ToList().ForEach(p => p.Start());
                Log4NetHelper.WriteDebugLog("服务启动");
                ConsoleLogHelper.WriteSucceedLog("The service start");
            }
            catch (Exception ex)
            {
                Log4NetHelper.WriteErrorLog(ex.Message, ex);
                Log4NetHelper.WriteDebugLog($"启动出错:errormessage：{ ex.Message}堆栈:{ex.StackTrace}");
                ConsoleLogHelper.WriteErrorLog($"Service startup error！errormessage：{ ex.Message}\r\n堆栈:{ex.StackTrace}");
            }
        }

        /// <summary>
        /// 结束任务
        /// </summary>
        public void TaskClose()
        {
            _BackgroundTcpServer.Values.ToList().ForEach(p => p.Stop());
            _BackgroundTcpClient.Values.ToList().ForEach(p => p.Stop());
            Log4NetHelper.WriteDebugLog("服务停止");
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="pType"></param>
        /// <param name="mes"></param>
        public void Send(IPType pType, byte[] mes)
        {
            _BackgroundTcpClient[pType].Send(mes);
        }

        /// <summary>
        /// 向指定客户端广播报文
        /// </summary>
        /// <param name="pType"></param>
        /// <param name="mes"></param>
        public void SendServer(IPType pType, byte[] mes, IPType SendType)
        {
            _BackgroundTcpServer[pType].Send(SendType, mes);
        }
    }
}
