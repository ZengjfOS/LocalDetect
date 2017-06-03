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

        public static bool running = false;
        public static bool refreshListViewFlag = false;
        static Thread udpBroadcastSendThread;
        static Thread udpCommunicationThread;
        private static long broadcastSendIndex = 0;
        private static object threadLock = new object();

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

        // 将MAC在ListView中显示
        private void refreshListView()
        {
            Action<String> AsyncUIDelegate1 = delegate(string n) { 

                // 清空ListView
                dataLV.Items.Clear();

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

                udpBroadcastSendThread = new Thread(udpBroadcastSend);
                udpBroadcastSendThread.Start();

                udpCommunicationThread = new Thread(udpGetMACIPAndSendCheck);
                udpCommunicationThread.Start();

                Thread refreshLV = new Thread(refreshMacIpListview);
                refreshLV.Start();

                start.Text = "Stop";
            }
            else
            {
                Thread stopThread = new Thread(stopAllThread);
                stopThread.Start();

                start.Text = "Stopping";
                start.Enabled = false;
            }
        }

        private void stopAllThread()
        {
            // 等待线程自动退出
            running = false;
            Thread.Sleep(3000);

            // 让线程去更新UI界面
            Console.WriteLine("zengjf {0}", macIpAL.Count);
            macIpAL.Clear();
            refreshListView();

            // 强制等待，防止端口复用时报错
            Thread.Sleep(3000);

            Action<String> AsyncUIDelegate1 = delegate(string n) { 
                start.Text = "Start";
                start.Enabled = true;
            };
            dataLV.Invoke(AsyncUIDelegate1, "");
        }

        private void udpBroadcastSend()
        {
            UdpClient udpClient = new UdpClient();
            udpClient.Connect("255.255.255.255", 50000);

            while (true)
            {
                try
                {
                    broadcastSendIndex++;
                    // 发送一个索引，这个索引目前并没有被使用
                    Byte[] senddata = Encoding.ASCII.GetBytes("" + broadcastSendIndex);
                    udpClient.Send(senddata, senddata.Length);
                    Console.WriteLine(broadcastSendIndex);
                }
                catch
                {
                    udpClient.Close();
                }

                // 当按下暂停发送的时候，要退出
                if (running == false)
                    break;

                Thread.Sleep(1000);

            }
        }

        private void udpGetMACIPAndSendCheck()
        {
            UdpClient udpServer = new UdpClient(50001);
            udpServer.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            udpServer.Client.ReceiveTimeout = 1000;

            while (true)
            {
                IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
                Byte[] receiveBytes = { };

                if (running == false)
                    break;

                // 这里使用超时退出一次，主要是为了解决按下Stop的时候需要退出，需要杀死线程导致的。
                try { 
                    receiveBytes = udpServer.Receive(ref RemoteIpEndPoint); 
                }
                catch
                {
                    continue;
                }

                Console.WriteLine("udp received.");
                
                string mac = Encoding.ASCII.GetString(receiveBytes);
                string ip = RemoteIpEndPoint.Address.ToString();

                MACIP macip = new MACIP(mac, ip);

                // 检查IP是否已经存在ArrayList中，删除，并重新添加，就没有必要添加到其中

                lock (threadLock)
                {
                    int count = 0;
                    for (; count < macIpAL.Count; count++)
                        if (((MACIP)macIpAL[count]).IP == ip)
                            break;
                    if (macIpAL.Count > 0 && count < macIpAL.Count)
                    {
                        ((MACIP)(macIpAL[count])).LeftTime = MACIP.LEFT_TIME;
                    }
                    else
                    {
                        macIpAL.Add(macip);
                    }
                }

                // 让线程去更新UI界面
                refreshListViewFlag = true;
                Console.WriteLine("lv refresh. macIpAL count: {0}", macIpAL.Count);

            }
        }

        private void refreshMacIpListview()
        {
            while (true)
            {
                if (running == false)
                    break;

                // 需要刷新的时候才刷新
                if (refreshListViewFlag)
                {
                    refreshListView();
                    refreshListViewFlag = false;
                }

                // 当timeleft到了0，说明该设备掉线了
                ArrayList copyOfMacIPAL = new ArrayList();
                foreach (MACIP macip in macIpAL)
                {
                    if (macip.LeftTime == 0) { copyOfMacIPAL.Add(macip); }

                    macip.LeftTime--;
                }

                // 其他的地方可能在往macIpAL中添加东西，这里又在删除，要保证线程安全
                lock (threadLock)
                {
                    foreach (MACIP macip in copyOfMacIPAL)
                    {
                        if (macIpAL.Contains(macip))
                            macIpAL.Remove(macip);

                        refreshListViewFlag = true;
                    }
                }

                Console.WriteLine("refresh {0}", macIpAL.Count);

                Thread.Sleep(1000);
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
