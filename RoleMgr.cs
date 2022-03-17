using System;
using System.Collections.Generic;
using System.Text;

namespace MMORPG_GameServer
{
    public class RoleMgr
    {
        #region 单例
        private static object lock_object = new object();
        private static RoleMgr instance;

        public static RoleMgr Instance
        {
            get
            {
                if(instance == null)
                {
                    lock(lock_object)
                    {
                        if(instance == null)
                        {
                            instance = new RoleMgr();
                        }
                    }
                }
                return instance;
            }
        }
        #endregion

        /// <summary>
        /// 角色列表
        /// </summary>
        private List<Role> m_AllRole;

        private RoleMgr()
        {
            m_AllRole = new List<Role>();
        }

        /// <summary>
        /// 添加角色
        /// </summary>
        /// <param name="role"></param>
        public void AddRole(Role role)
        {
            m_AllRole.Add(role);
        }

        /// <summary>
        /// 删除角色
        /// </summary>
        /// <param name="role"></param>
        public void RemoveRole(Role role)
        {
            m_AllRole.Remove(role);
        }
    }
}
