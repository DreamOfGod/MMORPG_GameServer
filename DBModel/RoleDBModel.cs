using MMORPG_GameServer.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;

namespace MMORPG_GameServer.DBModel
{
    class RoleDBModel
    {
        #region 单例
        private RoleDBModel() { }
        public static readonly RoleDBModel Instance = new RoleDBModel();
        #endregion

        /// <summary>
        /// 获取用户的角色列表
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        public async Task<List<RoleOperation_LogOnGameServerReturnProto.RoleItem>> GetRoleItemList(int accountId)
        {
            using(var conn = new SqlConnection(DBConn.MMORPG_GameServer))
            {
                await conn.OpenAsync();
                var sql = $"select Id, Nickname, JobId, Level from Role where AccountId = { accountId }";
                var command = new SqlCommand(sql, conn);
                var reader = await command.ExecuteReaderAsync();
                var list = new List<RoleOperation_LogOnGameServerReturnProto.RoleItem>();
                while(await reader.ReadAsync())
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

        /// <summary>
        /// 创建角色的参数
        /// </summary>
        public struct CreateRoleParam
        {
            public int AccountId;
            public int JobId;
            public string Nickname;
        }

        /// <summary>
        /// 创建角色方法的返回值类型
        /// </summary>
        public enum CreateRoleReturn: byte
        {
            Success, Fail, NicknameRepeat
        }

        /// <summary>
        /// 创建角色
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        public async Task<CreateRoleReturn> CreateRole(CreateRoleParam roleParam)
        {
            using(var conn = new SqlConnection(DBConn.MMORPG_GameServer))
            {
                await conn.OpenAsync();
                var tran = await conn.BeginTransactionAsync(IsolationLevel.ReadCommitted) as SqlTransaction;//设为ReadCommitted允许一定程度的昵称重复
                try
                {
                    var selectSql = "select count(Id) from Role where Nickname = @Nickname";
                    var selectCmd = new SqlCommand(selectSql, conn, tran);
                    selectCmd.Parameters.Add(new SqlParameter("@Nickname", roleParam.Nickname));
                    int count = (int)selectCmd.ExecuteScalar();
                    if (count > 0)
                    {
                        //昵称重复
                        return CreateRoleReturn.NicknameRepeat;
                    }
                    var insertSql = $"insert into Role (Status, AccountId, JobId, Nickname, Level, CreateTime) values ({ ((byte)EntityStatus.Released) }, { roleParam.AccountId }, { roleParam.JobId }, @Nickname, 1, '{ DateTime.Now }')";
                    var insertCmd = new SqlCommand(insertSql, conn, tran);
                    insertCmd.Parameters.Add(new SqlParameter("@Nickname", roleParam.Nickname));
                    count = await insertCmd.ExecuteNonQueryAsync();
                    await tran.CommitAsync();
                    return count == 1 ? CreateRoleReturn.Success : CreateRoleReturn.Fail;
                }
                catch(Exception)
                {
                    await tran.RollbackAsync();
                    return CreateRoleReturn.Fail;
                }
            }
        }
    }
}
