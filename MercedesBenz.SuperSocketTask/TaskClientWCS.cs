using MercedesBenz.DataBase;
using MercedesBenz.Infrastructure;
using MercedesBenz.Models;
using MercedesBenz.SuperSocketTask.ClientCode;
using System.Linq;
using System.Text;

namespace MercedesBenz.SuperSocketTask
{
    public class TaskClientWCS : BaseClientTask
    {

        public override void MessageAnalysis(byte[] messageData)
        {
            string _str_message = Encoding.Default.GetString(messageData).Trim("\0".ToArray());
            string[] str_messageList = _str_message.Split('\n').Where(p => p.Contains("messageName")).ToArray();
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
