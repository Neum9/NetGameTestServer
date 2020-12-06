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

    //add
    //客户端连接
    public static Conn[] conns;
    //最大连接数
    public static int maxConn = 50;

    public static void Init() {
        byte[] receiveBuffer = new byte[1 << 16];
        recvStream.Write(receiveBuffer, 0, receiveBuffer.Length);
        recvStream.Seek(0, SeekOrigin.Begin);

        sessionDict = new Dictionary<long, ProtocolFunctionDictionary.typeFunc>();
    }

    //获取连接池索引 返回负数表示获取失败
    public static int NewIndex() {
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

    public static void StartServer(string address, int port) {
        //连接池
        conns = new Conn[maxConn];
        for (int i = 0; i < maxConn; i++) {
            conns[i] = new Conn();
        }

        listenfd = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        IPAddress ipAdr = IPAddress.Parse(address);
        IPEndPoint ipEp = new IPEndPoint(ipAdr, port);
        listenfd.Bind(ipEp);
        listenfd.Listen(0);
        listenfd.BeginAccept(AcceptCb, null);
    }

    public static void Receive(IAsyncResult ar = null) {

        Conn conn = (Conn)ar.AsyncState;

        lock (conn) {
            try {

                int count = conn.socket.EndReceive(ar);
                if (count <= 0) {
                    Console.WriteLine("client close:" + conn.m_index);
                    conn.Close();
                    return;
                }
                conn.receivePosition += count;
            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
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
            Console.WriteLine("dequeue one!");
            ConnPair connPair = recvQueue.Dequeue();
            byte[] data = recvPack.unpack(connPair.data);
            int offset = pkg.init(data);

            int tag = (int)pkg.type;
            long session = (long)pkg.session;

            if (pkg.HasType) {
                RpcReqHandler rpcReqHandler = NetReceiver.GetHandler(tag);
                if (rpcReqHandler != null) {
                    SprotoTypeBase rpcRsp = rpcReqHandler(protocol.GenRequest(tag, data, offset), session);
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
            int index = NewIndex();

            Console.WriteLine("客户端连入: " + index);

            Conn conn = conns[index];
            conn.Init(socket, index);
            conn.socket.BeginReceive(conn.recvStream.Buffer, conn.receivePosition,
                conn.recvStream.Buffer.Length - conn.receivePosition,
                SocketFlags.None, receiveCallback, conn);
            listenfd.BeginAccept(AcceptCb, null);
        }
        catch (Exception e) {

            Console.WriteLine("AcceptCB失败:" + e.Message);
        }
    }

    public static void Enqueue(Conn conn, byte[] data) {
        recvQueue.Enqueue(new ConnPair() { data = data, conn = conn });
    }
    public static void RecordSession(long session, ProtocolFunctionDictionary.typeFunc typeFunc) {
        sessionDict.Add(session, typeFunc);
    }
}
