using MercedesBenz.Infrastructure;
using MercedesBenz.SystemTask;
using Topshelf;

namespace MercedesBenz.Console
{
    public class TaskServer : ServiceControl
    {
        private TaskDispose Task = new TaskDispose();
        public bool Start(HostControl hostControl)
        {
            Task.OpenSystemTask();
            Log4NetHelper.WriteErrorLog("启动成功");
            return true;
        }
        public bool Stop(HostControl hostControl)
        {
            Log4NetHelper.WriteErrorLog("停止成功");
            return true;
        }
    }
}
