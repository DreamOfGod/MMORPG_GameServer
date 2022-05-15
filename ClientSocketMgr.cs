using System.Collections.Generic;

namespace MMORPG_GameServer
{
    public class ClientSocketMgr
    {
        #region 单例
        private ClientSocketMgr() { }
        public static readonly ClientSocketMgr Instance = new ClientSocketMgr();
        #endregion

        /// <summary>
        /// 客户端Socket列表
        /// </summary>
        private List<ClientSocket> m_ClientSocketList = new List<ClientSocket>();

        /// <summary>
        /// 添加客户端Socket
        /// </summary>
        /// <param name="clientSocket"></param>
        public void AddClientSocket(ClientSocket clientSocket)
        {
            lock (m_ClientSocketList)
            {
                m_ClientSocketList.Add(clientSocket);
            }
        }

        /// <summary>
        /// 删除客户端Socket
        /// </summary>
        /// <param name="clientSocket"></param>
        public void RemoveClientSocket(ClientSocket clientSocket)
        {
            lock (m_ClientSocketList)
            {
                m_ClientSocketList.Remove(clientSocket);
            }
        }
    }
}
