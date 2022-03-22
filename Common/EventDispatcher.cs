//===============================================
//作    者：
//创建时间：2022-03-22 15:54:04
//备    注：
//===============================================
using System.Collections.Generic;

namespace MMORPG_GameServer
{
    public class EventDispatcher
    {
        #region 单例
        private static object lock_object = new object();
        private static EventDispatcher instance;

        public static EventDispatcher Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (lock_object)
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

        public delegate void EventHanlder(byte[] buffer, Role role);

        private Dictionary<ushort, HashSet<EventHanlder>> m_HandlerDic = new Dictionary<ushort, HashSet<EventHanlder>>();

        public void AddListener(ushort protoCode, EventHanlder handler)
        {
            if (m_HandlerDic.ContainsKey(protoCode))
            {
                m_HandlerDic[protoCode].Add(handler);
            }
            else
            {
                HashSet<EventHanlder> handlerSet = new HashSet<EventHanlder>();
                handlerSet.Add(handler);
                m_HandlerDic[protoCode] = handlerSet;
            }
        }

        public void RemoveListener(ushort protoCode, EventHanlder handler)
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
                HashSet<EventHanlder> handlerSet = m_HandlerDic[protoCode];
                foreach (EventHanlder handler in handlerSet)
                {
                    handler(buffer, role);
                }
            }
        }
    }
}