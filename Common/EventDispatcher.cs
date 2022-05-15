//===============================================
//作    者：
//创建时间：2022-03-22 15:54:04
//备    注：
//===============================================
using System;
using System.Collections.Generic;

namespace MMORPG_GameServer
{
    /// <summary>
    /// Socket消息派发
    /// </summary>
    public class SocketMsgDispatcher
    {
        #region 单例
        private SocketMsgDispatcher() { }
        public static readonly SocketMsgDispatcher Instance = new SocketMsgDispatcher();
        #endregion

        //事件处理器的字典
        private Dictionary<ushort, HashSet<Action<byte[], ClientSocket>>> m_HandlerDic = new Dictionary<ushort, HashSet<Action<byte[], ClientSocket>>>();

        #region 添加Socket消息监听器
        /// <summary>
        /// 添加Socket消息监听器
        /// </summary>
        /// <param name="protoCode"></param>
        /// <param name="handler"></param>
        public void AddListener(ushort protoCode, Action<byte[], ClientSocket> handler)
        {
            lock (m_HandlerDic)
            {
                HashSet<Action<byte[], ClientSocket>> handlerSet;
                if (!m_HandlerDic.TryGetValue(protoCode, out handlerSet))
                {
                    handlerSet = new HashSet<Action<byte[], ClientSocket>>();
                    m_HandlerDic.Add(protoCode, handlerSet);
                }
                handlerSet.Add(handler);
            }
        }
        #endregion

        #region 移除Socket消息监听器
        /// <summary>
        /// 移除Socket消息监听器
        /// </summary>
        /// <param name="protoCode"></param>
        /// <param name="handler"></param>
        public void RemoveListener(ushort protoCode, Action<byte[], ClientSocket> handler)
        {
            lock (m_HandlerDic)
            {
                m_HandlerDic[protoCode].Remove(handler);
            }
        }
        #endregion

        #region 派发Socket消息
        /// <summary>
        /// 派发Socket消息
        /// </summary>
        /// <param name="protoCode"></param>
        /// <param name="buffer"></param>
        /// <param name="role"></param>
        public void Dispatch(ushort protoCode, byte[] buffer, ClientSocket clientdSocket)
        {
            lock(m_HandlerDic)
            {
                HashSet<Action<byte[], ClientSocket>> handlerSet;
                if (m_HandlerDic.TryGetValue(protoCode, out handlerSet))
                {
                    if (handlerSet.Count > 0)
                    {
                        Console.WriteLine($"派发Socket消息，协议ID：{ protoCode }");
                        foreach (var handler in handlerSet)
                        {
                            handler(buffer, clientdSocket);
                        }
                        return;
                    }
                }
                Console.WriteLine($"消息没有处理器，协议ID：{ protoCode }");
            }
        }
        #endregion
    }
}