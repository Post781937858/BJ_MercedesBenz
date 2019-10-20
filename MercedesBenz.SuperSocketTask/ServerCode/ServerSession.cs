using SuperSocket.SocketBase;
using System;

namespace MercedesBenz.SuperSocketTask.ServerCode
{
    public class ServerSession : AppSession<ServerSession, ServerRequestInfo>
    {

        protected override void HandleException(Exception e)
        {
            base.HandleException(e);
        }

        protected override void OnSessionStarted()
        {
            base.OnSessionStarted();
        }

        protected override void HandleUnknownRequest(ServerRequestInfo requestInfo)
        {
            base.HandleUnknownRequest(requestInfo);
        }
    }
}
