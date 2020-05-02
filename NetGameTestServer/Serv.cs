using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Data;

namespace NetGameTestServer {
    class Serv {
        //监听套接字
        public Socket listenfd;
        //客户端连接
        public Conn[] conns;
        //最大连接数
        public int maxConn = 50;
        //数据库
        MySqlConnection sqlConn;

        //获取连接池索引 返回负数表示获取失败
        public int NewIndex() {
            if (conns == null) {
                return -1;
            }
            for (int i = 0; i < conns.Length; i++) {
                if (conns[i] == null) {
                    conns[i] = new Conn();
                    return i;
                } else if (conns[i].isUse == false) {
                    return i;
                }
            }
            return -1;
        }
        //开启服务器
        public void Start(string host, int port) {

            //数据库
            string connStr = "Database=test;Data Source=127.0.0.1;";
            connStr += "UserId=root;Password=963852741;port=3306;";
            sqlConn = new MySqlConnection(connStr);
            try {
                sqlConn.Open();
            } catch (Exception e) {

                Console.WriteLine("数据库连接失败 " + e.Message);
                return;
            }

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

        private void AcceptCb(IAsyncResult ar) {
            try {
                Socket socket = listenfd.EndAccept(ar);
                int index = NewIndex();

                if (index < 0) {
                    socket.Close();
                    Console.WriteLine("[警告]连接已满!");
                } else {
                    Conn conn = conns[index];
                    conn.Init(socket);
                    string adr = conn.GetAddress();
                    Console.WriteLine("客户端连接[" + adr + "] coon池ID: " + index);
                    conn.socket.BeginReceive(conn.readBuff, conn.buffCount, conn.BuffRemain(), SocketFlags.None, ReceiveCb, conn);
                    listenfd.BeginAccept(AcceptCb, null);
                }
            } catch (Exception e) {

                Console.WriteLine("AcceptCB失败:" + e.Message);
            }
        }

        private void ReceiveCb(IAsyncResult ar) {
            Conn conn = (Conn)ar.AsyncState;
            try {
                int count = conn.socket.EndReceive(ar);
                //关闭信号
                if (count <= 0) {
                    Console.Write("收到 [" + conn.GetAddress() + "] 断开连接!");
                    conn.Close();
                    return;
                }
                //数据处理
                string str = System.Text.Encoding.UTF8.GetString(conn.readBuff, 0, count);
                Console.WriteLine("收到 [" + conn.GetAddress() + "]数据:" + str);
                HandleMsg(conn, str);
                str = conn.GetAddress() + ":" + str;
                byte[] bytes = System.Text.Encoding.Default.GetBytes(str);
                //广播
                for (int i = 0; i < conns.Length; i++) {
                    if (conns[i] == null) {
                        continue;
                    } else if (!conns[i].isUse) {
                        continue;
                    }
                    Console.WriteLine("将消息转播给 " + conns[i].GetAddress());
                    conns[i].socket.Send(bytes);
                }
                //继续接收
                conn.socket.BeginReceive(conn.readBuff, conn.buffCount, conn.BuffRemain(), SocketFlags.None, ReceiveCb, conn);
            } catch (Exception e) {

                Console.WriteLine("收到 [" + conn.GetAddress() + "] 断开连接");
                conn.Close();
            }
        }

        private void HandleMsg(Conn conn, string str) {
            if (str == "_GET") {
                string cmdStr = "select * from msg order by id desc limit 10;";
                MySqlCommand cmd = new MySqlCommand(cmdStr, sqlConn);
                try {
                    MySqlDataReader dataReader = cmd.ExecuteReader();
                    str = "";
                    while (dataReader.Read()) {
                        str += dataReader["name"] + ":" + dataReader["msg"] + "\n\r";
                    }
                    dataReader.Close();
                    byte[] bytes = System.Text.Encoding.Default.GetBytes(str);
                    conn.socket.Send(bytes);
                } catch (Exception e) {

                    Console.WriteLine("数据库查询失败" + e.Message);
                }
            } else {
                // 插入数据
                string cmdStrFormat = "insert into msg set name= '{0}',msg = '{1}';";
                string cmdStr = string.Format(cmdStrFormat, conn.GetAddress(), str);
                MySqlCommand cmd = new MySqlCommand(cmdStr, sqlConn);
                try {
                    cmd.ExecuteNonQuery();
                } catch (Exception e) {

                    Console.WriteLine("数据库插入失败" + e.Message);
                }
            }
        }
    }

}
