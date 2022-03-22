using System;
using System.Net.Sockets;

namespace MMORPG_GameServer
{
    /// <summary>
    /// 客户端连接对象 负责和客户端进行通讯
    /// </summary>
    public class ClientSocket
    {
        public Role Role;

        /// <summary>
        /// 客户端Socket
        /// </summary>
        private Socket m_Socket;

        /// <summary>
        /// 接收数据包的字节数组缓冲区
        /// </summary>
        private byte[] m_ReceiveBuffer = new byte[2048];

        /// <summary>
        /// 接收数据包的缓冲数据流
        /// </summary>
        private MMO_MemoryStream m_ReceiveMS = new MMO_MemoryStream();

        public ClientSocket(Socket socket)
        {
            m_Socket = socket;

            MMO_MemoryStream ms = new MMO_MemoryStream();
            ms.WriteUTF8String(string.Format("欢迎登陆服务器{0}", DateTime.Now.ToString()));
            SendMsg(ms.ToArray());

            //异步接收数据
            m_Socket.BeginReceive(m_ReceiveBuffer, 0, m_ReceiveBuffer.Length, SocketFlags.None, ReceiveCallback, null);
        }

        /// <summary>
        /// 接收数据的回调
        /// </summary>
        /// <param name="asyncResult"></param>
        private void ReceiveCallback(IAsyncResult asyncResult)
        {
            int count;
            try
            {
                //异步方法Begin...必须调用相应的End...完成异步操作
                count = m_Socket.EndReceive(asyncResult);
            }
            catch (Exception ex)
            {
                //连接异常
                Console.WriteLine("与{0}的连接异常，error：{1}", m_Socket.RemoteEndPoint.ToString(), ex.Message);
                return;
            }

            if (count > 0)
            {
                //已经接收到数据

                //把接收的字节写入字节流的尾部
                m_ReceiveMS.Write(m_ReceiveBuffer, 0, count);

                //字节流长度达到2个字节，至少一个消息的头部接收完成，因为客户端消息的头部是ushort类型
                if (m_ReceiveMS.Length >= 2)
                {
                    m_ReceiveMS.Position = 0;
                    while (true)
                    {
                        //读取内容的字节数
                        ushort contentCount = m_ReceiveMS.ReadUShort();
                        if (m_ReceiveMS.Length - m_ReceiveMS.Position >= contentCount)
                        {
                            //消息的内容已经接收完成
                            byte[] content = new byte[contentCount];
                            m_ReceiveMS.Read(content, 0, contentCount);

                            MMO_MemoryStream ms = new MMO_MemoryStream(content);
                            ushort protoCode = ms.ReadUShort();
                            byte[] protoContent = new byte[contentCount - 2];
                            ms.Read(protoContent, 0, protoContent.Length);
                            EventDispatcher.Instance.Dispatch(protoCode, protoContent, Role);

                            long leftCount = m_ReceiveMS.Length - m_ReceiveMS.Position;
                            if (leftCount == 0)
                            {
                                //没有剩余字节，指针和长度都置为0
                                m_ReceiveMS.Position = 0;
                                m_ReceiveMS.SetLength(0);
                                break;
                            }
                            else if(leftCount == 1)
                            {
                                //剩余1个字节，移到字节流开头，指针和长度都置为1
                                byte[] buffer = m_ReceiveMS.GetBuffer();
                                buffer[0] = buffer[m_ReceiveMS.Position];
                                m_ReceiveMS.Position = 1;
                                m_ReceiveMS.SetLength(1);
                                break;
                            }
                        }
                        else
                        {
                            //消息内容接收不完整，将剩余字节移到字节流开头，等内容接收完整再解析
                            byte[] buffer = m_ReceiveMS.GetBuffer();
                            long i = 0, j = m_ReceiveMS.Position;
                            while (i < m_ReceiveMS.Length)
                            {
                                buffer[i] = buffer[j];
                                ++i;
                                ++j;
                            }
                            m_ReceiveMS.SetLength(j);
                            break;
                        }
                    }
                }

                //继续异步接收数据
                m_Socket.BeginReceive(m_ReceiveBuffer, 0, m_ReceiveBuffer.Length, SocketFlags.None, ReceiveCallback, null);
            }
            else
            {
                //客户端断开连接
                Console.WriteLine("客户端{0}断开连接", m_Socket.RemoteEndPoint.ToString());
            }
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="data"></param>
        public void SendMsg(byte[] data)
        {
            MMO_MemoryStream ms = new MMO_MemoryStream();
            ms.WriteUShort((ushort)data.Length);
            ms.Write(data, 0, data.Length);
            byte[] msg = ms.ToArray();
            m_Socket.BeginSend(msg, 0, msg.Length, SocketFlags.None, SendCallback, null);
        }

        /// <summary>
        /// 发送消息的回调
        /// </summary>
        /// <param name="asyncResult"></param>
        private void SendCallback(IAsyncResult asyncResult)
        {
            m_Socket.EndSend(asyncResult);
        }
    }
}