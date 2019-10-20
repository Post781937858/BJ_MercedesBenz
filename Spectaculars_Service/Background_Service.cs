using Microsoft.AspNet.SignalR.Client;
using Microsoft.Owin.Hosting;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using MercedesBenz.DataBase;
using MercedesBenz.Models;

namespace BJ_MercedesBenz_Spectaculars
{
    public class Background_Service
    {
        public static Background_Service _service { get; } = new Background_Service();
        private string IPaddress = ConfigurationManager.AppSettings["IPaddress"];
        private IHubProxy HubProxy;
        public async void ServiceStart()
        {
            bool IsRun = true;
            try
            {
                await MessageConnectAsync();
                ConsoleLogHelper.WriteSucceedLog("数据推送客户端开启成功！");
            }
            catch (Exception ex)
            {
                IsRun = false;
                ConsoleLogHelper.WriteErrorLog("数据推送客户端开启失败，请检查服务端是否开启！");
                Log4NetHelper.WriteErrorLog(ex.Message, ex);
            }
            await Task.Run(() =>
            {
                while (true)
                {
                    try
                    {
                        if (IsRun)
                            UpdateMessageService();
                        Thread.Sleep(3000);
                    }
                    catch (Exception ex)
                    {
                        Log4NetHelper.WriteErrorLog(ex.Message, ex);
                    }
                }
            });
        }


        public async Task MessageConnectAsync()
        {
            HubConnection Connection = new HubConnection(IPaddress);
            // 创建一个集线器代理对象
            HubProxy = Connection.CreateHubProxy("ChatHub");

            // 供服务端调用，将消息输出到消息列表框中
            HubProxy.On<string, string>("AddMessage", (name, message) =>
            {
                //ConsoleLogHelper.WriteInfoLog(message);
            });
            await Connection.Start();
        }
        public void UpdateMessageService()
        {
            EnvironmentInfo environmentInfo = SystemTaskDatabase.Instance.QueryenvironmentInfos();
            List<tb_warehousesstatus_model> WarehousesstatusesList = SystemTaskDatabase.Instance.QueryWarehouses();
            tb_warehousesstatus_model GalaxisWarehouses = WarehousesstatusesList.FirstOrDefault(p => p.wh_Id == 1);
            tb_warehousesstatus_model RGVWarehouses = WarehousesstatusesList.FirstOrDefault(p => p.wh_Id == 2);
            tb_warehousesstatus_model NDCWarehouses = WarehousesstatusesList.FirstOrDefault(p => p.wh_Id == 3);
            var MessageDate = new MessageUpdateData()
            {
                deliveryLineinfo = new DeliveryLine()
                {
                    IsRun = false,
                    ErrorMes = "正常",
                    IsError = false
                },
                forkCarinfoList = new List<ForkCar>()
                {
                    new ForkCar() { CarNumber = 1, Voltage =  24.5, ErrorMes = "正常", Status="无任务",IsCharge = false,IsError = false },
                    new ForkCar() { CarNumber = 2, Voltage = 25.5, ErrorMes = "正常", Status = "无任务",IsCharge = false ,IsError = false},
                    new ForkCar() { CarNumber = 3, Voltage = 24.5, ErrorMes = "正常", Status = "无任务",IsCharge = false ,IsError = false},
                },
                RGVCarinfoList = new List<RGVCar>()
                {
                    new RGVCar() { CarNumber = 1, Voltage = 24.5, ErrorMes = "正常", Status="无任务",IsCharge = false,IsError = false },
                    new RGVCar() { CarNumber = 2, Voltage = 25.5, ErrorMes = "正常", Status = "无任务",IsCharge = false,IsError = false },
                    new RGVCar() { CarNumber = 3, Voltage = 24.5, ErrorMes = "正常", Status = "无任务",IsCharge = false ,IsError = false},
                },
                shuttleCarinfoList = new List<ShuttleCar>()
                {
                    new ShuttleCar() { CarNumber = 1, Voltage = 24.5, ErrorMes = "正常", Status="无任务",IsCharge = false ,IsError = false},
                    new ShuttleCar() { CarNumber = 2, Voltage =  25.5, ErrorMes = "正常", Status = "无任务",IsCharge = false ,IsError = false},
                    new ShuttleCar() { CarNumber = 3, Voltage = 24.5, ErrorMes = "正常", Status = "无任务",IsCharge = false,IsError = false },
                    new ShuttleCar() { CarNumber = 4, Voltage = 26.1, ErrorMes = "正常", Status = "无任务" ,IsCharge = false ,IsError = false},
                    new ShuttleCar() { CarNumber = 5, Voltage =  26.8, ErrorMes = "正常", Status = "无任务",IsCharge = false,IsError = false }
                },
                WarehouseStatusesList = new List<warehouseStatus>()
                {
                    new warehouseStatus()
                    {
                        warehouseName="穿梭车库",
                        warehouseData=new List<Data>()
                        {
                            new Data() {  name ="满库率", value = GalaxisWarehouses == null ? 0 :GalaxisWarehouses.wh_full },
                            new Data() { name ="空库率", value=GalaxisWarehouses == null ? 0 :GalaxisWarehouses.wh_empty }
                        }
                    },
                    new warehouseStatus()
                    {
                        warehouseName="阁楼库",
                        warehouseData=new List<Data>()
                        {
                            new Data() {  name ="满库率",  value = RGVWarehouses==null?0:RGVWarehouses.wh_full },
                             new Data() {  name ="空库率", value = RGVWarehouses==null?0:RGVWarehouses.wh_empty }
                        }
                    },
                    new warehouseStatus()
                    {
                        warehouseName="重载库",
                        warehouseData=new List<Data>()
                        {
                            new Data() {   name ="满库率", value = NDCWarehouses==null?0:NDCWarehouses.wh_full },
                             new Data(){ name ="空库率", value = NDCWarehouses==null?0:NDCWarehouses.wh_empty }
                        }
                    }
                },
                Humidity = environmentInfo.info_humidity,
                Temperature = environmentInfo.info_temperature
            };
            List<tb_carinfo_model> CarInfoList = SystemTaskDatabase.Instance.QueryCarInfos();
            var agvInfoList = CarInfoList.Where(p => p.Info_cartype == "AGV").ToList();
            if (agvInfoList.Count > 0)
            {
                var CarList = new List<agvinfo>();
                agvInfoList.ForEach(p =>
                {
                    CarList.Add(new agvinfo()
                    {
                        CarNumber = p.Info_number,
                        ErrorMes = string.IsNullOrEmpty(p.Info_errormes) ? "正常" : p.Info_errormes,
                        IsCharge = p.Info_Ischarge == 1 ? true : false,
                        IsError = p.Info_Iserror == 1 ? true : false,
                        Status = string.IsNullOrEmpty(p.Info_status) ? "无任务" : p.Info_status,
                        Voltage = p.Info_voltage
                    });
                });
                MessageDate.agvInfoList = CarList;
            }
            HubProxy.Invoke("Send", "MessageSrvice", MessageDate.ToJson());
        }
    }
}
