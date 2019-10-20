using MercedesBenz.Infrastructure;
using SuperSocket.ClientEngine;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace MercedesBenz.SuperSocketTask.ClientCode
{
    public class BaseEasyClient
    {
        private Task requestTimer = null;
        private BaseClientTask _baseClientTask;
        public AsyncTcpSession GetEasyClient;
        private IPEndPoint ClientiPEndPoint;
        private CancellationTokenSource ClientCancel;
        private int OffLine = 0; //离线次数

        public BaseEasyClient(BaseClientTask baseClient, Models.IPType iPType)
        {
            ClientCancel = new CancellationTokenSource();
            GetEasyClient = new AsyncTcpSession();
            GetEasyClient.Closed += BaseAsyncTcpSession_Closed;
            GetEasyClient.Error += GetEasyClient_Error;
            GetEasyClient.Connected += BaseAsyncTcpSession_Connected;
            GetEasyClient.DataReceived += GetEasyClient_DataReceived;
            requestTimer = new Task(RequestTimer_ElapsedAsync, ClientCancel.Token);
            _baseClientTask = baseClient;
            Models.ServiceModel serviceModel = SystemConfiguration.Distance_serve(iPType);
            if (serviceModel != null)
            {
                ClientiPEndPoint = new IPEndPoint(IPAddress.Parse(serviceModel.IP), serviceModel.Port);
                _baseClientTask.iPEndPoint = ClientiPEndPoint;
            }
            else
            {
                throw new Exception($"未查询到对应IP类型配置参数，请检查配置文件。IPType:{iPType.ToString()}");
            }
        }

        private void GetEasyClient_DataReceived(object sender, DataEventArgs e)
        {
            try
            {
                byte[] message = new byte[e.Length];
                Array.Copy(e.Data, message, e.Length);
                _baseClientTask.MessageAnalysis(message);
            }
            catch (Exception ex) { Log4NetHelper.WriteErrorLog(ex.Message, ex); }
        }

        //异常事件
        private void GetEasyClient_Error(object sender, ErrorEventArgs e)
        {
            SystemTaskDispose.SystemLog($"server【{ClientiPEndPoint.Address}:{ClientiPEndPoint.Port}】 异常信息： {e.Exception.Message} ", true);
        }

        //连接服务器事件
        private void BaseAsyncTcpSession_Connected(object sender, EventArgs e)
        {
            SystemTaskDispose.SystemLog($"server【{ClientiPEndPoint.Address}:{ClientiPEndPoint.Port}】 已连接", true);
        }

        //连接断开事件
        private void BaseAsyncTcpSession_Closed(object sender, EventArgs e)
        {
            SystemTaskDispose.SystemLog($"server【{ClientiPEndPoint.Address}:{ClientiPEndPoint.Port}】 已断开", true);
        }

        //定时任务
        private void RequestTimer_ElapsedAsync()
        {
            while (!ClientCancel.IsCancellationRequested)
            {
                try
                {
                    if (GetEasyClient.IsConnected)
                    {
                        _baseClientTask.ClientTaskRun();
                    }
                    else
                    {
                        try
                        {
                            if (OffLine >= 50)
                            {
                                OffLine = 0;
                                SystemTaskDispose.SystemLog($"server【{ClientiPEndPoint.Address}:{ClientiPEndPoint.Port}】正在尝试重新连接", true);
                                GetEasyClient.Connect(ClientiPEndPoint);
                            }
                            else
                            {
                                OffLine++;
                            }
                        }
                        catch (Exception ex)
                        {
                            SystemTaskDispose.SystemLog($"server【{ClientiPEndPoint.Address}:{ClientiPEndPoint.Port}】 异常信息：{ex.Message}", true);
                        }
                    }
                    if (!ClientCancel.IsCancellationRequested)
                        Thread.Sleep(500);
                }
                catch (Exception ex) { Log4NetHelper.WriteErrorLog(ex.Message, ex); }
            }
        }

        /// <summary>
        /// 启动
        /// </summary>
        /// <returns></returns>
        public void StartAsync()
        {
            try
            {
                requestTimer.Start();
                GetEasyClient.Connect(ClientiPEndPoint);
            }
            catch (Exception ex) { Log4NetHelper.WriteErrorLog(ex.Message, ex); }
        }

        /// <summary>
        /// 停止
        /// </summary>
        public void StopAsync()
        {
            try
            {
                if (requestTimer.Status != TaskStatus.Canceled)
                {
                    ClientCancel.Cancel();
                    Task.WaitAll(new Task[] { requestTimer });
                }
                GetEasyClient.Close();
            }
            catch (Exception ex) { Log4NetHelper.WriteErrorLog(ex.Message, ex); }
        }
    }
}
