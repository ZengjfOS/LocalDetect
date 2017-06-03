using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Net.NetworkInformation; 

namespace LocalDetectTest
{
    class Program
    {
        private static byte[] result = new byte[1024];
        private static int count = 0;

        static void Main(string[] args)
        {
            while (true)
            {
                // 获取MAC地址
                string macAddr = NetTools.GetFirstMacAddress();

                // 连接udp server，这样server那边就知道client这边的IP了
                UdpClient udpClient = new UdpClient();
                udpClient.Connect("255.255.255.255", 50000);
                Byte[] senddata = Encoding.ASCII.GetBytes(macAddr);
                udpClient.Send(senddata, senddata.Length);

                // 设置自己为server，udp server那边会将自己TCP server port传过来
                UdpClient udpServer = new UdpClient(50001);
                udpServer.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                udpServer.Client.SendTimeout = 5000;
                udpServer.Client.ReceiveTimeout = 5000;
                IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
                Byte[] receiveBytes;
                string ipAddr = "0.0.0.0";
                int port = 50002;
                try
                {
                    receiveBytes = udpServer.Receive(ref RemoteIpEndPoint);
                    string returnData = Encoding.ASCII.GetString(receiveBytes);
                    Console.WriteLine(RemoteIpEndPoint.Address.ToString() + ":" + returnData.ToString());
                    ipAddr = RemoteIpEndPoint.Address.ToString();
                    port = Int32.Parse(returnData);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

                // 使用完，一定要注意close掉，要不然会报地址不能重复使用的异常
                udpClient.Close();
                udpServer.Close();

                Thread.Sleep(1000);

                //设定服务器IP地址  
                IPAddress ip = IPAddress.Parse(ipAddr);
                Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                try
                {
                    clientSocket.Connect(new IPEndPoint(ip, port)); //配置服务器IP与端口  
                    Console.WriteLine("连接服务器成功");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("连接服务器失败，请按回车键退出！");
                    Console.WriteLine(ex);
                    continue;
                }

                //通过 clientSocket 发送10帧数据  
                for (int i = 0; i < 10; i++)
                {
                    try
                    {
                        Thread.Sleep(1000);    //等待1秒钟  
                        string sendMessage = "OK";
                        if (clientSocket.Connected)
                            Console.WriteLine(clientSocket.Send(Encoding.ASCII.GetBytes(sendMessage)));

                        Console.WriteLine("向服务器发送消息{0}：{1}", i, sendMessage);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        break;
                    }
                }

                try
                {
                    clientSocket.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

                Console.WriteLine("发送完毕，按回车键退出 {0}", count++);

                Thread.Sleep(3000);
            }
        }
    }
}  
