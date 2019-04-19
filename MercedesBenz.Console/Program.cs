using MercedesBenz.Infrastructure;
using Topshelf;

namespace MercedesBenz.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            HostFactory.Run(x =>                                 
            {
                //x.Service<TaskServer>();
                //x.RunAsLocalSystem();                           
                //x.SetDescription("AGV通讯服务");        
                //x.SetDisplayName("Stuff");                       
                //x.SetServiceName("Stuff");                      

                x.Service<TaskServer>();
                x.SetDescription(SystemConfiguration.Description);
                x.SetDisplayName(SystemConfiguration.DisplayName);
                x.SetServiceName(SystemConfiguration.ServiceName);
                x.EnablePauseAndContinue();
                x.RunAsLocalSystem();
            });
        }
    }
}
