using System;
using MercedesBenz.Infrastructure;
using System.Net;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using MercedesBenz.Models;

namespace MercedesBenz.SystemTask
{

    /// <summary>
    /// 异步Tcp客户端任务处理
    /// </summary>
    public class AsyncTcpClientTask
    {
        public Dictionary<IPType, AsyncTcpClient > asyncTcpClient { get; private set; } //异步Tcp客户端
        public List<Task> connectTask { get; set; }  //断线重连线程
        private CancellationTokenSource CTSconnect { get; set; } //用于取消线程
        public Action<string> RequestWCS_Mes { get; set; } //处理WCS请求
        public Action<byte[]> RequestAGV_Mes { get; set; } //处理AGV请求
        public Action<byte[]> RequestNDC_Mes { get; set; } //处理NDC请求

        public AsyncTcpClientTask()
        {
            asyncTcpClient = new Dictionary<IPType, AsyncTcpClient>();
            connectTask = new List<Task>();
            CTSconnect = new CancellationTokenSource();
        }

        /// <summary>
        /// 连接远程服务器
        /// </summary>
        /// <param name="i"></param>
        /// <param name="Net"></param>
        /// <returns></returns>
        public void OpenServer()
        {
            try
            {
                CTSconnect.Cancel();
                Task.WaitAll(connectTask.ToArray());
                connectTask.Clear();
                asyncTcpClient.Clear();
                CTSconnect = new CancellationTokenSource();
                foreach (var item in SystemConfiguration.Distance_serve())
                {
                    if (item.ON)
                    {
                        asyncTcpClient.Add(item.type, _OpenServer(item)); ;
                        Thread.Sleep(1000);
                        Retryconnect(item);
                    }
                }
            }
            catch (Exception ex)
            {
                Log4NetHelper.WriteErrorLog(ex.Message, ex);
            }
        }


        private AsyncTcpClient _OpenServer(ServiceModel service)
        {
            AsyncTcpClient tcpClient = new AsyncTcpClient(IPAddress.Parse(service.IP), service.Port);
            tcpClient.ServerConnected += TcpClient_ServerConnected;
            tcpClient.ServerDisconnected += TcpClient_ServerDisconnected;
            tcpClient.ServerExceptionOccurred += TcpClient_ServerExceptionOccurred;
            switch (service.type)
            {
                case IPType.agv:
                    tcpClient.DatagramReceived += TcpClientAGV_DatagramReceived;
                    break;
                case IPType.ndc:
                    tcpClient.DatagramReceived += TcpClientNDC_DatagramReceived;
                    break;
                case IPType.wcs:
                    tcpClient.DatagramReceived += TcpClientWCS_DatagramReceived;
                    break;
            }
            tcpClient.Connect();
            return tcpClient;
        }

        public void Retryconnect(ServiceModel service)
        {
            int i = 1;
            connectTask.Add(Task.Run(() =>
            {
                while (!CTSconnect.IsCancellationRequested)
                {
                    try
                    {
                        if (!asyncTcpClient[service.type].Connected)
                        {
                            Console.WriteLine("********************************************************************");
                            Console.WriteLine($"IP:{service.IP},端口:{service.Port}, 类型:{service.type}  第{i}次尝试重新连接 线程ID:{Thread.CurrentThread.ManagedThreadId}");
                            CloseServer(service);
                            Thread.Sleep(1000);
                            asyncTcpClient[service.type] = _OpenServer(service);
                            i++;
                        }
                        else
                        {
                            i = 1;
                        }
                    }
                    catch (Exception ex)
                    {
                        Log4NetHelper.WriteErrorLog(ex.Message, ex);
                    }
                    Thread.Sleep(3000);
                }
            }, CTSconnect.Token));
        }



        /// <summary>
        /// 断开服务器连接
        /// </summary>
        public void CloseServer(ServiceModel service)
        {
            try
            {
                asyncTcpClient[service.type].Close();
                //asyncTcpClient.Dispose();
            }
            catch (Exception ex)
            {
                Log4NetHelper.WriteErrorLog(ex.Message, ex);
            }
        }

        /// <summary>
        /// 与服务器连接发送异常事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TcpClient_ServerExceptionOccurred(object sender, TcpServerExceptionOccurredEventArgs e)
        {
            //(sender as AsyncTcpClient).Close();
            // TxtLogShowInfo(e.Exception.Message, true);
            //Console.WriteLine(e.Exception.Message);
        }

        /// <summary>
        /// 与服务器端口断开事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TcpClient_ServerDisconnected(object sender, TcpServerDisconnectedEventArgs e)
        {
            Console.WriteLine($"IP：{e.Addresses[0].ToString()}，端口：{e.Port}，连接断开", true);
            // Console.WriteLine("断开连接");
        }

        /// <summary>
        /// 与服务器建立连接事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TcpClient_ServerConnected(object sender, TcpServerConnectedEventArgs e)
        {
            Console.WriteLine($"IP：{e.Addresses[0].ToString()}，端口：{e.Port}，连接成功", true);

            // Console.WriteLine("连接成功");
        }

        /// <summary>
        /// 接收到数据报文事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TcpClientWCS_DatagramReceived(object sender, TcpDatagramReceivedEventArgs<byte[]> e)
        {
            var byteStr = e.Datagram;
            string str_mes = System.Text.Encoding.GetEncoding("GB2312").GetString(byteStr);
            RequestWCS_Mes.Invoke(str_mes);
        }

        /// <summary>
        /// 接收到数据报文事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TcpClientAGV_DatagramReceived(object sender, TcpDatagramReceivedEventArgs<byte[]> e)
        {
            RequestAGV_Mes.Invoke(e.Datagram);
        }


        /// <summary>
        /// 接收到数据报文事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TcpClientNDC_DatagramReceived(object sender, TcpDatagramReceivedEventArgs<byte[]> e)
        {
            RequestNDC_Mes.Invoke(e.Datagram);
        }
    }
}
