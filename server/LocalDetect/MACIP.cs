using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace LocalDetect
{
    class MACIP
    {
        private string mac = "00:11:22:33:44:55";
        private string ip = "127.0.0.1";
        private int leftTime = 4;
        private Socket clientSocket = null;

        public MACIP(string mac, string ip, int leftTime = 4)
        {
            this.mac = mac;
            this.ip = ip;
            this.leftTime = leftTime;
        }

        public Socket ClientSocket
        {
            get
            {
                return clientSocket;
            }
            set
            {
                clientSocket = value;
            }
        }

        public string MAC
        {
            get
            {
                return mac;
            }
            set
            {
                mac = value;
            }
        }

        public string IP
        {
            get
            {
                return ip;
            }
            set
            {
                ip = value;
            }
        }

        public int LeftTime
        {
            get
            {
                return leftTime;
            }
            set
            {
                leftTime = value;
            }
        }

        public bool checkAndSubTime()
        {
            if (leftTime == 0)
                return false;

            leftTime--;
            return true;
        }

        public override string ToString()
        {
            return "MAC = " + mac + ", IP = " + ip + ", LeftTime = " + leftTime + ", clientSocket = " + clientSocket;
        }
    }
}
