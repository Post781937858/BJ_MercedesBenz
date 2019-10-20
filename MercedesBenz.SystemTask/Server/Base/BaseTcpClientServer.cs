using MercedesBenz.Infrastructure;
using MercedesBenz.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TaskStatus = System.Threading.Tasks.TaskStatus;

namespace MercedesBenz.SystemTask.Server.Base
{
    public class BaseTcpClientServer
    {
        //延时
        public virtual int Timelapse { get { return 1000; } }

        //异步TCP服务端
        private AsyncTCPServer asyncTcpServer;

        //服务端类型
        private IPType _GetType;

        //是否开启后台任务
        private bool IsCancellationTask = false;

        //用于取消后台任务
        private CancellationTokenSource ClientCancel;

        //任务集合
        private Task SystemTask;

        //状态锁
        private object _lock = new object();

        //是否开启
        public bool IsStart { get; set; }

        public BaseTcpClientServer(IPType type)
        {
            _GetType = type;
            asyncTcpServer = TcpServer(type);
        }


        /// <summary>
        /// 创建TCP服务端
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private AsyncTCPServer TcpServer(IPType type)
        {
            ServiceModel _serviceModel = SystemConfiguration.Distance_serve(type);
            var _asyncTcpServer = new AsyncTCPServer(IPAddress.Parse(_serviceModel.IP),_serviceModel.Port, _serviceModel.ServerMax);
            _asyncTcpServer.DataReceived += _asyncTcpServer_DataReceived;
            _asyncTcpServer.ClientDisconnected += _asyncTcpServer_ClientDisconnected;
            _asyncTcpServer.NetError += _asyncTcpServer_NetError; 
            _asyncTcpServer.OtherException += _asyncTcpServer_OtherException; 
            _asyncTcpServer.ClientConnected += _asyncTcpServer_ClientConnected;
            _asyncTcpServer.ClientConnected += (object sender, AsyncEventArgs e) =>
            {
                lock (_lock)
                {
                    IsCancellationTask = true;
                }
            };
            _asyncTcpServer.ClientDisconnected += (object sender, AsyncEventArgs e) =>
            {
                lock (_lock)
                {
                    IsCancellationTask = false;
                }
            };
            IsStart = _serviceModel.ON;
            return _asyncTcpServer;
        }

        private void Base_backgroundTask()
        {
            ClientCancel = new CancellationTokenSource();
            //后台任务
            SystemTask = Task.Run(() =>
             {
                 while (!ClientCancel.IsCancellationRequested)
                 {
                     try
                     {
                         TcpClient__BackgroundTask();
                     }
                     catch (Exception ex) { Log4NetHelper.WriteErrorLog(ex.Message, ex); }

                     if (!ClientCancel.IsCancellationRequested)
                         Thread.Sleep(Timelapse);
                 }
             }, ClientCancel.Token);
        }

        /// <summary>
        /// 后台任务处理
        /// </summary>
        public virtual void TcpClient__BackgroundTask()
        {
            try
            {
               ServerTaskRun();
            }
            catch (Exception ex) { Log4NetHelper.WriteErrorLog(ex.Message, ex); }
        }

        public virtual void ServerTaskRun()
        {

        }

        /// <summary>
        /// 接收到数据事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _asyncTcpServer_DataReceived(object sender, AsyncEventArgs e)
        {
            try
            {
                MessageAnalysis(e._state.RecvDataBuffer);
                if (e._state.ClientSocket != null)
                {
                    var _IPEndPoint = (System.Net.IPEndPoint)e._state.ClientSocket.RemoteEndPoint;
                    var IpConfig = _IPEndPoint.Address.ToString();
                    ServiceModel _serviceModel = SystemConfiguration.Servicecfig().FirstOrDefault(p => p.IP == IpConfig);
                    if (_serviceModel != null)
                    {
                        MessageAnalysis(e._state.RecvDataBuffer, _serviceModel.type);
                    }
                }
            }
            catch (Exception ex) { Log4NetHelper.WriteErrorLog(ex.Message, ex); }
        }

        public virtual void MessageAnalysis(byte[] message)
        {

        }

        public virtual void MessageAnalysis(byte[] message, IPType type)
        {

        }

        /// <summary>
        /// 与客户端连接建立事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _asyncTcpServer_ClientConnected(object sender, AsyncEventArgs e)
        {
            if (e._state != null && e._state.ClientSocket != null)
            {
                var _IPEndPoint = (System.Net.IPEndPoint)e._state.ClientSocket.RemoteEndPoint;
                if (_IPEndPoint != null)
                {
                    ConsoleLogHelper.WriteSucceedLog($"client:{_IPEndPoint.Address}:{_IPEndPoint.Port} 已连接");
                }
            }
        }

        /// <summary>
        /// 异常事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _asyncTcpServer_OtherException(object sender, AsyncEventArgs e)
        {
            Log4NetHelper.WriteErrorLog(e._msg);
        }

        /// <summary>
        /// 网格错误事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _asyncTcpServer_NetError(object sender, AsyncEventArgs e)
        {
            Log4NetHelper.WriteErrorLog(e._msg);
        }

        /// <summary>
        /// 与服务器连接断开事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _asyncTcpServer_ClientDisconnected(object sender, AsyncEventArgs e)
        {
            if (e._state != null && e._state.ClientSocket != null)
            {
                var _IPEndPoint = (System.Net.IPEndPoint)e._state.ClientSocket.RemoteEndPoint;
                ConsoleLogHelper.WriteErrorLog($"client:{_IPEndPoint.Address}:{_IPEndPoint.Port} 连接断开");
            }
        }

        /// <summary>
        /// 向指定客户端广播报文
        /// </summary>
        /// <param name="datagram"></param>
        public virtual void Send(IPType SendType, byte[] datagram)
        {
            try
            {
                if (asyncTcpServer != null)
                {
                    if (asyncTcpServer.IsRunning)
                    {
                        ServiceModel _serviceModel = SystemConfiguration.Distance_serve(SendType);
                        if (_serviceModel != null)
                        {
                            for (int i = 0; i < asyncTcpServer._clients.Count(); i++)
                            {
                                if (asyncTcpServer._clients[i].ClientSocket != null)
                                {
                                    var _IPEndPoint = (System.Net.IPEndPoint)asyncTcpServer._clients[i].ClientSocket.RemoteEndPoint;
                                    var IpConfig = _IPEndPoint.Address.ToString();
                                    if (IpConfig == _serviceModel.IP)
                                    {
                                        asyncTcpServer.Send(asyncTcpServer._clients[i], datagram);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { Log4NetHelper.WriteErrorLog(ex.Message, ex); }
        }

        /// <summary>
        /// 启动
        /// </summary>
        public virtual void Start()
        {
            if(IsStart)
            {
                asyncTcpServer.Start();
                Thread.Sleep(200);
                Base_backgroundTask();
            }
        }

        /// <summary>
        /// 停止
        /// </summary>
        public virtual void Stop()
        {
            if (asyncTcpServer != null)
            {
                asyncTcpServer.Stop();
                asyncTcpServer.Dispose();
            }
            if (ClientCancel != null)
            {
                ClientCancel.Cancel();
                //Task.WaitAll(new Task[] { SystemTask });
            }
        }
    }
}
