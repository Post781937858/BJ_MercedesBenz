using System;

namespace MercedesBenz.SuperSocketTask.ServerCode
{
    public abstract class SuperSocketBaseTask
    {
        public abstract void ServerTaskRun();

        public abstract void MessageDispose(ServerSession session, ServerRequestInfo requestInfo);
    }
}
