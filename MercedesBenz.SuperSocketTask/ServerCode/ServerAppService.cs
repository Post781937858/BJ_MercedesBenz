using MercedesBenz.Infrastructure;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Protocol;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MercedesBenz.SuperSocketTask.ServerCode
{
    public class BaseAppService : AppServer<ServerSession, ServerRequestInfo>
    {
        private Task requestTimer = null;
        private CancellationTokenSource ClientCancel;
        private SuperSocketBaseTask GetBaseTask;

        public BaseAppService(SuperSocketBaseTask baseTask) : base(new DefaultReceiveFilterFactory<ServerFilter, ServerRequestInfo>())
        {
            GetBaseTask = baseTask;
            ClientCancel = new CancellationTokenSource();
            requestTimer = new Task(RequestTimer_Elapsed, ClientCancel.Token);
            base.NewRequestReceived += appServer_NewRecivede; //接收事件
        }

        private void RequestTimer_Elapsed()
        {
            try
            {
                while (!ClientCancel.IsCancellationRequested)
                {
                    GetBaseTask.ServerTaskRun();
                    if (!ClientCancel.IsCancellationRequested)
                        Thread.Sleep(200);
                }
            }
            catch (Exception ex) { Log4NetHelper.WriteErrorLog(ex.Message, ex); }
        }

        /// <summary>
        /// 数据接收事件
        /// </summary>
        /// <param name="session"></param>
        /// <param name="requestInfo"></param>
        private void appServer_NewRecivede(ServerSession session, ServerRequestInfo requestInfo)
        {
            try
            {
                GetBaseTask.MessageDispose(session, requestInfo);
            }
            catch (Exception ex) { Log4NetHelper.WriteErrorLog(ex.Message, ex); }
        }


        /// <summary>
        /// 新的连接
        /// </summary>
        /// <param name="session"></param>
        protected override void OnNewSessionConnected(ServerSession session)
        {
            SystemTaskDispose.SystemLog($"Client【{session.RemoteEndPoint.Address.ToString()}:{session.RemoteEndPoint.Port}】 已连接", true);
            base.OnNewSessionConnected(session);
        }

        /// <summary>
        /// 连接断开
        /// </summary>
        /// <param name="session"></param>
        /// <param name="reason"></param>
        protected override void OnSessionClosed(ServerSession session, CloseReason reason)
        {
            SystemTaskDispose.SystemLog($"Client【{session.RemoteEndPoint.Address.ToString()}:{session.RemoteEndPoint.Port}】 已断开", true);
            base.OnSessionClosed(session, reason);
        }

        /// <summary>
        /// 命令接收
        /// </summary>
        /// <param name="session"></param>
        /// <param name="requestInfo"></param>
        protected override void ExecuteCommand(ServerSession session, ServerRequestInfo requestInfo)
        {
            base.ExecuteCommand(session, requestInfo);
        }

        /// <summary>
        /// 服务启动
        /// </summary>
        /// <returns></returns>
        public override bool Start()
        {
            try
            {
                requestTimer.Start();
                return base.Start();
            }
            catch (Exception ex) { Log4NetHelper.WriteErrorLog(ex.Message, ex); return false; }
        }

        /// <summary>
        /// 服务停止
        /// </summary>
        public override void Stop()
        {
            try
            {
                if (requestTimer.Status != TaskStatus.Canceled)
                {
                    ClientCancel.Cancel();
                    Task.WaitAll(new Task[] { requestTimer });
                }
                base.Stop();
            }
            catch (Exception ex) { Log4NetHelper.WriteErrorLog(ex.Message, ex); }
        }
    }
}
