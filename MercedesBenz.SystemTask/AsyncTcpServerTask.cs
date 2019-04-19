using MercedesBenz.Infrastructure;
using MercedesBenz.Models;
using System;
using System.Net;

namespace MercedesBenz.SystemTask
{
    public class AsyncTcpServerTask
    {

        public AsyncTCPServer asyncTCPServer { get; private set; } //异步Tcp服务端


        /// <summary>
        ///创建 服务器
        /// </summary>
        /// <param name="i"></param>
        /// <param name="Net"></param>
        /// <returns></returns>
        public void SetUpServer()
        {
            try
            {
                ServiceModel service = SystemConfiguration.This_serve();
                if (service != null)
                {
                    AsyncTCPServer tcpServer = new AsyncTCPServer(IPAddress.Parse(service.IP), service.Port, service.ServerMax);
                    asyncTCPServer = tcpServer;
                    tcpServer.ClientConnected += TcpServer_ClientConnected; ;
                    tcpServer.ClientDisconnected += TcpServer_ClientDisconnected; ;
                    tcpServer.DataReceived += TcpServer_DataReceived; ;
                    tcpServer.PrepareSend += TcpServer_PrepareSend; ;
                    tcpServer.CompletedSend += TcpServer_CompletedSend;
                    tcpServer.NetError += TcpServer_NetError;
                    tcpServer.OtherException += TcpServer_OtherException;
                    tcpServer.Start();
                }
            }
            catch (Exception ex)
            {
                Log4NetHelper.WriteErrorLog(ex.Message, ex);
            }
        }

        /// <summary>
        /// 关闭服务器
        /// </summary>
        public void CloseServer()
        {
            asyncTCPServer.CloseAllClient();
            asyncTCPServer.Dispose();
        }



        /// <summary>
        /// 异常事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TcpServer_OtherException(object sender, AsyncEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// 网络错误事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TcpServer_NetError(object sender, AsyncEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// 数据发送完毕事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TcpServer_CompletedSend(object sender, AsyncEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// 发送数据前的事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TcpServer_PrepareSend(object sender, AsyncEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// 接收到数据事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TcpServer_DataReceived(object sender, AsyncEventArgs e)
        {
            var byteStr = e._state.RecvDataBuffer;
        }

        /// <summary>
        /// 与客户端的连接已断开事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TcpServer_ClientDisconnected(object sender, AsyncEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// 与客户端的连接已建立事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TcpServer_ClientConnected(object sender, AsyncEventArgs e)
        {
            throw new System.NotImplementedException();
        }
    }
}
