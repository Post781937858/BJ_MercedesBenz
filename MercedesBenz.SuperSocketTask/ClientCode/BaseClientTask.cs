using SuperSocket.ClientEngine;
using System;
using System.Net;

namespace MercedesBenz.SuperSocketTask.ClientCode
{
    public abstract class BaseClientTask
    {
        public IPEndPoint iPEndPoint;

        public abstract void MessageAnalysis(byte[] messageData);

        public abstract void ClientTaskRun();
    }
}
