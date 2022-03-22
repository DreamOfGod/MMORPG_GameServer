using System;
using System.Collections.Generic;
using System.Text;

namespace MMORPG_GameServer.DBModel
{
    class TestDBModel
    {
        #region 单例
        private static object lock_object = new object();
        private static TestDBModel instance;

        public static TestDBModel Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (lock_object)
                    {
                        if (instance == null)
                        {
                            instance = new TestDBModel();
                        }
                    }
                }
                return instance;
            }
        }
        #endregion

        public void Init()
        {
            EventDispatcher.Instance.AddListener(ProtoCodeDef.Test, TestListener);
        }

        private void TestListener(byte[] buffer, Role role)
        {
            TestProto proto = TestProto.GetProto(buffer);
            Console.WriteLine("IsSuccess：{0}", proto.IsSuccess);
            Console.WriteLine("Name：{0}", proto.Name);
            for (int i = 0; i < proto.RoleList.Count; ++i)
            {
                Console.WriteLine("Role{0}: {1} {2}", i, proto.RoleList[i].RoleId, proto.RoleList[i].RoleName);
            }

            TestProto proto1 = new TestProto();
            proto1.IsSuccess = false;
            proto1.ErrorCode = 1;
            proto1.Count = 0;
            role.ClientSocket.SendMsg(proto1.ToArray());
        }
    }}
