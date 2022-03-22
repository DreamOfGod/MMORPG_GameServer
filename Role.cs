using System;
using System.Collections.Generic;
using System.Text;

namespace MMORPG_GameServer
{
    /// <summary>
    /// 角色
    /// </summary>
    public class Role
    {
        /// <summary>
        /// 角色的客户端Socket对象
        /// </summary>
        private ClientSocket m_ClientSocket;

        public ClientSocket ClientSocket { get { return m_ClientSocket; } }

        public Role(ClientSocket clientSocket)
        {
            m_ClientSocket = clientSocket;
        }
    }
}