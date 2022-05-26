using MMORPG_GameServer.CacheModel;
using MMORPG_GameServer.Common;
using static MMORPG_GameServer.DBModel.RoleDBModel;

namespace MMORPG_GameServer.Controller
{
    class RoleController: IController
    {
        #region 单例
        private RoleController() { }
        public static readonly RoleController Instance = new RoleController();
        #endregion

        #region Init、Reset
        public void Init()
        {
            SocketMsgDispatcher.Instance.AddListener(ProtoCodeDef.RoleOperation_LogOnGameServer, OnLogonGameServer);
            SocketMsgDispatcher.Instance.AddListener(ProtoCodeDef.RoleOperation_CreateRole, OnCreateRole);
            SocketMsgDispatcher.Instance.AddListener(ProtoCodeDef.RoleOperation_EnterGame, OnEnterGame);
        }

        public void Reset()
        {
            SocketMsgDispatcher.Instance.RemoveListener(ProtoCodeDef.RoleOperation_LogOnGameServer, OnLogonGameServer);
            SocketMsgDispatcher.Instance.RemoveListener(ProtoCodeDef.RoleOperation_CreateRole, OnCreateRole);
            SocketMsgDispatcher.Instance.RemoveListener(ProtoCodeDef.RoleOperation_EnterGame, OnEnterGame);
        }
        #endregion

        //登陆游戏服消息回调
        private async void OnLogonGameServer(byte[] buffer, ClientSocket clientSocket)
        {
            var logonServerProto = RoleOperation_LogOnGameServerProto.GetProto(buffer);
            clientSocket.AccountId = logonServerProto.AccountId;
            var roleItemList = await RoleCacheModel.Instance.GetRoleItemList(logonServerProto.AccountId);
            var logonServerReturnProto = new RoleOperation_LogOnGameServerReturnProto();
            logonServerReturnProto.RoleCount = roleItemList.Count;
            logonServerReturnProto.RoleList = roleItemList;
            clientSocket.BeginSend(logonServerReturnProto.ToArray());
        }

        //创建角色消息回调
        private async void OnCreateRole(byte[] buffer, ClientSocket clientSocket)
        {
            var createRoleProto = RoleOperation_CreateRoleProto.GetProto(buffer);
            var roleParam = new CreateRoleParam() { AccountId = clientSocket.AccountId, JobId = createRoleProto.JobId, Nickname = createRoleProto.RoleNickName, };
            var id = await RoleCacheModel.Instance.CreateRole(roleParam);
            var createRoleReturnProto = new RoleOperation_CreateRoleReturnProto();
            if(id >= 0)
            {
                createRoleReturnProto.IsSuccess = true;
                createRoleReturnProto.RoleId = id;
                createRoleReturnProto.RoleNickName = roleParam.Nickname;
                createRoleReturnProto.RoleJob = roleParam.JobId;
                createRoleReturnProto.RoleLevel = 1;
            }
            else
            {
                createRoleReturnProto.IsSuccess = false;
                createRoleReturnProto.MsgCode = id;
            }
            clientSocket.BeginSend(createRoleReturnProto.ToArray());
        }

        //进入游戏消息回调
        private void OnEnterGame(byte[] buffer, ClientSocket clientSocket)
        {
            var enterGameReturnProto = new RoleOperation_EnterGameReturnProto();
            enterGameReturnProto.IsSuccess = true;
            clientSocket.BeginSend(enterGameReturnProto.ToArray());
        }
    }
}
