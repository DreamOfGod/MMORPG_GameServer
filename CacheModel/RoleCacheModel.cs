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
        private static object lockObj = new object();
        private static RoleCacheModel instance;
        public static RoleCacheModel Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (lockObj)
                    {
                        if (instance == null)
                        {
                            instance = new RoleCacheModel();
                        }
                    }
                }
                return instance;
            }
        }
        #endregion

        public List<RoleOperation_LogOnGameServerReturnProto.RoleItem> GetRoleItemList(int accountId)
        {
            return RoleDBModel.Instance.GetRoleItemList(accountId);
        }
    }
}
