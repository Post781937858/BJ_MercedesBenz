using MercedesBenz.Infrastructure;
using MercedesBenz.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace MercedesBenz.SystemTask.Client.Base
{
    /// <summary>
    /// 基础客户端
    /// </summary>
    public class BaseTcpClient
    {
        //延时
        public virtual int Timelapse { get { return 1000; } }

        //用于取消后台任务
        private CancellationTokenSource ClientCancel;

        //异步TCP客户端
        private AsyncTcpClient asyncTcpClient;

        //客户端类型
        private IPType _GetType;

        //是否开启后台任务
        private bool IsCancellationTask = false;

        //状态锁
        private object _lock = new object();

        //锁
        private object _lockByte = new object();

        //任务集合
        private List<Task> TaskList = new List<Task>();

        //是否开启
        public bool IsStart { get; set; }

        public BaseTcpClient(IPType type)
        {
            _GetType = type;
            asyncTcpClient = TcpClient(type);
        }

        private void Base_backgroundTask()
        {
            ClientCancel = new CancellationTokenSource();
            //后台任务
            TaskList.Add(Task.Run(() =>
            {
                while (!ClientCancel.IsCancellationRequested)
                {
                    if (IsCancellationTask)
                    {
                        try
                        {
                            TcpClient__BackgroundTask();
                        }
                        catch (Exception ex) { Log4NetHelper.WriteErrorLog(ex.Message, ex); }
                    }
                    if (!ClientCancel.IsCancellationRequested)
                        Thread.Sleep(Timelapse);
                }
            }, ClientCancel.Token));

            //断线重连
            TaskList.Add(Task.Run(() =>
            {
                while (!ClientCancel.IsCancellationRequested)
                {
                    try
                    {
                        if (!asyncTcpClient.Connected && IsStart)
                        {
                            lock (_lock)
                            {
                                IsCancellationTask = false;
                            }
                            asyncTcpClient.Close();
                            if (!ClientCancel.IsCancellationRequested)
                                Thread.Sleep(1000);
                            asyncTcpClient.Dispose();
                            if (!ClientCancel.IsCancellationRequested)
                                Thread.Sleep(3000);
                            asyncTcpClient = TcpClient(_GetType);
                            asyncTcpClient.Connect();
                            if (!ClientCancel.IsCancellationRequested)
                                Thread.Sleep(3000);
                        }
                        if(!ClientCancel.IsCancellationRequested)
                        Thread.Sleep(2000);
                    }
                    catch (Exception ex) { Log4NetHelper.WriteErrorLog(ex.Message, ex); }
                }
            }, ClientCancel.Token));
        }

        /// <summary>
        /// 创建TCP客户端
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private AsyncTcpClient TcpClient(IPType type)
        {
            ServiceModel _serviceModel = SystemConfiguration.Distance_serve(type);
            var _asyncTcpClient = new AsyncTcpClient(IPAddress.Parse(_serviceModel.IP), _serviceModel.Port);
            _asyncTcpClient.DatagramReceived += TcpClient_DatagramReceived;
            _asyncTcpClient.ServerExceptionOccurred += TcpClient_ServerExceptionOccurred;
            _asyncTcpClient.ServerDisconnected += TcpClient_ServerDisconnected;
            _asyncTcpClient.ServerConnected += TcpClient_ServerConnected;
            _asyncTcpClient.ServerConnected += (object sender, TcpServerConnectedEventArgs e) =>
             {
                 lock (_lock)
                 {
                     IsCancellationTask = true;
                 }
             };
            _asyncTcpClient.ServerDisconnected += (object sender, TcpServerDisconnectedEventArgs e) =>
            {
                lock (_lock)
                {
                    IsCancellationTask = false;
                }
            };
            IsStart = _serviceModel.ON;
            return _asyncTcpClient;
        }

        /// <summary>
        /// 接受到报文事件处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public virtual void TcpClient_DatagramReceived(object sender, TcpDatagramReceivedEventArgs<byte[]> e)
        {
            try
            {
                lock (_lockByte)
                {
                    MessageAnalysis(e.Datagram);
                }
            }
            catch (Exception ex) { Log4NetHelper.WriteErrorLog(ex.Message, ex); }

        }

        public virtual void MessageAnalysis(byte[] message)
        {

        }


        /// <summary>
        /// 与服务器连接发送异常事件处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public virtual void TcpClient_ServerExceptionOccurred(object sender, TcpServerExceptionOccurredEventArgs e)
        {
            Log4NetHelper.WriteErrorLog(e.Exception.Message, e.Exception);
        }


        /// <summary>
        /// 与服务器端口断开事件处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public virtual void TcpClient_ServerDisconnected(object sender, TcpServerDisconnectedEventArgs e)
        {
            ConsoleLogHelper.WriteErrorLog($"{e.Addresses[0]}:{e.Port} {_GetType.ToString()} 连接断开");
        }


        /// <summary>
        /// 与服务器建立连接事件处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public virtual void TcpClient_ServerConnected(object sender, TcpServerConnectedEventArgs e)
        {
            ConsoleLogHelper.WriteSucceedLog($"{e.Addresses[0]}:{e.Port} {_GetType.ToString()} 已连接");
        }


        /// <summary>
        /// 后台任务处理
        /// </summary>
        public virtual void TcpClient__BackgroundTask()
        {
            try
            {
                ClientTaskRun();
            }
            catch (Exception ex) { Log4NetHelper.WriteErrorLog(ex.Message, ex); }
        }


        public virtual  void ClientTaskRun()
        {

        }


        /// <summary>
        /// 发送报文
        /// </summary>
        /// <param name="datagram"></param>
        public virtual void Send(byte[] datagram)
        {
            try
            {
                if (asyncTcpClient.Connected)
                    asyncTcpClient.Send(datagram);
            }
            catch (Exception ex) { Log4NetHelper.WriteErrorLog(ex.Message, ex); }
        }

        /// <summary>
        /// 启动
        /// </summary>
        public virtual void Start()
        {
            if (IsStart)
            {
                asyncTcpClient.Connect();
                Thread.Sleep(200);
                Base_backgroundTask();
            }
        }

        /// <summary>
        /// 停止
        /// </summary>
        public virtual void Stop()
        {
            if (asyncTcpClient != null)
            {
                asyncTcpClient.Close();
                asyncTcpClient.Dispose();
            }
            if (ClientCancel != null)
            {
                ClientCancel.Cancel();
                //Task.WaitAll(TaskList.ToArray());
            }
        }
    }
}
