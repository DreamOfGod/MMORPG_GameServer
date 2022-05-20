using MMORPG_GameServer.DBModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static MMORPG_GameServer.DBModel.RoleDBModel;

namespace MMORPG_GameServer.CacheModel
{
    class RoleCacheModel
    {
        #region 单例
        private RoleCacheModel() { }
        public static readonly RoleCacheModel Instance = new RoleCacheModel();
        #endregion

        /// <summary>
        /// 获取用户的角色列表
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        public async Task<List<RoleOperation_LogOnGameServerReturnProto.RoleItem>> GetRoleItemList(int accountId)
        {
            return await RoleDBModel.Instance.GetRoleItemList(accountId);
        }

        /// <summary>
        /// 创建角色
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        public async Task<CreateRoleReturn> CreateRole(CreateRoleParam roleParam)
        {
            return await RoleDBModel.Instance.CreateRole(roleParam);
        }
    }
}
