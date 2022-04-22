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
/// 客户端发送穿戴消息
/// </summary>
public struct Goods_EquipPutProto : IProtocol
{
    public ushort ProtoCode { get { return 16012; } }

    public byte Type; //0=穿上 1=脱下
    public int GoodsId; //装备编号
    public int GoodsServerId; //装备服务器端编号

    public byte[] ToArray()
    {
        using (MMO_MemoryStream ms = new MMO_MemoryStream())
        {
            ms.WriteUShort(ProtoCode);
            ms.WriteByte(Type);
            ms.WriteInt(GoodsId);
            ms.WriteInt(GoodsServerId);
            return ms.ToArray();
        }
    }

    public static Goods_EquipPutProto GetProto(byte[] buffer)
    {
        Goods_EquipPutProto proto = new Goods_EquipPutProto();
        using (MMO_MemoryStream ms = new MMO_MemoryStream(buffer))
        {
            proto.Type = (byte)ms.ReadByte();
            proto.GoodsId = ms.ReadInt();
            proto.GoodsServerId = ms.ReadInt();
        }
        return proto;
    }
}