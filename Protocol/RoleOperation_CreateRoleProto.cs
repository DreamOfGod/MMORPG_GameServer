//===============================================
//作    者：
//创建时间：2022-04-20 15:54:30
//备    注：
//===============================================
//===================================================
//作    者：
//创建时间：2022-04-20 15:53:36
//备    注：
//===================================================
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// 客户端发送创建角色消息
/// </summary>
public struct RoleOperation_CreateRoleProto : IProtocol
{
    public ushort ProtoCode { get { return 10003; } }

    public byte JobId; //职业ID
    public string RoleNickName; //角色名称

    public byte[] ToArray()
    {
        using (MMO_MemoryStream ms = new MMO_MemoryStream())
        {
            ms.WriteUShort(ProtoCode);
            ms.WriteByte(JobId);
            ms.WriteUTF8String(RoleNickName);
            return ms.ToArray();
        }
    }

    public static RoleOperation_CreateRoleProto GetProto(byte[] buffer)
    {
        RoleOperation_CreateRoleProto proto = new RoleOperation_CreateRoleProto();
        using (MMO_MemoryStream ms = new MMO_MemoryStream(buffer))
        {
            proto.JobId = (byte)ms.ReadByte();
            proto.RoleNickName = ms.ReadUTF8String();
        }
        return proto;
    }
}