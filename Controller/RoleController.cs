using MMORPG_GameServer.CacheModel;

namespace MMORPG_GameServer.Controller
{
    class RoleController
    {
        #region 单例
        private RoleController() { }
        private static object lockObj = new object();
        private static RoleController instance;
        public static RoleController Instance 
        {
            get
            {
                if(instance == null)
                {
                    lock(lockObj) 
                    {
                        if(instance == null)
                        {
                            instance = new RoleController();
                        } 
                    }
                }
                return instance;
            }
        }
        #endregion

        public void Init()
        {
            EventDispatcher.Instance.AddListener(ProtoCodeDef.RoleOperation_LogOnGameServer, OnLogonGameServer);
        }

        private void OnLogonGameServer(byte[] buffer, Role role)
        {
            var protocol = RoleOperation_LogOnGameServerProto.GetProto(buffer);
            RoleOperation_LogOnGameServerReturnProto returnProtocol = new RoleOperation_LogOnGameServerReturnProto();
            var roleItemList = RoleCacheModel.Instance.GetRoleItemList(protocol.AccountId);
            returnProtocol.RoleCount = roleItemList.Count;
            returnProtocol.RoleList = roleItemList;
            role.ClientSocket.SendMsg(returnProtocol.ToArray());
        }
    }
}
