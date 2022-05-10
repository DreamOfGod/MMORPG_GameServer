//===============================================
//作    者：
//创建时间：2022-03-22 15:54:04
//备    注：
//===============================================
using System;
using System.Collections.Generic;

namespace MMORPG_GameServer
{
    public class EventDispatcher
    {
        #region 单例
        private EventDispatcher() { }
        public static readonly EventDispatcher Instance = new EventDispatcher();
        #endregion

        //事件处理器的字典
        private Dictionary<ushort, HashSet<Action<byte[], Role>>> m_HandlerDic = new Dictionary<ushort, HashSet<Action<byte[], Role>>>();

        /// <summary>
        /// 添加Socket消息监听器
        /// </summary>
        /// <param name="protoCode"></param>
        /// <param name="handler"></param>
        public void AddListener(ushort protoCode, Action<byte[], Role> handler)
        {
            lock (m_HandlerDic)
            {
                HashSet<Action<byte[], Role>> handlerSet;
                if (!m_HandlerDic.TryGetValue(protoCode, out handlerSet))
                {
                    handlerSet = new HashSet<Action<byte[], Role>>();
                    m_HandlerDic.Add(protoCode, handlerSet);
                }
                handlerSet.Add(handler);
            }
        }

        /// <summary>
        /// 移除Socket消息监听器
        /// </summary>
        /// <param name="protoCode"></param>
        /// <param name="handler"></param>
        public void RemoveListener(ushort protoCode, Action<byte[], Role> handler)
        {
            lock (m_HandlerDic)
            {
                m_HandlerDic[protoCode].Remove(handler);
            }
        }

        /// <summary>
        /// 派发Socket消息
        /// </summary>
        /// <param name="protoCode"></param>
        /// <param name="buffer"></param>
        /// <param name="role"></param>
        public void Dispatch(ushort protoCode, byte[] buffer, Role role)
        {
            lock(m_HandlerDic)
            {
                HashSet<Action<byte[], Role>> handlerSet;
                if (m_HandlerDic.TryGetValue(protoCode, out handlerSet))
                {
                    if (handlerSet.Count > 0)
                    {
                        Console.WriteLine($"派发Socket消息，协议ID：{ protoCode }");
                        foreach (var handler in handlerSet)
                        {
                            handler(buffer, role);
                        }
                        return;
                    }
                }
                Console.WriteLine($"消息没有处理器，协议ID：{ protoCode }");
            }
        }
    }
}