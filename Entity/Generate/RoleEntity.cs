/// <summary>
/// 类名 : RoleEntity
/// 作者 : 
/// 说明 : 
/// 创建日期 : 2022-04-09 12:41:36
/// </summary>
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Mmcoy.Framework.AbstractBase;

/// <summary>
/// 
/// </summary>
[Serializable]
public partial class RoleEntity : MFAbstractEntity
{
    #region 重写基类属性
    /// <summary>
    /// 主键
    /// </summary>
    public override int? PKValue
    {
        get
        {
            return this.Id;
        }
        set
        {
            this.Id = value;
        }
    }
    #endregion

    #region 实体属性

    /// <summary>
    /// 编号
    /// </summary>
    public int? Id { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    public EnumEntityStatus Status { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public int AccountId { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public int GameServerId { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public int JobID { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string Nickname { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public byte Sex { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public int Level { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public DateTime CreateTime { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public DateTime UpdateTime { get; set; }

    #endregion
}
