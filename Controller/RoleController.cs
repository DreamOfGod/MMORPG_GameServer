using MMORPG_GameServer.CacheModel;

namespace MMORPG_GameServer.Controller
{
    class RoleController
    {
        #region 单例
        private RoleController() { }
        public static readonly RoleController Instance = new RoleController();
        #endregion

        public void Init()
        {
            SocketMsgDispatcher.Instance.AddListener(ProtoCodeDef.RoleOperation_LogOnGameServer, OnLogonGameServer);
        }

        private void OnLogonGameServer(byte[] buffer, ClientSocket clientSocket)
        {
            var protocol = RoleOperation_LogOnGameServerProto.GetProto(buffer);
            var returnProtocol = new RoleOperation_LogOnGameServerReturnProto();
            var roleItemList = RoleCacheModel.Instance.GetRoleItemList(protocol.AccountId);
            returnProtocol.RoleCount = roleItemList.Count;
            returnProtocol.RoleList = roleItemList;
            clientSocket.BeginSend(returnProtocol.ToArray());
        }
    }
}
