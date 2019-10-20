using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Topshelf;

namespace BJ_MercedesBenz_Spectaculars
{
    class TaskServer : ServiceControl
    {
        public bool Start(HostControl hostControl)
        {
            Background_Service._service.ServiceStart();
            return true;
        }
        public bool Stop(HostControl hostControl)
        {
            return true;
        }
    }
}
