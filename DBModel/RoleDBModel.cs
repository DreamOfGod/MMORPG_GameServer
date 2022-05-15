using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;

namespace MMORPG_GameServer.DBModel
{
    class RoleDBModel
    {
        #region 单例
        private RoleDBModel() { }
        public static RoleDBModel Instance = new RoleDBModel();
        #endregion

        public List<RoleOperation_LogOnGameServerReturnProto.RoleItem> GetRoleItemList(int accountId)
        {
            using(var conn = new SqlConnection(DBConn.MMORPG_GameServer))
            {
                conn.Open();
                var sql = $"select Id, Nickname, JobId, Level from Role where AccountId = { accountId }";
                var command = new SqlCommand(sql, conn);
                var reader = command.ExecuteReader();
                var list = new List<RoleOperation_LogOnGameServerReturnProto.RoleItem>();
                while(reader.Read())
                {
                    var roleItem = new RoleOperation_LogOnGameServerReturnProto.RoleItem();
                    roleItem.RoleId = reader.GetInt32(0);
                    roleItem.RoleNickName = reader.GetString(1);
                    roleItem.RoleJob = reader.GetByte(2);
                    roleItem.RoleLevel = reader.GetInt32(3);
                    list.Add(roleItem);
                }
                return list;
            }
        }
    }
}
