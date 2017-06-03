using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using System.Net.Sockets;
using System.Net;
using System.Threading; 

namespace LocalDetect
{
    public partial class localDetect : Form
    {
        // Is ArrayList/List a thread safe collection? If not how would you make it thread safe?
        //     http://stackoverflow.com/questions/25968967/is-arraylist-list-a-thread-safe-collection-if-not-how-would-you-make-it-thread
        ArrayList macIpAL = ArrayList.Synchronized(new ArrayList());

        private static int UDPPort;
        private static int TCPPort;
        private static Socket serverSocket;
        public static bool running = false;
        public static bool refreshListViewFlag = false;
        static UdpClient udpServer;
        static UdpClient udpClient;
        static Thread udpCommunication;

        public localDetect()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            start.Focus(); 
            // 设置Icon
            this.Icon = global::LocalDetect.Properties.Resources.LocalDetect;

            // 初始化显示表格
            initMacIPListview();

            // 用于开启Console，方便调试。
            // AllocConsole();
        }

        private void initMacIPListview()
        {
            dataLV.GridLines = true;     // 表格是否显示网格线
            dataLV.FullRowSelect = true; // 是否选中整行

            dataLV.View = View.Details;  // 设置显示方式
            dataLV.Scrollable = true;    // 是否自动显示滚动条
            dataLV.MultiSelect = false;  // 是否可以选择多行

            // 添加表头（列）
            // C# Listview 第一列不能居中
            //     http://www.cnblogs.com/zengjfgit/p/6144253.html
            dataLV.Columns.Add("NONE", 0, HorizontalAlignment.Center);
            dataLV.Columns.Add("NO.", 60, HorizontalAlignment.Center);
            dataLV.Columns.Add("MAC", 150, HorizontalAlignment.Center);
            dataLV.Columns.Add("IP", 150, HorizontalAlignment.Center);

            dataLV.Columns[0].TextAlign= HorizontalAlignment.Center;

            refreshListView();
        }

        // 从数据库中获取所有的MAC，并将MAC在ListView中显示
        private void refreshListView()
        {
            Action<String> AsyncUIDelegate1 = delegate(string n) { 

                // 清空ListView
                dataLV.Items.Clear();

                /*
                int count = 1;
                foreach (MACIP macip in macIpAL)
                {
                    ListViewItem item = new ListViewItem();
                    item.SubItems.Clear();

                    item.SubItems[0].Text = "" + 1;
                    item.SubItems.Add("" + (count++));
                    item.SubItems.Add(macip.MAC);
                    item.SubItems.Add(macip.IP);

                    dataLV.Items.Add(item);
                }
                */

                // 这里很容易导致：集合已修改；可能无法执行枚举操作
                // 所以这里换成macIpAL.ToArray()进行数据操作
                int count = 1;
                foreach (MACIP macip in macIpAL.ToArray())
                {
                    ListViewItem item = new ListViewItem();
                    item.SubItems.Clear();

                    item.SubItems[0].Text = "" + 1;
                    item.SubItems.Add("" + (count++));
                    item.SubItems.Add(macip.MAC);
                    item.SubItems.Add(macip.IP);

                    dataLV.Items.Add(item);
                }

                // 选中listview第一行，这里会造成自动调用listview的SelectedIndexChanged事件函数；
                // 于是设置当前MAC、QRCode这事情都在dataLV_SelectedIndexChanged处理了。
                // initCurrentMACLable()也就没啥用了。
                if (dataLV.Items.Count > 0)
                {
                    dataLV.Items[dataLV.Items.Count - 1].Selected = true;
                    dataLV.Select();
                }
            };
            dataLV.Invoke(AsyncUIDelegate1, "");
        }

        private void start_Click(object sender, EventArgs e)
        {
            if (start.Text == "Start")
            {
                running = true;

                udpCommunication = new Thread(udpGetMACIPAndSendCheck);
                udpCommunication.Start();

                Thread refreshLV = new Thread(refreshMacIpListview);
                refreshLV.Start();

                //服务器IP地址  
                IPAddress ip = IPAddress.Parse("0.0.0.0");
                serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                serverSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                TCPPort = Int32.Parse(tcpPortValue.Text);
                serverSocket.Bind(new IPEndPoint(ip, TCPPort));         //绑定IP地址：端口  
                serverSocket.Listen(20);                                //设定最多20个排队连接请求  
                Console.WriteLine("启动监听{0}成功, port: {1}", serverSocket.LocalEndPoint.ToString(), TCPPort);

                //通过Clientsoket发送数据  
                Thread myThread = new Thread(ListenClientConnect);
                myThread.Start();

                start.Text = "Exit";
            }
            else
            {
                running = false;
                Thread.Sleep(3000);

                Console.WriteLine("zengjf {0}", macIpAL.Count);
                foreach (MACIP macip in macIpAL)
                {
                            Console.WriteLine("zengjf");
                    // 断开socket连接
                    if (macip.ClientSocket != null)
                    {
                        try
                        {
                            Console.WriteLine("zengjf");
                            // 超时，关闭连接
                            macip.ClientSocket.Shutdown(SocketShutdown.Both);
                            macip.ClientSocket.Disconnect(true);
                            macip.ClientSocket.Close();
                        }
                        catch { }
                    }
                }
                Console.WriteLine("zengjf {0}", macIpAL.Count);
                // 让线程去更新UI界面
                macIpAL.Clear();
                refreshListView();

                udpServer.Close();

                udpCommunication.Interrupt();
                if (!udpCommunication.Join(2000))
                { 
                    // or an agreed resonable time
                    udpCommunication.Abort();
                }

                System.Environment.Exit(0);

            }
        }

