using MercedesBenz.DataBase;
using MercedesBenz.Infrastructure;
using MercedesBenz.Models;
using MercedesBenz.SystemTask.Client.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MercedesBenz.SystemTask
{
    public class OrderTaskClientWCS : BaseTcpClient
    {
        public OrderTaskClientWCS(IPType type) : base(type)
        { }

        public override void MessageAnalysis(byte[] mes)
        {
            string _str_message = Encoding.GetEncoding("GB2312").GetString(mes).Trim("\0".ToArray());
            string[] str_messageList = _str_message.Replace('\n', '$').Split('$');
            foreach (var str_message in str_messageList)
            {
                if (str_message.Contains("messageName"))
                {
                    BaseMessage _baseMessage = str_message.ToObject<BaseMessage>();
                    switch (_baseMessage.messageName)
                    {
                        case "updateMovement": //任务状态更新回应
                            {
                                ResponsemesUpdate responsemes = str_message.ToObject<ResponsemesUpdate>();
                                SystemTaskDatabase.Instance.UpdateMessage(responsemes);
                            }
                            break;
                        case "alarmException": //报警回应
                            {

                            }
                            break;
                    }
                    ConsoleLogHelper.WriteInfoLog(str_message);
                }
            }
            Log4NetHelper.WriteDebugLog(_str_message);
        }

        public override void ClientTaskRun()
        {

        }
    }
}
