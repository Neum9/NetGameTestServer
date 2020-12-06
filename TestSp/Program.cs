using Sproto;
using SprotoType;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace TestSp
{
    class Program
    {
        static SprotoRpc client = new SprotoRpc();
        static SprotoRpc service = new SprotoRpc(Protocol.Instance);
        static SprotoRpc.RpcRequest clientRequest = client.Attach(Protocol.Instance);

        static void Main(string[] args) {
            runServer();
        }

        static void runServer() {
            Console.WriteLine("{Server start!}");

            NetSender.Init();
            NetReceiver.Init();
            NetCore.Init();

            InitMgr();

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

        static void InitMgr() {


            NetReceiver.AddHandler<Protocol.foobar>(
                (SprotoTypeBase sp, long session) => {
                    EventManager.instance.FireEvent(EVENTKEY.net_Recv, sp, session, Protocol.foobar.Tag);
                    return null;
                }
                );

            NetReceiver.AddHandler<Protocol.playopt>(
                (SprotoTypeBase sp, long session) => {
                    EventManager.instance.FireEvent(EVENTKEY.net_Recv, sp, session, Protocol.playopt.Tag);
                    return null;
                }
    );
        }
    }
}
