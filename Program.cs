using MMORPG_GameServer.Controller;
using System;
using System.Net;
using System.Net.Sockets;

namespace MMORPG_GameServer
{
    class Program
    {
        private const string m_ServerIP = "127.0.0.1";
        private const int m_Port = 1011;

        private static Socket m_Socket;

        static void Main(string[] args)
        {
            InitController();
            InitSocket();
        }

        private static void InitController()
        {
            RoleController.Instance.Init();
        }

        private static void InitSocket()
        {
            //实例化socket，用来监听连接请求
            m_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            //向操作系统申请一个可用的ip和端口号用来通讯
            m_Socket.Bind(new IPEndPoint(IPAddress.Parse(m_ServerIP), m_Port));

            //设置最多3000个排队连接请求
            m_Socket.Listen(3000);

            Console.WriteLine($"启动socket监听{ m_Socket.LocalEndPoint }成功");

            //监听socket连接请求
            while (true)
            {
                //接收客户端请求
                Socket socket = m_Socket.Accept();

                Console.WriteLine($"客户端{ socket.RemoteEndPoint }已经连接");

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
