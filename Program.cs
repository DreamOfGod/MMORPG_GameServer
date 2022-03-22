using MMORPG_GameServer.DBModel;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace MMORPG_GameServer
{
    class Program
    {
        private const string m_ServerIP = "127.0.0.1";
        private const int m_Port = 1011;

        private static Socket m_ServerSocket;

        static void Main(string[] args)
        {
            TestDBModel.Instance.Init();

            //实例化socket，用来监听连接请求
            m_ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            //向操作系统申请一个可用的ip和端口号用来通讯
            m_ServerSocket.Bind(new IPEndPoint(IPAddress.Parse(m_ServerIP), m_Port));

            //设置最多3000个排队连接请求
            m_ServerSocket.Listen(3000);

            Console.WriteLine("启动监听{0}成功", m_ServerSocket.LocalEndPoint.ToString());

            //监听socket连接请求
            while (true)
            {
                //接收客户端请求
                Socket socket = m_ServerSocket.Accept();

                Console.WriteLine("客户端{0}已经连接", socket.RemoteEndPoint.ToString());

                ClientSocket clientSocket = new ClientSocket(socket);

                //一个角色就相当于一个客户端
                Role role = new Role(clientSocket);

                clientSocket.Role = role;

                //把角色添加到角色管理
                RoleMgr.Instance.AddRole(role);
            }
        }
    }
}
