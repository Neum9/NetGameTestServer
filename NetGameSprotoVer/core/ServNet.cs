using Sproto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace NetGameServer
{
    class ServNet
    {
        //监听嵌套字
        public Socket listenfd;
        //客户端连接
        public Conn m_conn = new Conn();
        //单例
        public static ServNet instance;

        //主定时器
        System.Timers.Timer timer = new System.Timers.Timer(1000);
        //心跳时间
        public long heartBeatTime = 180;

        // use rpc
        private SprotoRpc m_rpc;
        private SprotoRpc.RpcRequest m_rpcReq;

        public ServNet() {
            instance = this;
            m_rpc = new SprotoRpc(Protocol.Instance);
            m_rpcReq = m_rpc.Attach(Protocol.Instance);
        }


        //开启服务器
        public void Start(string host, int port) {
            //定时器
            timer.Elapsed += new System.Timers.ElapsedEventHandler(HandlerMainTimer);
            timer.AutoReset = false;
            timer.Enabled = true;

            //Socket
            listenfd = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //Bind
            IPAddress ipAdr = IPAddress.Parse(host);
            IPEndPoint ipEp = new IPEndPoint(ipAdr, port);
            listenfd.Bind(ipEp);
            listenfd.Listen(1);
            //Accept
            listenfd.BeginAccept(AcceptCb, null);
            Console.WriteLine("{服务器}启动成功!");
        }

        private void HandlerMainTimer(object sender, ElapsedEventArgs e) {
            HeartBeat();
            timer.Start();
        }
        public void HeartBeat() {
            //Console.WriteLine("[主定时器执行]");
            long timeNow = Sys.GetTimeStamp();

            if (m_conn == null) {
                return;
            }
            if (!m_conn.isUse) {
                return;
            }

            if (m_conn.lastTickTime < timeNow - heartBeatTime) {
                Console.WriteLine("[心跳引起断开连接]" + m_conn.GetAddress());
                lock (m_conn) {
                    m_conn.Close();
                }
            }


        }

        private void AcceptCb(IAsyncResult ar) {
            try {
                Socket socket = listenfd.EndAccept(ar);
                m_conn.Init(socket);
                string adr = m_conn.GetAddress();
                Console.WriteLine("客户端连接[" + adr + "]");
                m_conn.socket.BeginReceive(m_conn.readBuff, m_conn.buffCount, m_conn.BuffRemain(), SocketFlags.None, ReceiveCb, m_conn);
                //listenfd.BeginAccept(AcceptCb, null);
            } catch (Exception e) {

                Console.WriteLine("AcceptCB失败:" + e.Message);
            }
        }

        public void Close() {
            m_conn.Close();
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
                } catch (Exception e) {
                    Console.WriteLine("[ReceiveCb Error!]" + e.Message);
                    Console.WriteLine("收到 [" + conn.GetAddress() + "] 断开连接");
                    conn.Close();
                }
            }
        }

        private void ProcessData(Conn conn) {
            byte[] tempBuff = new byte[conn.buffCount];
            //包体长度
            Array.Copy(conn.readBuff, tempBuff, conn.buffCount);
            SprotoRpc.RpcInfo sInfo = m_rpc.Dispatch(tempBuff);
            HandleMsg(conn, sInfo);

            // 协议解码
            conn.buffCount = 0;
        }

        private void HandleMsg(Conn conn, SprotoRpc.RpcInfo sInfo) {
            SprotoTypeBase sp;
            if (sInfo.type == SprotoRpc.RpcType.REQUEST) {
                sp = sInfo.requestObj;
            } else if (sInfo.type == SprotoRpc.RpcType.RESPONSE) {
                sp = sInfo.responseObj;
            }
            Console.WriteLine("HandleMsg");
        }

        public void Send<T>(Conn conn, SprotoTypeBase protocol) {
            byte[] sendBuff = m_rpcReq.Invoke<T>(protocol, 1);
            try {
                conn.socket.BeginSend(sendBuff, 0, sendBuff.Length, SocketFlags.None, null, null);
            } catch (Exception e) {
                Console.WriteLine("[发送消息]" + conn.GetAddress() + ":" + e.Message);
            }
        }

        //// 广播
        //public void Broadcast(ProtocolBase protocol) {
        //    for (int i = 0; i < conns.Length; i++) {
        //        if (!conns[i].isUse) {
        //            continue;
        //        }
        //        if (conns[i].player == null) {
        //            continue;
        //        }
        //        Send(conns[i], protocol);
        //    }
        //}

        ////打印信息
        //public void Print() {
        //    Console.WriteLine("===服务器登录信息===");
        //    for (int i = 0; i < conns.Length; i++) {
        //        if (conns[i] == null) {
        //            continue;
        //        }
        //        if (!conns[i].isUse) {
        //            continue;
        //        }

        //        string str = " 连接 [" + conns[i].GetAddress() + "] ";
        //        if (conns[i].player != null) {
        //            str += "玩家id " + conns[i].player.id;
        //        }
        //        Console.WriteLine("str");
        //    }
        //}
    }
}

