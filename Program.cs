using MMORPG_GameServer.Controller;
using System;
using System.Net;
using System.Net.Sockets;

namespace MMORPG_GameServer
{
    class Program
    {
        //socket监听的ip
        private const string m_ServerIP = "127.0.0.1";
        //socket监听的端口号
        private const int m_Port = 1001;

        //accpet socket
        private static Socket m_AcceptSocket;

        static void Main(string[] args)
        {
            InitController();
            InitSocket();
        }

        //初始化控制器
        private static void InitController()
        {
            RoleController.Instance.Init();
        }

        //初始化accpet socket
        private static void InitSocket()
        {
            m_AcceptSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            m_AcceptSocket.Bind(new IPEndPoint(IPAddress.Parse(m_ServerIP), m_Port));
            m_AcceptSocket.Listen(3000);
            Console.WriteLine($"启动socket监听{ m_AcceptSocket.LocalEndPoint }成功");
            while (true)
            {
                Socket socket;
                try
                {
                    socket = m_AcceptSocket.Accept();
                }
                catch(SocketException ex)
                {
                    m_AcceptSocket.Close();
                    Console.WriteLine($"Accept socket发生异常，exception：{ ex }");
                    return;
                }
                Console.WriteLine($"客户端{ socket.RemoteEndPoint }已经连接");
                Role role = new Role();
                ClientSocket clientSocket = new ClientSocket(socket, role);
                ClientSocketMgr.Instance.AddClientSocket(clientSocket);
            }
        }
    }
}
