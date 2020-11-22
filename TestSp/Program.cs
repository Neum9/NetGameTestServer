using Sproto;
using System;
using System.Net;
using System.Net.Sockets;

namespace TestSp
{
    class Program
    {
        static void Main(string[] args) {
            //runSpc();
            runServer();
        }
        static void runSpc() {
            SprotoRpc client = new SprotoRpc();
            SprotoRpc service = new SprotoRpc(Protocol.Instance);
            SprotoRpc.RpcRequest clientRequest = client.Attach(Protocol.Instance);

            // ===============foobar=====================
            // request

            SprotoType.foobar.request obj = new SprotoType.foobar.request();
            obj.what = "foo";
            byte[] req = clientRequest.Invoke<Protocol.foobar>(obj, 1);

            // dispatch
            SprotoRpc.RpcInfo sinfo = service.Dispatch(req);
        }

        static void runServer() {
            ////Socket
            //Socket listenfd = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            ////Bind
            //IPAddress ipAdr = IPAddress.Parse("127.0.0.1");
            //IPEndPoint ipEp = new IPEndPoint(ipAdr, 1234);
            //listenfd.Bind(ipEp);
            //listenfd.Listen(0);
            //Console.WriteLine("{Server start!}");

            //SprotoRpc service = new SprotoRpc(Protocol.Instance);

            //while (true) {
            //    //Accept
            //    Socket connfd = listenfd.Accept();
            //    Console.WriteLine("{Server Accpet!}");
            //    //Recv
            //    //byte[] readBuff = new byte[1024];
            //    //int count = connfd.Receive(readBuff);
            //    //string str = System.Text.Encoding.UTF8.GetString(readBuff, 0, count);
            //    //Console.WriteLine("{server receice!} " + str);
            //    //Send
            //    //byte[] bytes = System.Text.Encoding.Default.GetBytes("server echo " + str);
            //    //connfd.Send(bytes);
            //    try {
            //        byte[] readBuff = new byte[1024];
            //        int count = connfd.Receive(readBuff);
            //        SprotoRpc.RpcInfo sinfo = service.Dispatch(readBuff);

            //        if (sinfo.requestObj.GetType() == typeof(SprotoType.foobar.request)) {

            //            SprotoType.foobar.request req_obj = (SprotoType.foobar.request)sinfo.requestObj;
            //            Console.WriteLine("request: " + req_obj.what);
            //            SprotoType.foobar.response obj2 = new SprotoType.foobar.response();
            //            obj2.ok = false;
            //            byte[] resp = sinfo.Response(obj2);
            //            connfd.Send(resp);
            //        }

            //    }
            //    catch (System.Exception ex) {
            //        Console.WriteLine("连接已断开:" + ex.Message);
            //    }

            //}


            Console.WriteLine("{Server start!}");

            NetSender.Init();
            NetReceiver.Init();
            NetCore.Init();

            NetCore.logined = true;
            NetCore.enabled = true;

            NetCore.StartServer("127.0.0.1", 1234);

            ProcessNet();

            Console.ReadKey();
        }

        static void ProcessNet() {
            var timer = new System.Threading.Timer(Update, null, 0, 100);
        }

        static void Update(object obj) {
            NetCore.Dispatch();
        }
    }
}
