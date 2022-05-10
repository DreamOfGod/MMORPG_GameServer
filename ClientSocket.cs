using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace MMORPG_GameServer
{
    /// <summary>
    /// 客户端连接对象 负责和客户端进行通讯
    /// </summary>
    public class ClientSocket
    {
        public Role Role;

        #region socket对象
        //客户端Socket
        private Socket m_Socket;

        //远程终端
        private IPEndPoint m_RemoteEndPoint;
        #endregion

        #region 发送相关
        //同步发送线程的事件
        private AutoResetEvent m_CheckToBeSentMsgEvent = new AutoResetEvent(false);

        //待发送的消息队列
        private Queue<byte[]> m_ToBeSentMsgQueue = new Queue<byte[]>();

        // 进行压缩的长度下限
        private const int COMPRESS_LENGTH = 200;
        #endregion

        #region 接收相关
        // 接收数据包的字节数组缓冲区
        private byte[] m_ReceiveBuffer = new byte[2048];

        // 接收数据包的缓冲数据流
        private MMO_MemoryStream m_ReceiveMS = new MMO_MemoryStream();
        #endregion

        #region 构造函数
        public ClientSocket(Socket socket)
        {
            m_Socket = socket;
            m_RemoteEndPoint = (IPEndPoint)socket.RemoteEndPoint;
            ThreadPool.QueueUserWorkItem(SendThreadMain, null);
            BeginReceive();
        }
        #endregion

        #region 发送线程
        //发送线程的执行入口
        private void SendThreadMain(object state)
        {
            while (true)
            {
                lock(m_ToBeSentMsgQueue)
                {
                    if (m_ToBeSentMsgQueue.Count > 0)
                    {
                        try
                        {
                            byte[] msg = m_ToBeSentMsgQueue.Peek();
                            m_Socket.BeginSend(msg, 0, msg.Length, SocketFlags.None, SendCallback, null);
                        }
                        catch (SocketException ex)
                        {
                            Console.WriteLine($"与{ m_RemoteEndPoint }的连接异常，exception：{ ex }");
                            try
                            {
                                m_Socket.Shutdown(SocketShutdown.Both);
                            }
                            finally
                            {
                                m_Socket.Close();
                            }
                            m_ToBeSentMsgQueue.Clear();
                        }
                        catch (ObjectDisposedException)
                        {

                        }
                    }
                }
                m_CheckToBeSentMsgEvent.WaitOne();
            }
        }

        // 发送消息的回调
        private void SendCallback(IAsyncResult asyncResult)
        {
            try
            {
                int count = m_Socket.EndSend(asyncResult);
                if (count == m_ToBeSentMsgQueue.Peek().Length)
                {
                    //消息发送完整
                    m_ToBeSentMsgQueue.Dequeue();
                }
                else if (count >= 0)
                {
                    //消息未发送完整
                    byte[] msg = m_ToBeSentMsgQueue.Dequeue();
                    byte[] restMsg = new byte[msg.Length - count];//消息的剩余未发送的字节
                    Array.Copy(msg, count, restMsg, 0, restMsg.Length);
                    m_ToBeSentMsgQueue.Enqueue(restMsg);
                }
                m_CheckToBeSentMsgEvent.Set();
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"与{ m_RemoteEndPoint }的连接异常，exception：{ ex }");
                try
                {
                    m_Socket.Shutdown(SocketShutdown.Both);
                }
                finally
                {
                    m_Socket.Close();
                }
                m_ToBeSentMsgQueue.Clear();
            }
            catch (ObjectDisposedException)
            {

            }
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
                }
                Console.WriteLine($"与{ m_RemoteEndPoint }的连接异常，exception：{ ex }");
            }
        }

        // 接收数据的回调
        private void ReceiveCallback(IAsyncResult asyncResult)
        {
            int count;
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
                }
                Console.WriteLine($"与{ m_RemoteEndPoint }的连接异常，exception：{ ex }");
                return;
            }
            catch (ObjectDisposedException)
            {
                return;
            }

            if (count > 0)
            {
                //已经接收到数据

                //把接收的字节写入字节流的尾部
                m_ReceiveMS.Write(m_ReceiveBuffer, 0, count);

                //字节流长度达到2个字节，至少消息的数据包长度接收完成
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
                            //压缩标志
                            bool compressed = m_ReceiveMS.ReadBool();
                            //crc16校验码
                            ushort crc16 = m_ReceiveMS.ReadUShort();
                            //数据包
                            byte[] content = new byte[contentCount - 3];
                            m_ReceiveMS.Read(content, 0, content.Length);
                            //crc16校验
                            if (Crc16.CalculateCrc16(content) != crc16)
                            {

                            }
                            //数据包异或
                            SecurityUtil.XOR(content);
                            //解压
                            if (compressed)
                            {
                                content = ZlibHelper.DeCompressBytes(content);
                            }

                            MMO_MemoryStream ms = new MMO_MemoryStream(content);
                            //协议ID
                            ushort protoCode = ms.ReadUShort();
                            //协议内容
                            byte[] protoContent = new byte[contentCount - 2];
                            ms.Read(protoContent, 0, protoContent.Length);
                            //派发协议消息
                            EventDispatcher.Instance.Dispatch(protoCode, protoContent, Role);

                            long leftCount = m_ReceiveMS.Length - m_ReceiveMS.Position;
                            if (leftCount < 2)
                            {
                                //剩余不到2个字节，将剩余字节移到字节流开头，等内容接收完整再解析
                                byte[] buffer = m_ReceiveMS.GetBuffer();
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
                            byte[] buffer = m_ReceiveMS.GetBuffer();
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

                BeginReceive();
            }
            else
            {
                //客户端断开连接
                try
                {
                    m_Socket.Shutdown(SocketShutdown.Both);
                }
                finally
                {
                    m_Socket.Close();
                }
                Console.WriteLine($"客户端{ m_RemoteEndPoint }断开连接");
            }
        }
        #endregion

        #region 异步发送
        // 封装数据包
        private byte[] MakeMsg(byte[] data)
        {
            //消息格式：数据包长度(ushort)|压缩标志(bool)|crc16(ushort)|先压缩后异或的数据包
            MMO_MemoryStream ms = new MMO_MemoryStream();
            if (data.Length > COMPRESS_LENGTH)
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
            lock (m_ToBeSentMsgQueue)
            {
                m_ToBeSentMsgQueue.Enqueue(MakeMsg(data));
                if (m_ToBeSentMsgQueue.Count == 1)
                {
                    m_CheckToBeSentMsgEvent.Set();
                }
            }
        }
        #endregion
    }
}