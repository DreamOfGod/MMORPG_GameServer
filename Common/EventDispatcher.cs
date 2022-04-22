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
        private static object lockObj = new object();
        private static EventDispatcher instance;
        public static EventDispatcher Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (lockObj)
                    {
                        if (instance == null)
                        {
                            instance = new EventDispatcher();
                        }
                    }
                }
                return instance;
            }
        }
        #endregion

        private Dictionary<ushort, HashSet<Action<byte[], Role>>> m_HandlerDic = new Dictionary<ushort, HashSet<Action<byte[], Role>>>();

        public void AddListener(ushort protoCode, Action<byte[], Role> handler)
        {
            if (m_HandlerDic.ContainsKey(protoCode))
            {
                m_HandlerDic[protoCode].Add(handler);
            }
            else
            {
                var handlerSet = new HashSet<Action<byte[], Role>>();
                handlerSet.Add(handler);
                m_HandlerDic[protoCode] = handlerSet;
            }
        }

        public void RemoveListener(ushort protoCode, Action<byte[], Role> handler)
        {
            if (m_HandlerDic.ContainsKey(protoCode))
            {
                m_HandlerDic[protoCode].Remove(handler);
            }
        }

        public void Dispatch(ushort protoCode, byte[] buffer, Role role)
        {
            if (m_HandlerDic.ContainsKey(protoCode))
            {
                var handlerSet = m_HandlerDic[protoCode];
                foreach (var handler in handlerSet)
                {
                    handler(buffer, role);
                }
            }
        }
    }
}