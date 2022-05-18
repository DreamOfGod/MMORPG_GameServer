using MMORPG_GameServer.CacheModel;
using MMORPG_GameServer.DBModel;
using static MMORPG_GameServer.DBModel.RoleDBModel;

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
            SocketMsgDispatcher.Instance.AddListener(ProtoCodeDef.RoleOperation_CreateRole, OnCreateRole);
        }

        private void OnLogonGameServer(byte[] buffer, ClientSocket clientSocket)
        {
            var logonServerProto = RoleOperation_LogOnGameServerProto.GetProto(buffer);
            clientSocket.AccountId = logonServerProto.AccountId;
            var roleItemList = RoleCacheModel.Instance.GetRoleItemList(logonServerProto.AccountId);
            var logonServerReturnProto = new RoleOperation_LogOnGameServerReturnProto();
            logonServerReturnProto.RoleCount = roleItemList.Count;
            logonServerReturnProto.RoleList = roleItemList;
            clientSocket.BeginSend(logonServerReturnProto.ToArray());
        }

        private async void OnCreateRole(byte[] buffer, ClientSocket clientSocket)
        {
            var createRoleProto = RoleOperation_CreateRoleProto.GetProto(buffer);
            var roleParam = new CreateRoleParam() { AccountId = clientSocket.AccountId, JobId = createRoleProto.JobId, Nickname = createRoleProto.RoleNickName, };
            var result = await RoleDBModel.Instance.CreateRole(roleParam);
            var createRoleReturnProto = new RoleOperation_CreateRoleReturnProto();
            switch(result)
            {
                case CreateRoleReturn.Success:
                    createRoleReturnProto.IsSuccess = true;
                    createRoleReturnProto.MsgCode = 0;
                    break;
                case CreateRoleReturn.NicknameRepeat:
                    createRoleReturnProto.IsSuccess = false;
                    createRoleReturnProto.MsgCode = 1;
                    break;
                case CreateRoleReturn.Fail:
                    createRoleReturnProto.IsSuccess = false;
                    createRoleReturnProto.MsgCode = 2;
                    break;
            }
            clientSocket.BeginSend(createRoleReturnProto.ToArray());
        }
    }
}
