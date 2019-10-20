using System;
using System.Linq;
using MercedesBenz.Infrastructure;
using MercedesBenz.Models;
using MercedesBenz.SuperSocketTask.ClientCode;
using MercedesBenz.SuperSocketTask.ServerCode;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace MercedesBenz.SuperSocketTask
{
    public class SystemTaskDispose
    {
        //系统日志委托
        public static Action<string, bool> SystemLog;

        /// <summary>
        /// TCP服务端列表
        /// </summary>
        public static Dictionary<IPType, BaseAppService> _BackgroundTcpServer = new Dictionary<IPType, BaseAppService>();

        /// <summary>
        /// TCP客户端列表
        /// </summary>
        public static Dictionary<IPType, BaseEasyClient> _BackgroundTcpClient = new Dictionary<IPType, BaseEasyClient>();

        /// <summary>
        /// 所有AGV状态信息
        /// </summary>
        public static ConcurrentDictionary<int, agvInfo> agvInfoList = new ConcurrentDictionary<int, agvInfo>();

        /// <summary>
        /// 出/入库门状态
        /// </summary>
        public static Dictionary<DoorType, DoorInfo> DoorInfoArray = new Dictionary<DoorType, DoorInfo>();



        //开启服务
        public static void StartService()
        {
            try
            {
                _BackgroundTcpServer.Clear();
                _BackgroundTcpClient.Clear();
                agvInfoList.Clear();
                DoorInfoArray.Clear();
                var CarServiceConfig = SystemConfiguration.CarService();
                for (int i = 0; i < CarServiceConfig.Count(); i++)
                {
                    if (CarServiceConfig[i].ON)
                        agvInfoList.GetOrAdd(CarServiceConfig[i].CarNumber, new agvInfo() { agvNumber = CarServiceConfig[i].CarNumber, CarIptype = CarServiceConfig[i].type });
                }
                DoorInfoArray.Add(DoorType.In, new DoorInfo() { DoorStatus = DoorStatus.Close, doorType = DoorType.In });
                DoorInfoArray.Add(DoorType.Out, new DoorInfo() { DoorStatus = DoorStatus.Close, doorType = DoorType.Out });
                _BackgroundTcpServer.Add(IPType.server, new BaseAppService(new TaskServiceWCS()));
                _BackgroundTcpServer.Add(IPType.agvserver, new BaseAppService(new TaskServiceAGV()));
                _BackgroundTcpClient.Add(IPType.wcs, new BaseEasyClient(new TaskClientWCS(), IPType.wcs));
                _BackgroundTcpClient.Add(IPType.ndc, new BaseEasyClient(new TaskClientNDC(), IPType.ndc));
                _BackgroundTcpClient.Add(IPType.InSite, new BaseEasyClient(new TaskClientInDoor(), IPType.InSite));
                _BackgroundTcpClient.Add(IPType.OutSite, new BaseEasyClient(new TaskClientOutDoor(), IPType.OutSite));

                _BackgroundTcpServer.Keys.ToList().ForEach(serverKey =>
                {
                    BaseAppService serverItem = _BackgroundTcpServer[serverKey];
                    ServiceModel serviceModel = SystemConfiguration.Distance_serve(serverKey);
                    if (serverItem.Setup(serviceModel.IP, serviceModel.Port))
                    {
                        if (!serverItem.Start())
                        {
                            SystemLog($"【{serviceModel.IP}:{serviceModel.Port}】 开启失败，请重新开启", true);
                        }
                    }
                });
                _BackgroundTcpClient.Values.ToList().ForEach(clientItem =>
                {
                    clientItem.StartAsync();
                });
            }
            catch (Exception ex) { Log4NetHelper.WriteErrorLog(ex.Message, ex); }
        }


        //停止服务
        public static void StopService()
        {
            try
            {
                _BackgroundTcpServer.Keys.ToList().ForEach(serverKey =>
                {
                    BaseAppService serverItem = _BackgroundTcpServer[serverKey];
                    serverItem.Stop();
                });
                _BackgroundTcpClient.Values.ToList().ForEach(clientItem =>
                {
                    clientItem.StopAsync();
                });
            }
            catch (Exception ex) { Log4NetHelper.WriteErrorLog(ex.Message, ex); }
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="pType"></param>
        /// <param name="mes"></param>
        public static void Send(IPType pType, byte[] mes)
        {
            try
            {
                if (_BackgroundTcpClient[pType].GetEasyClient.IsConnected)
                    _BackgroundTcpClient[pType].GetEasyClient.Send(mes, 0, mes.Length);
            }
            catch (Exception ex) { Log4NetHelper.WriteErrorLog(ex.Message, ex); }
        }
    }
}
