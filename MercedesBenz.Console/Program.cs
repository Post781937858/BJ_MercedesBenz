using MercedesBenz.Infrastructure;
using MercedesBenz.SystemTask;
using Topshelf;
using System;
using System.Linq;

namespace MercedesBenz.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            HostFactory.Run(x =>
            {
                x.Service<TaskServer>();
                x.SetDescription(SystemConfiguration.Description);
                x.SetDisplayName(SystemConfiguration.DisplayName);
                x.SetServiceName(SystemConfiguration.ServiceName);
                x.EnablePauseAndContinue();
                x.RunAsLocalSystem();
            });
            System.Console.ReadKey();
        }
    }
}
