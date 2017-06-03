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

                // 设置自己为server，udp server那边会将自己TCP server port传过来
                UdpClient udpServer = new UdpClient(50000);
                udpServer.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                udpServer.Client.Blocking = true;
                IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
                Byte[] receiveBytes;
                string ipAddr = "0.0.0.0";

                try
                {
                    receiveBytes = udpServer.Receive(ref RemoteIpEndPoint);
                    string returnData = Encoding.ASCII.GetString(receiveBytes);
                    Console.WriteLine(RemoteIpEndPoint.Address.ToString() + ":" + returnData.ToString());

                    ipAddr = RemoteIpEndPoint.Address.ToString();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    continue;
                }

                // 使用完，一定要注意close掉，要不然会报地址不能重复使用的异常
                udpServer.Close();

                UdpClient udpClient = new UdpClient();
                udpClient.Connect(ipAddr, 50001);
                Byte[] senddata = Encoding.ASCII.GetBytes(macAddr);
                udpClient.Send(senddata, senddata.Length);
                udpClient.Close();

                Thread.Sleep(1000);

                Console.WriteLine(count++);
            }
        }
    }
}  
