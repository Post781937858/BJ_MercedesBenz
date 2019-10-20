using MercedesBenz.DataBase;
using MercedesBenz.Infrastructure;
using MercedesBenz.Models;
using MercedesBenz.SuperSocketTask.ClientCode;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MercedesBenz.SuperSocketTask
{
    public class TaskClientInDoor : BaseClientTask
    {
        public override void MessageAnalysis(byte[] messageData)
        {
            List<byte[]> byteList = BytePackagedis.AnalysisByte(messageData);
            foreach (var messageitem in byteList)
            {
                if (messageitem.Length < 17)
                    break;
                if (messageitem[7] == 0x03)
                {
                    int DoorOutStatus = messageitem[10];
                    int Buffer1 = messageitem[12];
                    int Buffer2 = messageitem[14];
                    int Buffer3 = messageitem[16];
                    if (DoorOutStatus == 1)
                    {
                        SystemTaskDispose.DoorInfoArray[DoorType.In].DoorStatus = DoorStatus.Open;
                    }
                    else
                    {
                        SystemTaskDispose.DoorInfoArray[DoorType.In].DoorStatus = DoorStatus.Close;
                    }
                    SystemTaskDispose.DoorInfoArray[DoorType.In].UpdateDateTime = UTC.ConvertDateTimeLong(DateTime.Now);
                    SystemTaskDispose.DoorInfoArray[DoorType.In].updateDateInfoTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    SystemTaskDatabase.Instance.UpdateBufferStatus(1, Buffer1 == 1 ? 1 : 0);
                    SystemTaskDatabase.Instance.UpdateBufferStatus(2, Buffer2 == 1 ? 1 : 0);
                    SystemTaskDatabase.Instance.UpdateBufferStatus(3, Buffer3 == 1 ? 1 : 0);
                    var Log = string.Join(" ", messageitem.Select(s => s.ToString("X2")));
                    Log4NetHelper.WritedoorLog($"Form {base.iPEndPoint.Address}:{iPEndPoint.Port}：{Log}");
                }
            }
        }

        public override void ClientTaskRun()
        {
            SystemTaskDispose.Send(IPType.InSite, GroupMessage.QueryInSite());
            if (SystemConfiguration.InDoorON == 1)
            {
                SendCommand(1);
            }
            else if (SystemConfiguration.InDoorON == 2)
            {
                SendCommand(2);
            }
            if (SystemConfiguration.IsRunDoor)
            {
                DoorTask();
            }
            //状态1分钟未更新默认门为关闭状态
            if (UTC.ConvertDateTimeLong(DateTime.Now) - SystemTaskDispose.DoorInfoArray[DoorType.In].UpdateDateTime > 10)
            {
                SystemTaskDatabase.Instance.UpdateBufferStatus(1, 1);
                SystemTaskDatabase.Instance.UpdateBufferStatus(2, 1);
                SystemTaskDatabase.Instance.UpdateBufferStatus(3, 1);
                SystemTaskDispose.DoorInfoArray[DoorType.In].DoorStatus = DoorStatus.Close;
                throw new Exception("10秒未收到入库PLC状态更新消息，已强制更改门状态为关闭，缓存位状态为已占用");
            }
        }

        //发送指令
        private void SendCommand(int command)
        {
            SystemTaskDispose.Send(IPType.InSite, GroupMessage.writeSiteInPLC(command));
            Log4NetHelper.WriteTaskLog($"入库手动操作，指令command:{command}");
            OperateIniTool.OperateIniWrite(0, "Service", "InDoorNO");
        }


        private void DoorTask()
        {
            foreach (var agvNumber in SystemTaskDispose.agvInfoList.Keys)
            {
                var info = SystemTaskDispose.agvInfoList[agvNumber];

                if (info.ThisStation == 44 || info.ThisStation == 192 || info.ThisStation == 193)
                {
                    SendCommand(1);
                    Log4NetHelper.WriteTaskLog($"入库请求开门,当前站点：{info.ThisStation}");
                }
                else if (info.ThisStation == 46)
                {
                    SendCommand(2);
                    Log4NetHelper.WriteTaskLog($"入库请求关门,当前站点：{info.ThisStation}");
                }
                else if (info.ThisStation == 184 || info.ThisStation == 13 || info.ThisStation == 191 || info.ThisStation == 190)
                {
                    SendCommand(1);
                    Log4NetHelper.WriteTaskLog($"入库请求开门,当前站点：{info.ThisStation}");
                }
                else if (info.ThisStation == 18)
                {
                    SendCommand(2);
                    Log4NetHelper.WriteTaskLog($"入库请求关门,当前站点：{info.ThisStation}");
                }
            }
        }
    }
}
