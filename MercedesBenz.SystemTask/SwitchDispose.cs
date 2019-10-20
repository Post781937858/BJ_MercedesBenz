using MercedesBenz.Infrastructure;
using MercedesBenz.Models;
using MercedesBenz.SystemTask.Client.Base;
using MercedesBenz.SystemTask.Server.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MercedesBenz.SystemTask
{
    /// <summary>
    /// 空开及看板推送服务
    /// </summary>
    public class SwitchDispose
    {
        /// <summary>
        /// TCP服务端列表
        /// </summary>
        public Dictionary<IPType, BaseTcpClientServer> _BackgroundTcpServer = new Dictionary<IPType, BaseTcpClientServer>();

        /// <summary>
        /// TCP客户端列表
        /// </summary>
        public Dictionary<IPType, BaseTcpClient> _BackgroundTcpClient = new Dictionary<IPType, BaseTcpClient>();

        //任务单例
        public static SwitchDispose Instance { get; } = new SwitchDispose();


        // <summary>
        /// 开启任务
        /// </summary>
        public void TaskStart()
        {
            //添加任务
            try
            {
                _BackgroundTcpClient.Add(IPType.Temperature1,new TemperatureManage(IPType.Temperature1));
                _BackgroundTcpServer.Add(IPType.SwitchServer, new SwitchManage(IPType.SwitchServer));
                _BackgroundTcpServer.Values.ToList().ForEach(p => p.Start());
                _BackgroundTcpClient.Values.ToList().ForEach(p => p.Start());
                Log4NetHelper.WriteDebugLog("服务启动");
                ConsoleLogHelper.WriteSucceedLog("The service start");
            }
            catch (System.Exception ex)
            {
                Log4NetHelper.WriteErrorLog(ex.Message, ex);
                ConsoleLogHelper.WriteErrorLog($"Service startup error！errormessage：{ ex.Message}\r\n堆栈:{ex.StackTrace}");
            }
        }

        /// <summary>
        /// 结束任务
        /// </summary>
        public void TaskClose()
        {
            _BackgroundTcpClient.Values.ToList().ForEach(p => p.Stop());
            _BackgroundTcpServer.Values.ToList().ForEach(p => p.Stop());
        }
    }
}
