using System;
using System.Net.Sockets;

namespace MMORPG_GameServer
{
    /// <summary>
    /// 客户端连接对象 负责和客户端进行通讯
    /// </summary>
    public class ClientSocket
    {
        /// <summary>
        /// 客户端Socket
        /// </summary>
        private Socket m_Socket;

        /// <summary>
        /// 接收数据包的字节数组缓冲区
        /// </summary>
        private byte[] m_ReceiveBuffer = new byte[10240];

        /// <summary>
        /// 接收数据包的缓冲数据流
        /// </summary>
        private MMO_MemoryStream m_ReceiveMS = new MMO_MemoryStream();

        public ClientSocket(Socket socket)
        {
            m_Socket = socket;

            //异步接收数据
            m_Socket.BeginReceive(m_ReceiveBuffer, 0, m_ReceiveBuffer.Length, SocketFlags.None, ReceiveCallback, null);
        }

        private void ReceiveCallback(IAsyncResult asyncResult)
        {
            try
            {
                //异步方法Begin...必须调用相应的End...完成异步操作
                int len = m_Socket.EndReceive(asyncResult);

                Console.WriteLine("接收到消息，字节数：{0}", len);

                if (len > 0)
                {
                    //已经接收到数据

                    //把接收到的数据写入缓冲数据流的尾部
                    m_ReceiveMS.Position = m_ReceiveMS.Length;

                    //把指定长度的字节写入数据流
                    m_ReceiveMS.Write(m_ReceiveBuffer, 0, len);

                    if (m_ReceiveMS.Length > 2)
                    {
                        m_ReceiveMS.Position = 0;
                        while (true)
                        {
                            //数据包的头部接收完成，读取包体长度
                            ushort bodyLen = m_ReceiveMS.ReadUShort();
                            if (m_ReceiveMS.Length - m_ReceiveMS.Position >= bodyLen)
                            {
                                //包体接收完成
                                string body = m_ReceiveMS.ReadUTF8String();
                                Console.WriteLine("接收到消息：" + body);
                                if(m_ReceiveMS.Length - m_ReceiveMS.Position <= 2)
                                {
                                    //剩余不足两个字节，将剩余字节移到开头
                                    byte[] buffer = m_ReceiveMS.GetBuffer();
                                    long i = m_ReceiveMS.Position, j = 0;
                                    while (i < m_ReceiveMS.Length)
                                    {
                                        buffer[j] = buffer[i];
                                        ++i;
                                        ++j;
                                    }
                                    m_ReceiveMS.SetLength(j);
                                    break;
                                }
                            }
                            else
                            {
                                //包体不完整，等包体接收完整再解析
                                byte[] buffer = m_ReceiveMS.GetBuffer();
                                long i = m_ReceiveMS.Position, j = 0;
                                while (i < m_ReceiveMS.Length)
                                {
                                    buffer[j] = buffer[i];
                                    ++i;
                                    ++j;
                                }
                                m_ReceiveMS.SetLength(j);
                                break;
                            }
                        }
                    }

                    //继续异步接收消息
                    m_Socket.BeginReceive(m_ReceiveBuffer, 0, m_ReceiveBuffer.Length, SocketFlags.None, ReceiveCallback, null);
                }
                else
                {
                    //客户端断开连接
                    Console.WriteLine("客户端{0}断开连接", m_Socket.RemoteEndPoint.ToString());
                }
            }
            catch(Exception ex)
            {
                //连接异常
                Console.WriteLine("与{0}的连接异常，error：{1}", m_Socket.RemoteEndPoint.ToString(), ex.Message);
            }
        }
    }
}