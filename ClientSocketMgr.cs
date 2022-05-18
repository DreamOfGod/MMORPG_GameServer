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
        private List<ClientSocket> m_ClientList = new List<ClientSocket>();

        /// <summary>
        /// 添加客户端
        /// </summary>
        /// <param name="client"></param>
        public void AddClientSocket(ClientSocket client)
        {
            lock (m_ClientList)
            {
                m_ClientList.Add(client);
            }
        }

        /// <summary>
        /// 删除客户端
        /// </summary>
        /// <param name="client"></param>
        public void RemoveClientSocket(ClientSocket client)
        {
            lock (m_ClientList)
            {
                m_ClientList.Remove(client);
            }
        }
    }
}