        private void udpGetMACIPAndSendCheck()
        {
            UDPPort = Int32.Parse(udpPortValue.Text);

            udpServer = new UdpClient(UDPPort);
            udpServer.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            udpServer.Client.ReceiveTimeout = 5000;

            udpClient = new UdpClient();
            udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            while (true)
            {
                IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
                Byte[] receiveBytes = { };

                if (running == false)
                    break;

                // 这里使用超时退出一次，主要是为了解决按下Stop的时候需要退出，需要杀死线程导致的。
                try { receiveBytes = udpServer.Receive(ref RemoteIpEndPoint); }
                catch { continue; }

                Console.WriteLine("udp received.");
                
                string mac = Encoding.ASCII.GetString(receiveBytes);
                string ip = RemoteIpEndPoint.Address.ToString();

                // 这里需要等待一下，主要是因为client端的server需要一点时间创建server：,
                // 否者由于发送太快，server还没建立就已经发完了数据。
                System.Threading.Thread.Sleep(100);
                udpClient.Connect(RemoteIpEndPoint.Address.ToString(), UDPPort+1);
                Byte[] senddata = Encoding.ASCII.GetBytes(TCPPort.ToString());
                udpClient.Send(senddata, senddata.Length);

                Console.WriteLine("udp sended.");

                MACIP macip = new MACIP(mac, ip);

                // 检查IP是否已经存在ArrayList中，删除，并重新添加，就没有必要添加到其中
                int count = 0;
                for (; count < macIpAL.Count; count++)
                    if (((MACIP)macIpAL[count]).IP == ip)
                        break;
                if (macIpAL.Count > 0 && count < macIpAL.Count)
                {
                    try
                    {
                        ((MACIP)macIpAL[count]).ClientSocket.Close();
                    } catch { }

                    macIpAL.RemoveAt(count);
                }
                macIpAL.Add(macip);

                // 让线程去更新UI界面
                refreshListViewFlag = true;
                Console.WriteLine("lv refresh. macIpAL count: {0}", macIpAL.Count);

            }
        }

        private void refreshMacIpListview()
        {
            while (true)
            {
                // 需要刷新的时候才刷新
                if (refreshListViewFlag)
                {
                    refreshListView();
                    refreshListViewFlag = false;
                }

                // 当timeleft到了0，说明应该
                ArrayList copyOfMacIPAL = new ArrayList();
                foreach (MACIP macip in macIpAL)
                {
                    if (macip.LeftTime == 0) { copyOfMacIPAL.Add(macip); }

                    macip.LeftTime--;
                }
                foreach (MACIP macip in copyOfMacIPAL)
                {
                    if (macIpAL.Contains(macip))
                        macIpAL.Remove(macip);

                    refreshListViewFlag = true;

                    // 断开socket连接
                    if (macip.ClientSocket != null)
                    {
                        try
                        {
                            // 超时，关闭连接
                            macip.ClientSocket.Shutdown(SocketShutdown.Both);
                            macip.ClientSocket.Disconnect(true);
                        }
                        catch { }
                    }
                }

                Console.WriteLine("refresh {0}", macIpAL.Count);
                if (macIpAL.Count != 0)
                {
                    Console.WriteLine("network {0}", ((MACIP)macIpAL[0]).ClientSocket);
                }

                Thread.Sleep(1000);
            }
        }

        /// <summary>  
        /// 监听客户端连接  
        /// </summary>  
        private void ListenClientConnect()
        {
            while (true)
            {
                try
                {
                    Socket clientSocket = serverSocket.Accept();
                    RecvThread recvThread = new RecvThread(clientSocket, macIpAL);
                    recvThread.Start();
                    Console.WriteLine("ListenClient accept once.");

                    if (running == false)
                        break;
                }
                catch
                {
                    Console.WriteLine("zengjf out of the ListenClient .");
                    break;
                }
            }
        }

        // 用于开启Console，方便调试。
        /*
        [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true)]
        [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
        static extern bool AllocConsole();

        [System.Runtime.InteropServices.DllImport("Kernel32")]
        public static extern void FreeConsole();
        */

    }
}
