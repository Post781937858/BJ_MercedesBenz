using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Owin;

[assembly: OwinStartup(typeof(BJ_MercedesBenz_Spectaculars.Startup))]

namespace BJ_MercedesBenz_Spectaculars
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // 有关如何配置应用程序的详细信息，请访问 http://go.microsoft.com/fwlink/?LinkID=316888
            // 允许CORS跨域
            app.Map("/signalr", map =>
            {
                map.UseCors(CorsOptions.AllowAll);//跨域
                var hubConfiguration = new HubConfiguration
                {
                    EnableJSONP = true
                };
                map.RunSignalR(hubConfiguration);
            });
        }
    }
}
