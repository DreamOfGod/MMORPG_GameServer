/// <summary>
/// 类名 : RoleDBModel
/// 作者 : 
/// 说明 : 
/// 创建日期 : 2022-04-09 12:41:57
/// </summary>
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;

using Mmcoy.Framework.AbstractBase;

/// <summary>
/// DBModel
/// </summary>
public partial class RoleDBModel : MFAbstractSQLDBModel<RoleEntity>
{
    #region RoleDBModel 私有构造
    /// <summary>
    /// 私有构造
    /// </summary>
    private RoleDBModel()
    {

    }
    #endregion

    #region 单例
    private static object lock_object = new object();
    private static RoleDBModel instance = null;
    public static RoleDBModel Instance
    {
        get
        {
            if (instance == null)
            {
                lock (lock_object)
                {
                    if (instance == null)
                    {
                        instance = new RoleDBModel();
                    }
                }
            }
            return instance;
        }
    }
    #endregion

    #region 实现基类的属性和方法

    #region ConnectionString 数据库连接字符串
    /// <summary>
    /// 数据库连接字符串
    /// </summary>
    protected override string ConnectionString
    {
        get { return DBConn.MMORPG_GameServer; }
    }
    #endregion

    #region TableName 表名
    /// <summary>
    /// 表名
    /// </summary>
    protected override string TableName
    {
        get { return "Role"; }
    }
    #endregion

    #region ColumnList 列名集合
    private IList<string> _ColumnList;
    /// <summary>
    /// 列名集合
    /// </summary>
    protected override IList<string> ColumnList
    {
        get
        {
            if (_ColumnList == null)
            {
                _ColumnList = new List<string> { "Id", "Status", "AccountId", "GameServerId", "JobID", "Nickname", "Sex", "Level", "CreateTime", "UpdateTime" };
            }
            return _ColumnList;
        }
    }
    #endregion

    #region ValueParas 转换参数
    /// <summary>
    /// 转换参数
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    protected override SqlParameter[] ValueParas(RoleEntity entity)
    {
        SqlParameter[] parameters = new SqlParameter[] {
                new SqlParameter("@Id", entity.Id) { DbType = DbType.Int32 },
                new SqlParameter("@Status", entity.Status) { DbType = DbType.Byte },
                new SqlParameter("@AccountId", entity.AccountId) { DbType = DbType.Int32 },
                new SqlParameter("@GameServerId", entity.GameServerId) { DbType = DbType.Int32 },
                new SqlParameter("@JobID", entity.JobID) { DbType = DbType.Int32 },
                new SqlParameter("@Nickname", entity.Nickname) { DbType = DbType.String },
                new SqlParameter("@Sex", entity.Sex) { DbType = DbType.Byte },
                new SqlParameter("@Level", entity.Level) { DbType = DbType.Int32 },
                new SqlParameter("@CreateTime", entity.CreateTime) { DbType = DbType.DateTime },
                new SqlParameter("@UpdateTime", entity.UpdateTime) { DbType = DbType.DateTime },
                new SqlParameter("@RetMsg", SqlDbType.NVarChar, 255),
                new SqlParameter("@ReturnValue", SqlDbType.Int)
            };
        return parameters;
    }
    #endregion

    #region GetEntitySelfProperty 封装对象
    /// <summary>
    /// 封装对象
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="table"></param>
    /// <returns></returns>
    protected override RoleEntity GetEntitySelfProperty(IDataReader reader, DataTable table)
    {
        RoleEntity entity = new RoleEntity();
        foreach (DataRow row in table.Rows)
        {
            var colName = row.Field<string>(0);
            if (reader[colName] is DBNull)
                continue;
            switch (colName.ToLower())
            {
                case "id":
                    if (!(reader["Id"] is DBNull))
                        entity.Id = Convert.ToInt32(reader["Id"]);
                    break;
                case "status":
                    if (!(reader["Status"] is DBNull))
                        entity.Status = (EnumEntityStatus)Convert.ToInt32(reader["Status"]);
                    break;
                case "accountid":
                    if (!(reader["AccountId"] is DBNull))
                        entity.AccountId = Convert.ToInt32(reader["AccountId"]);
                    break;
                case "gameserverid":
                    if (!(reader["GameServerId"] is DBNull))
                        entity.GameServerId = Convert.ToInt32(reader["GameServerId"]);
                    break;
                case "jobid":
                    if (!(reader["JobID"] is DBNull))
                        entity.JobID = Convert.ToInt32(reader["JobID"]);
                    break;
                case "nickname":
                    if (!(reader["Nickname"] is DBNull))
                        entity.Nickname = Convert.ToString(reader["Nickname"]);
                    break;
                case "sex":
                    if (!(reader["Sex"] is DBNull))
                        entity.Sex = Convert.ToByte(reader["Sex"]);
                    break;
                case "level":
                    if (!(reader["Level"] is DBNull))
                        entity.Level = Convert.ToInt32(reader["Level"]);
                    break;
                case "createtime":
                    if (!(reader["CreateTime"] is DBNull))
                        entity.CreateTime = Convert.ToDateTime(reader["CreateTime"]);
                    break;
                case "updatetime":
                    if (!(reader["UpdateTime"] is DBNull))
                        entity.UpdateTime = Convert.ToDateTime(reader["UpdateTime"]);
                    break;
            }
        }
        return entity;
    }
    #endregion

    #endregion
}
