namespace MMORPG_GameServer
{
    /// <summary>
    /// 协议接口
    /// </summary>
    public interface IProtocol
    {
        /// <summary>
        /// 协议编号
        /// </summary>
        ushort ProtocolID { get; }
    }
}