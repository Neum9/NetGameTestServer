using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NetGameTestServer {
    class Program {
        //static void Main(string[] args) {
        //    Console.WriteLine("Hello World!");
        //    //Socket
        //    Socket listenfd = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        //    //Bind
        //    IPAddress ipAdr = IPAddress.Parse("127.0.0.1");
        //    IPEndPoint ipEp = new IPEndPoint(ipAdr, 1234);
        //    listenfd.Bind(ipEp);
        //    listenfd.Listen(0);
        //    Console.WriteLine("{Server start!}");
        //    while (true) {
        //        //Accept
        //        Socket connfd = listenfd.Accept();
        //        Console.WriteLine("{Server Accpet!}");
        //        //Recv
        //        byte[] readBuff = new byte[1024];
        //        int count = connfd.Receive(readBuff);
        //        string str = System.Text.Encoding.UTF8.GetString(readBuff, 0, count);
        //        Console.WriteLine("{server receice!} " + str);
        //        //Send
        //        byte[] bytes = System.Text.Encoding.Default.GetBytes("server echo " + str);
        //        connfd.Send(bytes);
        //    }
        //}
        public static void Main(string[] args) {
            Console.WriteLine("Hello World!");
            Serv serv = new Serv();
            serv.Start("127.0.0.1", 1234);
            while (true) {
                string str = Console.ReadLine();
                switch (str) {
                    case "quit":
                        return;
                }
            }
        }
    }
}
