using MMORPG_GameServer.DBModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MMORPG_GameServer.CacheModel
{
    class RoleCacheModel
    {
        #region 单例
        private RoleCacheModel() { }
        public static RoleCacheModel Instance = new RoleCacheModel();
        #endregion

        public List<RoleOperation_LogOnGameServerReturnProto.RoleItem> GetRoleItemList(int accountId)
        {
            return RoleDBModel.Instance.GetRoleItemList(accountId);
        }
    }
}
