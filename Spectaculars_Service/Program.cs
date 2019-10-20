using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.Owin.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Configuration;
using Topshelf;

namespace BJ_MercedesBenz_Spectaculars
{
    class Program
    {
        static void Main(string[] args)
        {
            HostFactory.Run(x =>
            {
                x.Service<TaskServer>();
                x.SetDescription("GALAXIS_AGV_Server");
                x.SetDisplayName("GALAXIS_AGV_Server");
                x.SetServiceName("GALAXIS_AGV_Server");
                x.EnablePauseAndContinue();
                x.RunAsLocalSystem();
            });
            System.Console.ReadKey();
        }
    }
}
