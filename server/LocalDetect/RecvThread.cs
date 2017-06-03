using System;
using System.Collections.Generic;
using System.Text;
using System.Threading; 
using System.Net.Sockets;
using System.Net;
using System.Collections;

namespace LocalDetect
{
    class RecvThread 
    {
        Socket clientSocket;
        MACIP macip;
        ArrayList macIpAL;
        private static byte[] result = new byte[1024];

        private Thread _thread;

        public RecvThread(Socket clientSocket, ArrayList macIpAL) { 
            this.clientSocket = clientSocket;
            this.macIpAL = macIpAL;

            _thread = new Thread(new ThreadStart(RunThread));

        }

        // Thread methods / properties
        public void Start() { _thread.Start(); }
        public void Join() { _thread.Join(); }
        public bool IsAlive { get { return _thread.IsAlive; } }

        
        /// <summary>  
        /// 1. 获取IP；
        /// 2. 获取IP对应的MACIP对象，如果不在范围，直接返回，线程结束；
        /// 3. 获取客户端的数据，并更新MACIP对应的心跳时间；
        /// 4. 连接异常或者断开之后关闭socket；
        /// 5. 删除Arraylist中的对象。
        /// </summary>  
        public void RunThread() {

            string ip = ((IPEndPoint)(clientSocket.RemoteEndPoint)).Address.ToString();

            Console.WriteLine("RecvThread ip : {0}", ip);
            int index = 0;
            for (; index < macIpAL.Count; index ++)
                if (((MACIP)macIpAL[index]).IP == ip)
                {
                    macip = ((MACIP)macIpAL[index]);
                    macip.ClientSocket = this.clientSocket;
                    Console.WriteLine("RecvThread ClientSocket value {0}", this.clientSocket == null);
                    Console.WriteLine("RecvThread add ClientSocket {0}", macip.ClientSocket);
                    break;
                }

            if (index >= macIpAL.Count)
            {
                // clientSocket.Shutdown(SocketShutdown.Both);
                clientSocket.Close();
                return;
            }

            while (LocalDetect.localDetect.running)
            {
                try
                {
                    //通过clientSocket接收数据  
                    int receiveNumber = clientSocket.Receive(result);
                    if (receiveNumber == 0) { break; }

                    macip.LeftTime = 4;

                    Console.WriteLine("接收客户端{0}消息{1}", clientSocket.RemoteEndPoint.ToString(), Encoding.ASCII.GetString(result, 0, receiveNumber));
                }
                catch { break; }
            }

            try {
                Console.WriteLine("recvThread close socket.");
                macip.ClientSocket.Shutdown(SocketShutdown.Both);
                macip.ClientSocket.Close();
            }
            catch { }

            // 确认存在之后再去删除
            if (macIpAL.Contains(macip))
                macIpAL.Remove(macip);

            LocalDetect.localDetect.refreshListViewFlag = true;

        }
    }
}
