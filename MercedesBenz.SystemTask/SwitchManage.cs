﻿using MercedesBenz.DataBase;
using MercedesBenz.Infrastructure;
using MercedesBenz.Models;
using MercedesBenz.SystemTask.Server.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MercedesBenz.SystemTask
{
    public class SwitchManage : BaseTcpClientServer
    {
        public SwitchManage(IPType type) : base(type)
        { }

        public override void MessageAnalysis(byte[] mes, IPType type)
        {
            Log4NetHelper.WriteDebugLog(string.Join(" ", mes.Select(s => s.ToString("X2"))));
            foreach (var messageitem in BytePackagedis.AnalysisSwitchByte(mes))
            {
                if (messageitem.Length < 36)
                    break;
                switch (type)
                {
                    case IPType.AL1_1:
                        SystemTaskDatabase.Instance.UpdateStatus(6, messageitem[35]);
                        break;
                    case IPType.AL1_2:
                        SystemTaskDatabase.Instance.UpdateStatus(2, messageitem[35]);
                        break;
                    case IPType.AL2_1:
                        SystemTaskDatabase.Instance.UpdateStatus(5, messageitem[35]);
                        break;
                    case IPType.AL2_2:
                        SystemTaskDatabase.Instance.UpdateStatus(1, messageitem[35]);
                        break;
                    case IPType.AL3_1:
                        SystemTaskDatabase.Instance.UpdateStatus(4, messageitem[35]);
                        break;
                    case IPType.AL3_2:
                        SystemTaskDatabase.Instance.UpdateStatus(3, messageitem[35]);
                        break;
                }
                Log4NetHelper.WriteTaskLog($"来自{type.ToString()}【{messageitem[35]}】：{ string.Join(" ", messageitem.Select(s => s.ToString("X2")))}");
            }
        }

        public override void ServerTaskRun()
        {
            var SwitchInfoList = SystemTaskDatabase.Instance.QuerySwitchConfiguration();
            SwitchInfoList.ForEach(SwitchItem=> {
                switch (SwitchItem.ID)
                {
                    case 1:
                        if (SwitchItem.Switch == 1)
                        {
                            base.Send(IPType.AL2_2, new byte[] { 0x68, 0x00, 0x11, 0x35, 0xE1, 0x41, 0x36, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x39, 0x38, 0x44, 0x38, 0x36, 0x33, 0x34, 0x31, 0x32, 0x45, 0x32, 0x36, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x01, 0x02, 0xFF, 0x03, 0xFF, 0x04, 0xFF, 0x05, 0xFF, 0x06, 0xFF, 0x07, 0xFF, 0x08, 0xFF, 0x7E });
                            
                        }
                        else if(SwitchItem.Switch == 2)
                        {
                            base.Send(IPType.AL2_2, new byte[] { 0x68, 0x00, 0x11, 0x35, 0xE1, 0x41, 0x36, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x39, 0x38, 0x44, 0x38, 0x36, 0x33, 0x34, 0x31, 0x32, 0x45, 0x32, 0x36, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x02, 0xFF, 0x03, 0xFF, 0x04, 0xFF, 0x05, 0xFF, 0x06, 0xFF, 0x07, 0xFF, 0x08, 0xFF, 0x7D });
                        }
                        break;
                    case 2:
                        if (SwitchItem.Switch == 1)
                        {
                            base.Send(IPType.AL1_2, new byte[] { 0x68, 0x00, 0x11, 0x35, 0xE1, 0x41, 0x31, 0x36, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x39, 0x38, 0x44, 0x38, 0x36, 0x33, 0x34, 0x31, 0x32, 0x44, 0x37, 0x39, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x01, 0x02, 0xFF, 0x03, 0xFF, 0x04, 0xFF, 0x05, 0xFF, 0x06, 0xFF, 0x07, 0xFF, 0x08, 0xFF, 0x96 });
                        }
                        else if(SwitchItem.Switch == 2)
                        {
                            base.Send(IPType.AL1_2, new byte[] { 0x68, 0x00, 0x11, 0x35, 0xE1, 0x41, 0x31, 0x36, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x39, 0x38, 0x44, 0x38, 0x36, 0x33, 0x34, 0x31, 0x32, 0x44, 0x37, 0x39, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x02, 0xFF, 0x03, 0xFF, 0x04, 0xFF, 0x05, 0xFF, 0x06, 0xFF, 0x07, 0xFF, 0x08, 0xFF, 0x95 });
                        }
                        break;
                    case 3:
                        if (SwitchItem.Switch == 1)
                        {
                            base.Send(IPType.AL3_2, new byte[] { 0x68, 0x00, 0x11, 0x35, 0xE1, 0x41, 0x33, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x39, 0x38, 0x44, 0x38, 0x36, 0x33, 0x34, 0x31, 0x32, 0x37, 0x41, 0x33, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x01, 0x02, 0xFF, 0x03, 0xFF, 0x04, 0xFF, 0x05, 0xFF, 0x06, 0xFF, 0x07, 0xFF, 0x08, 0xFF, 0x79 });
                        }
                        else if (SwitchItem.Switch == 2)
                        {
                            base.Send(IPType.AL3_2, new byte[] { 0x68, 0x00, 0x11, 0x35, 0xE1, 0x41, 0x33, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x39, 0x38, 0x44, 0x38, 0x36, 0x33, 0x34, 0x31, 0x32, 0x37, 0x41, 0x33, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x02, 0xFF, 0x03, 0xFF, 0x04, 0xFF, 0x05, 0xFF, 0x06, 0xFF, 0x07, 0xFF, 0x08, 0xFF, 0x78 });
                        }
                        break;
                    case 4:

                        if (SwitchItem.Switch == 1)
                        {
                            base.Send(IPType.AL3_1, new byte[] { 0x68, 0x00, 0x11, 0x35, 0xE1, 0x41, 0x31, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x39, 0x38, 0x44, 0x38, 0x36, 0x33, 0x34, 0x31, 0x32, 0x44, 0x36, 0x41, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x01, 0x02, 0xFF, 0x03, 0xFF, 0x04, 0xFF, 0x05, 0xFF, 0x06, 0xFF, 0x07, 0xFF, 0x08, 0xFF, 0x87 });
                        }
                        else if (SwitchItem.Switch == 2)
                        {
                           base.Send(IPType.AL3_1, new byte[] { 0x68, 0x00, 0x11, 0x35, 0xE1, 0x41, 0x31, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x39, 0x38, 0x44, 0x38, 0x36, 0x33, 0x34, 0x31, 0x32, 0x44, 0x36, 0x41, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x02, 0xFF, 0x03, 0xFF, 0x04, 0xFF, 0x05, 0xFF, 0x06, 0xFF, 0x07, 0xFF, 0x08, 0xFF, 0x86 });
                        }
                        break;
                    case 5:
                        if (SwitchItem.Switch == 1)
                        {
                            base.Send(IPType.AL2_1, new byte[] { 0x68, 0x00, 0x11, 0x35, 0xE1, 0x41, 0x31, 0x34, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x39, 0x38, 0x44, 0x38, 0x36, 0x33, 0x34, 0x31, 0x32, 0x45, 0x31, 0x34, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x01, 0x02, 0xFF, 0x03, 0xFF, 0x04, 0xFF, 0x05, 0xFF, 0x06, 0xFF, 0x07, 0xFF, 0x08, 0xFF, 0x8A });
                        }
                        else if (SwitchItem.Switch == 2)
                        {
                            base.Send(IPType.AL2_1, new byte[] { 0x68, 0x00, 0x11, 0x35, 0xE1, 0x41, 0x31, 0x34, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x39, 0x38, 0x44, 0x38, 0x36, 0x33, 0x34, 0x31, 0x32, 0x45, 0x31, 0x34, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x02, 0xFF, 0x03, 0xFF, 0x04, 0xFF, 0x05, 0xFF, 0x06, 0xFF, 0x07, 0xFF, 0x08, 0xFF, 0x89 });
                        }
                        break;
                    case 6:
                        if (SwitchItem.Switch == 1)
                        {
                            base.Send(IPType.AL1_1, new byte[] { 0x68, 0x00, 0x11, 0x35, 0xE1, 0x41, 0x39, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x39, 0x38, 0x44, 0x38, 0x36, 0x33, 0x34, 0x31, 0x32, 0x45, 0x42, 0x35, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x01, 0x02, 0xFF, 0x03, 0xFF, 0x04, 0xFF, 0x05, 0xFF, 0x06, 0xFF, 0x07, 0xFF, 0x08, 0xFF, 0x90 });
                        }
                        else if (SwitchItem.Switch == 2)
                        {
                            base.Send(IPType.AL1_1, new byte[] { 0x68, 0x00, 0x11, 0x35, 0xE1, 0x41, 0x39, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x39, 0x38, 0x44, 0x38, 0x36, 0x33, 0x34, 0x31, 0x32, 0x45, 0x42, 0x35, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x02, 0xFF, 0x03, 0xFF, 0x04, 0xFF, 0x05, 0xFF, 0x06, 0xFF, 0x07, 0xFF, 0x08, 0xFF, 0x8F });
                        }
                        break;
                }
                if (SwitchItem.Switch == 1 || SwitchItem.Switch == 2)
                {
                    SystemTaskDatabase.Instance.UpdateSwitch(SwitchItem.ID, 0);
                }
            });
        }
    }
}
