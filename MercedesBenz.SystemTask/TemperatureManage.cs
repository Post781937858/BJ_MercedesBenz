using MercedesBenz.Models;
using MercedesBenz.SystemTask.Client.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using MercedesBenz.DataBase;

namespace MercedesBenz.SystemTask
{
    public class TemperatureManage : BaseTcpClient
    {
        public TemperatureManage(IPType type) : base(type)
        {

        }


        public override void MessageAnalysis(byte[] mes)
        {
            if (mes.Length < 8)
                return;

            double Humidity = (mes[3] << 8 | mes[4]) / 10.0;

            double Temperature = (mes[5] << 8 | mes[6]) / 10.0;

            SystemTaskDatabase.Instance.UpdateEnvironment(Temperature, Humidity);
        }

        public override void ClientTaskRun()
        {
            base.Send(GroupMessage.QueryTemperature());
            Thread.Sleep(1000);
        }
    }
}
