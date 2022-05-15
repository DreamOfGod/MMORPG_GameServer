using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace MMORPG_GameServer
{
    /// <summary>
    /// 客户端socket对象 负责和客户端进行通讯
    /// </summary>
    public class ClientSocket
    {
        //角色
        private Role m_Role;

        //锁对象
        private object m_Lock = new object();

        #region socket对象
        //客户端Socket
        private Socket m_Socket;

        //远程终端
        private IPEndPoint m_RemoteEndPoint;
        #endregion

        #region 发送相关字段
        //待发送的消息队列
        private Queue<byte[]> m_ToBeSentMsgQueue = new Queue<byte[]>();

        // 进行压缩的长度下限
        private const int m_CompressLength = 200;
        #endregion

        #region 接收相关字段
        // 接收数据包的字节数组缓冲区
        private byte[] m_ReceiveBuffer = new byte[2048];

        // 接收数据包的缓冲数据流
        private MMO_MemoryStream m_ReceiveMS = new MMO_MemoryStream();
        #endregion

        #region 构造函数
        public ClientSocket(Socket socket, Role role)
        {
            m_Socket = socket;
            m_Role = role;
            m_RemoteEndPoint = (IPEndPoint)socket.RemoteEndPoint;
            BeginReceive();
        }
        #endregion

        #region 异步接收
        //异步接收
        private void BeginReceive()
        {
            try
            {
                m_Socket.BeginReceive(m_ReceiveBuffer, 0, m_ReceiveBuffer.Length, SocketFlags.None, ReceiveCallback, null);
            }
            catch (SocketException ex)
            {
                try
                {
                    m_Socket.Shutdown(SocketShutdown.Both);
                }
                finally
                {
                    m_Socket.Close();
                    m_Socket = null;
                    ClientSocketMgr.Instance.RemoveClientSocket(this);
                    Console.WriteLine($"与{ m_RemoteEndPoint }的连接异常，exception：{ ex }");
                }
            }
            catch(ObjectDisposedException)
            {

            }
        }

        // 异步接收的回调
        private void ReceiveCallback(IAsyncResult asyncResult)
        {
            int count;
            lock (m_Lock)
            {
                if (m_Socket == null)
                {
                    return;
                }
                try
                {
                    count = m_Socket.EndReceive(asyncResult);
                }
                catch (SocketException ex)
                {
                    try
                    {
                        m_Socket.Shutdown(SocketShutdown.Both);
                    }
                    finally
                    {
                        m_Socket.Close();
                        m_Socket = null;
                        ClientSocketMgr.Instance.RemoveClientSocket(this);
                        Console.WriteLine($"与{ m_RemoteEndPoint }的连接异常，exception：{ ex }");
                    }
                    return;
                }
                catch (ObjectDisposedException)
                {
                    return;
                }
                if(count == 0)
                {
                    //客户端主动断开连接
                    try
                    {
                        m_Socket.Shutdown(SocketShutdown.Both);
                    }
                    finally
                    {
                        m_Socket.Close();
                        m_Socket = null;
                        ClientSocketMgr.Instance.RemoveClientSocket(this);
                        Console.WriteLine($"客户端{ m_RemoteEndPoint }主动断开连接");
                    }
                    return;
                }
            }

            //把接收的字节写入字节流的尾部
            m_ReceiveMS.Write(m_ReceiveBuffer, 0, count);
            ParseMsg();

            lock (m_Lock)
            {
                if(m_Socket != null)
                {
                    BeginReceive();
                }
            }
        }

        //解析消息
        private void ParseMsg()
        {
            //字节流长度达到2个字节，至少消息的数据包长度接收完成
            if (m_ReceiveMS.Length >= 2)
            {
                m_ReceiveMS.Position = 0;
                while (true)
                {
                    //读取内容的字节数
                    var contentCount = m_ReceiveMS.ReadUShort();
                    if (m_ReceiveMS.Length - m_ReceiveMS.Position >= contentCount)
                    {
                        //消息的内容已经接收完成
                        //压缩标志
                        var compressed = m_ReceiveMS.ReadBool();
                        //crc16校验码
                        var crc16 = m_ReceiveMS.ReadUShort();
                        //数据包
                        var content = new byte[contentCount - 3];
                        m_ReceiveMS.Read(content, 0, content.Length);
                        //crc16校验：校验不通过就丢弃
                        if (Crc16.CalculateCrc16(content) == crc16)
                        {
                            //数据包异或
                            SecurityUtil.XOR(content);
                            //解压
                            if (compressed)
                            {
                                content = ZlibHelper.DeCompressBytes(content);
                            }
                            var ms = new MMO_MemoryStream(content);
                            //协议ID
                            var protoCode = ms.ReadUShort();
                            //协议内容
                            var protoContent = new byte[contentCount - 2];
                            ms.Read(protoContent, 0, protoContent.Length);
                            //派发协议消息
                            SocketMsgDispatcher.Instance.Dispatch(protoCode, protoContent, this);
                        }
                        var leftCount = m_ReceiveMS.Length - m_ReceiveMS.Position;
                        if (leftCount < 2)
                        {
                            //剩余不到2个字节，将剩余字节移到字节流开头，等内容接收完整再解析
                            var buffer = m_ReceiveMS.GetBuffer();
                            long i = 0, j = m_ReceiveMS.Position;
                            while (j < m_ReceiveMS.Length)
                            {
                                buffer[i] = buffer[j];
                                ++i;
                                ++j;
                            }
                            m_ReceiveMS.SetLength(i);
                            break;
                        }
                    }
                    else
                    {
                        //消息内容接收不完整，将剩余字节移到字节流开头，等内容接收完整再解析
                        var buffer = m_ReceiveMS.GetBuffer();
                        long i = 0, j = m_ReceiveMS.Position;
                        while (j < m_ReceiveMS.Length)
                        {
                            buffer[i] = buffer[j];
                            ++i;
                            ++j;
                        }
                        m_ReceiveMS.SetLength(i);
                        break;
                    }
                }
            }
        }
        #endregion

        #region 异步发送
        // 封装数据包
        private byte[] MakeMsg(byte[] data)
        {
            //消息格式：数据包长度(ushort)|压缩标志(bool)|crc16(ushort)|先压缩后异或的数据包
            var ms = new MMO_MemoryStream();
            if (data.Length > m_CompressLength)
            {
                //压缩
                data = ZlibHelper.CompressBytes(data);
                //数据包长度
                ms.WriteUShort((ushort)(data.Length + 3));
                //压缩标志
                ms.WriteBool(true);
            }
            else
            {
                //不压缩
                //数据包长度
                ms.WriteUShort((ushort)(data.Length + 3));
                //压缩标志
                ms.WriteBool(false);
            }
            //异或
            SecurityUtil.XOR(data);
            //crc16
            ms.WriteUShort(Crc16.CalculateCrc16(data));
            //数据包
            ms.Write(data, 0, data.Length);
            return ms.ToArray();
        }

        /// <summary>
        /// 异步发送
        /// </summary>
        /// <param name="data"></param>
        public void BeginSend(byte[] data)
        {
            var msg = MakeMsg(data);
            lock (m_Lock)
            {
                if(m_Socket == null)
                {
                    return;
                }
                m_ToBeSentMsgQueue.Enqueue(msg);
                if (m_ToBeSentMsgQueue.Count == 1)
                {
                    try
                    {
                        m_Socket.BeginSend(msg, 0, msg.Length, SocketFlags.None, SendCallback, null);
                    }
                    catch (SocketException ex)
                    {
                        try
                        {
                            m_Socket.Shutdown(SocketShutdown.Both);
                        }
                        finally
                        {
                            m_Socket.Close();
                            m_Socket = null;
                            ClientSocketMgr.Instance.RemoveClientSocket(this);
                            Console.WriteLine($"与{ m_RemoteEndPoint }的连接异常，exception：{ ex }");
                        }
                    }
                    catch (ObjectDisposedException)
                    {

                    }
                }
            }
        }

        // 发送消息的回调
        private void SendCallback(IAsyncResult asyncResult)
        {
            lock (m_Lock)
            {
                if (m_Socket == null)
                {
                    return;
                }
                try
                {
                    var count = m_Socket.EndSend(asyncResult);
                    if (count == m_ToBeSentMsgQueue.Peek().Length)
                    {
                        //消息发送完整
                        m_ToBeSentMsgQueue.Dequeue();
                        if (m_ToBeSentMsgQueue.Count > 0)
                        {
                            var msg = m_ToBeSentMsgQueue.Peek();
                            m_Socket.BeginSend(msg, 0, msg.Length, SocketFlags.None, SendCallback, null);
                        }
                    }
                    else if (count >= 0)
                    {
                        //消息未发送完整
                        var msg = m_ToBeSentMsgQueue.Dequeue();
                        var leftMsg = new byte[msg.Length - count];//消息的剩余未发送的字节
                        Array.Copy(msg, count, leftMsg, 0, leftMsg.Length);
                        m_ToBeSentMsgQueue.Enqueue(leftMsg);
                        m_Socket.BeginSend(leftMsg, 0, leftMsg.Length, SocketFlags.None, SendCallback, null);
                    }
                    else
                    {
                        //发送失败，重新发送
                        var msg = m_ToBeSentMsgQueue.Peek();
                        m_Socket.BeginSend(msg, 0, msg.Length, SocketFlags.None, SendCallback, null);
                    }
                }
                catch (SocketException ex)
                {
                    try
                    {
                        m_Socket.Shutdown(SocketShutdown.Both);
                    }
                    finally
                    {
                        m_Socket.Close();
                        m_Socket = null;
                        ClientSocketMgr.Instance.RemoveClientSocket(this);
                        Console.WriteLine($"与{ m_RemoteEndPoint }的连接异常，exception：{ ex }");
                    }
                }
                catch (ObjectDisposedException)
                {

                }
            }
        }
        #endregion
    }
}