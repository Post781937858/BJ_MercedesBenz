using MercedesBenz.DataBase;
using MercedesBenz.Infrastructure;
using MercedesBenz.Models;
using MercedesBenz.SystemTask.Client.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace MercedesBenz.SystemTask
{
    /// <summary>
    /// 重载入库缓存区状态及门控制
    /// </summary>
    public class InConnectionManage : BaseTcpClient
    {
        public override int Timelapse { get { return 500; } }

        public InConnectionManage(IPType type) : base(type)
        { }

        public override void MessageAnalysis(byte[] mes)
        {
            List<byte[]> byteList = BytePackagedis.AnalysisByte(mes);
            foreach (var messageitem in byteList)
            {
                if (messageitem[7] == 0x03)
                {
                    if (messageitem.Length < 17)
                        break;
                    int DoorOutStatus = messageitem[10];
                    int Buffer1 = messageitem[12];
                    int Buffer2 = messageitem[14];
                    int Buffer3 = messageitem[16];
                    if (DoorOutStatus == 1)
                    {
                        TaskDispose.Instance.DoorInfoArray[DoorType.In].DoorStatus = DoorStatus.Open;
                    }
                    else
                    {
                        TaskDispose.Instance.DoorInfoArray[DoorType.In].DoorStatus = DoorStatus.Close;
                    }
                    TaskDispose.Instance.DoorInfoArray[DoorType.In].UpdateDateTime = UTC.ConvertDateTimeLong(DateTime.Now);
                    SystemTaskDatabase.Instance.UpdateBufferStatus(1, Buffer1 == 1 ? 1 : 0);
                    SystemTaskDatabase.Instance.UpdateBufferStatus(2, Buffer2 == 1 ? 1 : 0);
                    SystemTaskDatabase.Instance.UpdateBufferStatus(3, Buffer3 == 1 ? 1 : 0);
                    var Log = string.Join(" ", messageitem.Select(s => s.ToString("X2")));
                    Log4NetHelper.WritedoorLog(Log);
                }
            }
        }

        public override void ClientTaskRun()
        {
            base.Send(GroupMessage.QueryInSite());

            if (SystemConfiguration.DoorON == 1)
            {
                base.Send(GroupMessage.writeSiteInPLC(1));
            }
            else if (SystemConfiguration.DoorON == 2)
            {
                base.Send(GroupMessage.writeSiteInPLC(2));
            }
            if (SystemConfiguration.IsRunDoor)
            {   
                //状态1分钟未更新默认门为关闭状态
                if (UTC.ConvertDateTimeLong(DateTime.Now) - TaskDispose.Instance.DoorInfoArray[DoorType.In].UpdateDateTime > 60)
                {
                    TaskDispose.Instance.DoorInfoArray[DoorType.In].DoorStatus = DoorStatus.Close;
                }
                DoorTask();
            }
        }

        private void DoorTask()
        {
            foreach (var agvNumber in TaskDispose.Instance.agvInfoList.Keys)
            {
                var info = TaskDispose.Instance.agvInfoList[agvNumber];

                if (info.ThisStation == 44 || info.ThisStation == 192 ||  info.ThisStation == 193)
                {
                    base.Send(GroupMessage.writeSiteInPLC(1));
                    Log4NetHelper.WriteTaskLog($"入库请求开门,当前站点：{info.ThisStation}");
                }
                else if (info.ThisStation == 46)
                {
                    base.Send(GroupMessage.writeSiteInPLC(2));
                    Log4NetHelper.WriteTaskLog($"入库请求关门,当前站点：{info.ThisStation}");
                }
                else if (info.ThisStation == 184 || info.ThisStation == 13|| info.ThisStation == 191 || info.ThisStation == 190)
                {
                    base.Send(GroupMessage.writeSiteInPLC(1));
                    Log4NetHelper.WriteTaskLog($"入库请求开门,当前站点：{info.ThisStation}");
                }
                else if (info.ThisStation == 18)
                {
                    base.Send(GroupMessage.writeSiteInPLC(2));
                    Log4NetHelper.WriteTaskLog($"入库请求关门,当前站点：{info.ThisStation}");
                }
            }
        }
    }
}
