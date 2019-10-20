using MercedesBenz.Infrastructure;
using MercedesBenz.SystemTask;
using Topshelf;

namespace MercedesBenz.Console
{
    public class TaskServer : ServiceControl
    {
        public bool Start(HostControl hostControl)
        {
            TaskDispose.Instance.TaskStart();
            return true;
        }
        public bool Stop(HostControl hostControl)
        {
            TaskDispose.Instance.TaskClose();
            return true;
        }
    }
}
