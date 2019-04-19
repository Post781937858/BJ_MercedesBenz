using MercedesBenz.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using MercedesBenz.Models;
using System.Threading;

namespace MercedesBenz.SystemTask
{
    /// <summary>
    /// 任务处理
    /// </summary>
    public class TaskDispose
    {
        private AsyncTcpClientTask clientTask { get; set; } //异步Tcp客户端任务处理
        private AsyncTcpServerTask serverTask { get; set; } //异步Tcp服务端任务处理
        private CancellationTokenSource CTSTask { get; set; } //用于取消线程
        private Queue<IssueTaskmsg> OrderList { get; set; }  //订单队列
        private List<Task> mesTask { get; set; }
        private List<agvInfo> ListagvInfo { get; set; }

        public TaskDispose()
        {
            clientTask = new AsyncTcpClientTask();
            clientTask.RequestWCS_Mes = RequestWCS_Dispose;
            clientTask.RequestNDC_Mes = RequestNDC_Dispose;
            clientTask.RequestAGV_Mes = RequestAGV_Dispose;
            serverTask = new AsyncTcpServerTask();
            OrderList = new Queue<IssueTaskmsg>();
            mesTask = new List<Task>();
            CTSTask = new CancellationTokenSource();
            ListagvInfo = new List<agvInfo>();
        }

        /// <summary>
        /// 开启任务
        /// </summary>
        public void OpenSystemTask()
        {
            CTSTask.Cancel();
            Task.WaitAll(mesTask.ToArray());
            mesTask.Clear();
            ListagvInfo.Clear();
            clientTask.OpenServer();
            CTSTask = new CancellationTokenSource();
            if (SystemConfiguration.OnWcs)
            {
                mesTask.Add(Task.Run(() =>
                {
                    while (!CTSTask.IsCancellationRequested)
                    {
                        WCSBackgroundTask();
                        Thread.Sleep(1000);
                    }
                }, CTSTask.Token));
            }
            if (SystemConfiguration.OnAgv)
            {
                mesTask.Add(Task.Run(() =>
                {
                    while (!CTSTask.IsCancellationRequested)
                    {
                        agvBackgroundTask();
                        Thread.Sleep(1000);
                    }
                }, CTSTask.Token));
            }
            if (SystemConfiguration.OnNdc)
            {
                mesTask.Add(Task.Run(() =>
                {
                    while (!CTSTask.IsCancellationRequested)
                    {
                        ndcBackgroundTask();
                        Thread.Sleep(1000);
                    }
                }, CTSTask.Token));
            }
        }

        /// <summary>
        /// 关闭任务
        /// </summary>
        public void CloseSystemTask()
        {
            CTSTask.Cancel();
            Task.WaitAll(mesTask.ToArray());
            //clientTask.CloseServer();
            //serverTask.CloseServer();
        }


        /// <summary>
        /// 处理WCS请求
        /// </summary>
        /// <param name="mes"></param>
        public void RequestWCS_Dispose(string mes)
        {
            if (mes.Contains("messageName") && mes.Contains("Id"))
            {
                var model_mes = JsonConvert.DeserializeObject<BaseMessage>(mes);
                switch (model_mes.messageName)
                {
                    case "issuetask":    //下发任务
                        OrderList.Enqueue(JsonConvert.DeserializeObject<IssueTaskmsg>(mes));
                        break;
                    case "verifytask":   //任务状态回应

                        break;
                    case "canceltask":    //取消任务

                        break;
                }
                SendWCS(model_mes);
            }
        }

        /// <summary>
        /// 处理AGV请求
        /// </summary>
        /// <param name="mes"></param>
        public void RequestAGV_Dispose(byte[] mes)
        {

            List<byte[]> mes_Byte = BytePackagedis.AnalysisByte(mes);
            Console.WriteLine("*********************************************************");
            foreach (byte[] byteitem in mes_Byte)
            {
                Console.WriteLine(string.Join(" ", byteitem.Select(p => p.ToString("X2"))));
                UpdateagvInfo(byteitem);
            }
            Console.WriteLine("*********************************************************");

        }

        /// <summary>
        /// 处理NDC请求
        /// </summary>
        /// <param name="mes"></param>
        public void RequestNDC_Dispose(byte[] mes)
        {
            


        }



        /// <summary>
        ///WCS后台任务处理
        /// </summary>
        public void WCSBackgroundTask()
        {
            TaskStatic mesmodel = new TaskStatic()
            {
                ordermark = "MK201910110256655",
                soleId = 3,
                agvnumber = 1,
                Id = 9,
                messageName = "updatetask",
                ordertype = 0,
                state = 1
            };
            var mes = JsonConvert.SerializeObject(mesmodel);
            if (clientTask.asyncTcpClient[IPType.wcs].Connected)
                clientTask.asyncTcpClient[IPType.wcs].Send(Encoding.Default.GetBytes(mes));
            Thread.Sleep(3000);
        }


        /// <summary>
        /// agv后台任务处理
        /// </summary>
        public void agvBackgroundTask()
        {
            byte[] Querybyte = { 0x01, 0x03, 0x00, 0x00, 0x00, 0x0A, 0x00, 0x00 };
            if (clientTask.asyncTcpClient[IPType.agv].Connected)
                clientTask.asyncTcpClient[IPType.agv].Send(Querybyte.CalCrc());
            Thread.Sleep(5000);
        }

        /// <summary>
        /// agv后台任务处理
        /// </summary>
        public void ndcBackgroundTask()
        {

        }


        #region WCS回应

        public void SendWCS(BaseMessage message)
        {
            var mes = JsonConvert.SerializeObject(new Responsemes() { Id = message.Id, comm_state = 0, result = 0 });
            if (clientTask.asyncTcpClient[IPType.wcs].Connected)
                clientTask.asyncTcpClient[IPType.wcs].Send(Encoding.Default.GetBytes(mes));
        }



        #endregion


        private void UpdateagvInfo(byte[] message)
        {
            if (message.CheckStrCrc())
            {
                int _agvNumber = message[4].ToString().TransformInt();
                agvInfo _agvInfo = ListagvInfo.FirstOrDefault(p => p.agvNumber == _agvNumber);
                if (_agvInfo != null)
                {
                    agvInfo agv = new agvInfo();
                    agv.agvNumber = _agvNumber;
                    agv.Run = (message[3] & 0x01) == 1;
                    agv.IsError = (message[3] & 0x02) >> 1 == 1;
                    agv.Selfmotion = (message[3] & 0x04) >> 2 == 1;
                    agv.Advance = (message[3] & 0x08) >> 3 == 1;
                    agv.TaskIn = (message[3] & 0x10) >> 4 == 1;
                    agv.location_X = message[5] + message[6];
                    agv.location_Y = message[7] + message[8];
                    agv.speed = message[9];
                    agv.voltage = message[10];
                    agv.OrderStatic = message[11];
                    agv.PBS = message[12];
                    ListagvInfo.Add(agv);
                }
            }
        }


    }
}
