using System;
using System.IO;
using System.Threading;
using System.Net.Sockets;
using System.Collections.Generic;
using Sproto;
using SprotoType;
using System.Net;

public delegate void SocketConnected();

public class NetCore
{
    private struct ConnPair
    {
        public Conn conn;
        public byte[] data;
    }

    private static Socket listenfd;

    public static bool logined;
    public static bool enabled;

    private static int CONNECT_TIMEOUT = 3000;
    private static ManualResetEvent TimeoutObject;

    private static Queue<ConnPair> recvQueue = new Queue<ConnPair>();

    private static SprotoPack recvPack = new SprotoPack();

    private static SprotoStream recvStream = new SprotoStream();

    private static ProtocolFunctionDictionary protocol = Protocol.Instance.Protocol;
    private static Dictionary<long, ProtocolFunctionDictionary.typeFunc> sessionDict;

    private static AsyncCallback receiveCallback = new AsyncCallback(Receive);

    public static void Init() {
        byte[] receiveBuffer = new byte[1 << 16];
        recvStream.Write(receiveBuffer, 0, receiveBuffer.Length);
        recvStream.Seek(0, SeekOrigin.Begin);

        sessionDict = new Dictionary<long, ProtocolFunctionDictionary.typeFunc>();
    }

    public static void StartServer(string address, int port) {
        listenfd = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        IPAddress ipAdr = IPAddress.Parse(address);
        IPEndPoint ipEp = new IPEndPoint(ipAdr, port);
        listenfd.Bind(ipEp);
        listenfd.Listen(0);
        listenfd.BeginAccept(AcceptCb, null);
    }

    //private static int receivePosition;
    public static void Receive(IAsyncResult ar = null) {

        Conn conn = (Conn)ar.AsyncState;

        lock (conn) {
            if (ar != null) {
                try {
                    conn.receivePosition += conn.socket.EndReceive(ar);
                }
                catch (Exception e) {
                    Console.WriteLine(e.ToString());
                }
            }

            conn.ProcessData();

            try {
                conn.socket.BeginReceive(conn.recvStream.Buffer, conn.receivePosition,
                    conn.recvStream.Buffer.Length - conn.receivePosition,
                    SocketFlags.None, receiveCallback, conn);
            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
            }
        }


    }

    public static void Dispatch() {
        package pkg = new package();

        if (recvQueue.Count > 20) {
            Console.WriteLine("recvQueue.Count: " + recvQueue.Count);
        }

        while (recvQueue.Count > 0) {
            ConnPair connPair = recvQueue.Dequeue();
            byte[] data = recvPack.unpack(connPair.data);
            int offset = pkg.init(data);

            int tag = (int)pkg.type;
            long session = (long)pkg.session;

            if (pkg.HasType) {
                RpcReqHandler rpcReqHandler = NetReceiver.GetHandler(tag);
                if (rpcReqHandler != null) {
                    SprotoTypeBase rpcRsp = rpcReqHandler(protocol.GenRequest(tag, data, offset));
                    if (pkg.HasSession) {
                        connPair.conn.Send(rpcRsp, session, tag);
                    }
                }
            }
            else {
                RpcRspHandler rpcRspHandler = NetSender.GetHandler(session);
                if (rpcRspHandler != null) {
                    ProtocolFunctionDictionary.typeFunc GenResponse;
                    sessionDict.TryGetValue(session, out GenResponse);
                    rpcRspHandler(GenResponse(data, offset));
                }
            }
        }
    }

    private static void AcceptCb(IAsyncResult ar) {
        try {
            Socket socket = listenfd.EndAccept(ar);

            Conn conn = new Conn();
            conn.Init(socket);
            string adr = conn.GetAddress();
            conn.socket.BeginReceive(conn.recvStream.Buffer, conn.receivePosition,
                conn.recvStream.Buffer.Length - conn.receivePosition,
                SocketFlags.None, receiveCallback, conn);
            listenfd.BeginAccept(AcceptCb, null);
        }
        catch (Exception e) {

            Console.WriteLine("AcceptCBÊ§°Ü:" + e.Message);
        }
    }

    public static void Enqueue(Conn conn, byte[] data) {
        recvQueue.Enqueue(new ConnPair() { data = data, conn = conn });
    }
    public static void RecordSession(long session, ProtocolFunctionDictionary.typeFunc typeFunc) {
        sessionDict.Add(session, typeFunc);
    }
}
