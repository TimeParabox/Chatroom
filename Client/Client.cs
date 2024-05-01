using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Client
{
    public class Client
    {
        private Socket socket = null;
        private string IP = string.Empty;
        private int Port = 0;
        private byte[] buffer = new byte[1024 * 1024 * 2];
        private Dictionary<string, Socket> clientDic = new Dictionary<string, Socket>();
        public List<string> UserList = new List<string>();

        public Client(string ip, int port)
        {
            this.IP = ip;
            this.Port = port;
        }

        public void ClientConnect(int port)
        {
            try
            {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPAddress address = IPAddress.Parse(IP);
                IPEndPoint endPoint = new IPEndPoint(address, Port);
                socket.Bind(new IPEndPoint(IPAddress.Parse("0.0.0.0"), port));
                socket.Connect(endPoint);
                Console.WriteLine("连接服务器成功");
                Thread MessageThread = new Thread(Message);
                Thread SendThread = new Thread(Send);
                MessageThread.Start();
                SendThread.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void Message()
        {
            try
            {
                while (true)
                {
                    int lengths = socket.Receive(buffer);
                    string stringFromBytes1 = Encoding.UTF8.GetString(buffer, 0, lengths);
                    Console.WriteLine(stringFromBytes1);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void Send()
        {
            while (true)
            {
                try
                {
                    string Message = Console.ReadLine();
                    string sendMessage = string.Format(Message);
                    socket.Send(Encoding.UTF8.GetBytes(sendMessage));
                    sendMessage = string.Empty;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }
    }

    internal class MainProgram
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("请输入服务器ip地址");
            string ip = Console.ReadLine();
            Console.WriteLine("请输入服务器端口");
            int serverpPort = int.Parse(Console.ReadLine());
            Console.WriteLine("请输入用户端口");
            int userPort = int.Parse(Console.ReadLine());
            Client client = new Client(ip, serverpPort);
            client.ClientConnect(userPort);
        }
    }
}