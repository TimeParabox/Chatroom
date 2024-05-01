using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Server
{
    public class TCPServer
    {
        private Socket socket = null;
        private string IP = string.Empty;
        private int Port = 0;
        private byte[] buffer = new byte[1024 * 1024 * 2];
        private Dictionary<string, Socket> clientDic = new Dictionary<string, Socket>();
        public List<string> UserList = new List<string>();

        public TCPServer(string ip, int port)
        {
            this.IP = ip;
            this.Port = port;
        }

        public TCPServer(int port)
        {
            this.IP = "0.0.0.0";
            this.Port = port;
        }

        public void SocketListen()
        {
            try
            {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPAddress address = IPAddress.Parse(IP);
                IPEndPoint endPoint = new IPEndPoint(address, Port);
                socket.Bind(endPoint);
                socket.Listen(int.MaxValue);
                Console.WriteLine("服务器已开启");
                Thread thread = new Thread(ListenClientConnect);
                thread.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void ListenClientConnect()
        {
            try
            {
                while (true)
                {
                    Socket clientSocket = socket.Accept();
                    EndPoint clientipe = clientSocket.RemoteEndPoint;
                    Console.WriteLine($"接收客户端【{clientipe}】连接...");
                    string IP = clientipe.ToString();
                    clientDic.Add(clientipe.ToString(), clientSocket);
                    Console.WriteLine($"用户【{clientipe}】上线");
                    UserList.Add(clientipe.ToString());
                    string Users = string.Empty;
                    foreach (var item in UserList)
                    {
                        Users = item + "," + Users;
                    }
                    foreach (var kvp in clientDic)
                    {
                        kvp.Value.Send(Encoding.UTF8.GetBytes($"{clientipe},进入聊天"));
                        kvp.Value.Send(Encoding.UTF8.GetBytes($"Users,{Users}"));
                    }
                    Thread thread = new Thread(ReceiveMessage);
                    thread.Start(clientSocket);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void ReceiveMessage(object socket)
        {
            try
            {
                Socket clientSocket = socket as Socket;
                while (true)
                {
                    try
                    {
                        int length = clientSocket.Receive(buffer);
                        string stringFromBytes = Encoding.UTF8.GetString(buffer, 0, length);

                        Console.WriteLine($"接收客户端消息:{stringFromBytes}");
                        EndPoint clientipe = clientSocket.RemoteEndPoint;
                        foreach (var kvp in clientDic)
                        {
                            kvp.Value.Send(Encoding.UTF8.GetBytes($"{clientipe},{stringFromBytes}"));
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        EndPoint clientipe = clientSocket.RemoteEndPoint;
                        clientDic.Remove(clientipe.ToString());

                        clientSocket.Shutdown(SocketShutdown.Both);
                        clientSocket.Close();

                        Console.WriteLine($"用户【{clientipe}】下线");

                        UserList.Remove(clientipe.ToString());
                        string Users = string.Empty;
                        foreach (var item in UserList)
                        {
                            Users = Users + "," + item;
                        }
                        foreach (var kvp in clientDic)
                        {
                            kvp.Value.Send(Encoding.UTF8.GetBytes($"Users,{Users}"));
                            kvp.Value.Send(Encoding.UTF8.GetBytes($"{clientipe},退出聊天"));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }

    internal class MainProgram
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("请输入ip地址");
            string ip = Console.ReadLine();
            Console.WriteLine("请输入端口");
            int port = int.Parse(Console.ReadLine());
            TCPServer server = new TCPServer(ip, port);
            server.SocketListen();
        }
    }
}