using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace NetGameServer {
    class ServNet {
        //监听嵌套字
        public Socket listenfd;
        //客户端连接
        public Conn[] conns;
        //最大连接数
        public int maxConn = 50;
        //单例
        public static ServNet instance;

        //主定时器
        System.Timers.Timer timer = new System.Timers.Timer(1000);
        //心跳时间
        public long heartBeatTime = 10;

        //协议
        public ProtocolBase proto;

        public ServNet() {
            instance = this;
        }

        //获取连接池索引 返回负数表示获取失败
        public int NewIndex() {
            if (conns == null) {
                return -1;
            }
            for (int i = 0; i < conns.Length; i++) {
                if (conns[i] == null) {
                    conns[i] = new Conn();
                    return i;
                }
                else if (conns[i].isUse == false) {
                    return i;
                }
            }
            return -1;
        }

        //开启服务器
        public void Start(string host, int port) {
            //定时器
            timer.Elapsed += new System.Timers.ElapsedEventHandler(HandlerMainTimer);
            timer.AutoReset = false;
            timer.Enabled = true;
            //连接池
            conns = new Conn[maxConn];
            for (int i = 0; i < maxConn; i++) {
                conns[i] = new Conn();
            }
            //Socket
            listenfd = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //Bind
            IPAddress ipAdr = IPAddress.Parse(host);
            IPEndPoint ipEp = new IPEndPoint(ipAdr, port);
            listenfd.Bind(ipEp);
            listenfd.Listen(maxConn);
            //Accept
            listenfd.BeginAccept(AcceptCb, null);
            Console.WriteLine("{服务器}启动成功!");
        }

        private void HandlerMainTimer(object sender, ElapsedEventArgs e) {
            HeartBeat();
            timer.Start();
        }
        public void HeartBeat() {
            Console.WriteLine("[主定时器执行]");
            long timeNow = Sys.GetTimeStamp();
            for (int i = 0;i < conns.Length;i++) {
                Conn conn = conns[i];
                if (conn == null) {
                    continue;
                }
                if (!conn.isUse) {
                    continue;
                }

                if (conn.lastTickTime < timeNow - heartBeatTime) {
                    Console.WriteLine("[心跳引起断开连接]" + conn.GetAddress());
                    lock (conn) {
                        conn.Close();
                    }
                }
            }
        }

        private void AcceptCb(IAsyncResult ar) {
            try {
                Socket socket = listenfd.EndAccept(ar);
                int index = NewIndex();

                if (index < 0) {
                    socket.Close();
                    Console.WriteLine("[警告]连接已满!");
                }
                else {
                    Conn conn = conns[index];
                    conn.Init(socket);
                    string adr = conn.GetAddress();
                    Console.WriteLine("客户端连接[" + adr + "] coon池ID: " + index);
                    conn.socket.BeginReceive(conn.readBuff, conn.buffCount, conn.BuffRemain(), SocketFlags.None, ReceiveCb, conn);
                    listenfd.BeginAccept(AcceptCb, null);
                }
            }
            catch (Exception e) {

                Console.WriteLine("AcceptCB失败:" + e.Message);
            }
        }

        public void Close() {
            for (int i = 0; i < conns.Length; i++) {
                Conn conn = conns[i];
                if (conn == null) {
                    continue;
                }
                if (!conn.isUse) {
                    continue;
                }
                lock (conn) {
                    conn.Close();
                }
            }
        }

        private void ReceiveCb(IAsyncResult ar) {
            Conn conn = (Conn)ar.AsyncState;
            lock (conn) {
                try {
                    int count = conn.socket.EndReceive(ar);
                    //关闭信号
                    if (count <= 0) {
                        Console.Write("收到 [" + conn.GetAddress() + "] 断开连接!");
                        conn.Close();
                        return;
                    }
                    conn.buffCount += count;
                    ProcessData(conn);
                    //继续接收
                    conn.socket.BeginReceive(conn.readBuff, conn.buffCount, conn.BuffRemain(), SocketFlags.None, ReceiveCb, conn);
                }
                catch (Exception e) {
                    Console.WriteLine("收到 [" + conn.GetAddress() + "] 断开连接");
                    conn.Close();
                }
            }
        }

        private void ProcessData(Conn conn) {
            if (conn.buffCount < sizeof(Int32)) {
                return;
            }
            //消息长度
            Array.Copy(conn.readBuff, conn.lenByte, sizeof(Int32));
            conn.msgLength = BitConverter.ToInt32(conn.lenByte, 0);
            if (conn.buffCount < conn.msgLength + sizeof(Int32)) {
                return;
            }
            //处理消息
            string str = System.Text.Encoding.UTF8.GetString(conn.readBuff, sizeof(Int32), conn.msgLength);
            Console.WriteLine("收到消息 [" + conn.GetAddress() + "] " + str);
            if (str == "HeartBeat") {
                conn.lastTickTime = Sys.GetTimeStamp();
            }
            Send(conn, str);
            //清除已处理的消息
            int count = conn.buffCount - conn.msgLength - sizeof(Int32);
            Array.Copy(conn.readBuff, sizeof(Int32) + conn.msgLength, conn.readBuff, 0, count);
            conn.buffCount = count;
            if (conn.buffCount > 0) {
                ProcessData(conn);
            }
        }

        private void Send(Conn conn, string str) {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(str);
            byte[] length = BitConverter.GetBytes(bytes.Length);
            byte[] sendbuff = length.Concat(bytes).ToArray();
            try {
                conn.socket.BeginSend(sendbuff, 0, sendbuff.Length, SocketFlags.None, null, null);
            }
            catch (Exception e) {
                Console.WriteLine("[发送消息]" + conn.GetAddress() + ":" + e.Message);
            }
        }
    }
}

