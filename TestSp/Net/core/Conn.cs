using Sproto;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

class Conn
{
    //常量
    public const int BUFFER_SIZE = 1024;
    //Socket
    public Socket socket;
    public bool isUse = false;
    //Buff
    public byte[] readBuff = new byte[BUFFER_SIZE];
    public int buffCount = 0;
    //粘包分包
    public byte[] lenByte = new byte[sizeof(UInt32)];
    public Int32 msgLength = 0;
    //心跳时间
    public long lastTickTime = long.MinValue;

    //add
    public SprotoStream sendStream = new SprotoStream();
    public SprotoStream recvStream = new SprotoStream();

    private static ProtocolFunctionDictionary protocol = Protocol.Instance.Protocol;

    //构造函数
    public Conn() {
        readBuff = new byte[BUFFER_SIZE];
    }
    //初始化
    public void Init(Socket socket) {
        this.socket = socket;
        isUse = true;
        buffCount = 0;
        //心跳处理,稍后实现GetTimeStamp方法 TODOCjc
        //lastTickTime = Sys.GetTimeStamp();
    }
    // 剩余的Buff
    public int BuffRemain() {
        return BUFFER_SIZE - buffCount;
    }
    //获取客户端地址
    public string GetAddress() {
        if (!isUse) {
            return "无法获取地址";
        }
        return socket.RemoteEndPoint.ToString();
    }
    //关闭
    public void Close() {
        if (!isUse) {
            return;
        }
        //TODOCjc
        //if (player != null) {
        //    player.Logout();
        //    return;
        //}
        Console.WriteLine("断开连接 " + GetAddress());
        socket.Shutdown(SocketShutdown.Both);
        socket.Close();
        isUse = false;
    }

    //TODOCjc
    //public void Send(ProtocolBytes protocol) {
    //    ServNet.instance.Send(this, protocol);
    //}

    public int receivePosition;

    public void ProcessData() {
        int i = recvStream.Position;
        while (receivePosition >= i + 2) {
            int length = (recvStream[i] << 8) | recvStream[i + 1];

            int sz = length + 2;
            if (receivePosition < i + sz) {
                break;
            }

            recvStream.Seek(2, SeekOrigin.Current);

            if (length > 0) {
                byte[] data = new byte[length];
                recvStream.Read(data, 0, length);
                NetCore.Enqueue(data);
            }

            i += sz;
        }

        if (receivePosition == recvStream.Buffer.Length) {
            recvStream.Seek(0, SeekOrigin.End);
            recvStream.MoveUp(i, i);
            receivePosition = recvStream.Position;
            recvStream.Seek(0, SeekOrigin.Begin);
        }
    }

    public static void Send<T>(SprotoTypeBase rpc = null, long? session = null) {
        Send(rpc, session, protocol[typeof(T)]);
    }

    private static int MAX_PACK_LEN = (1 << 16) - 1;
    private static void Send(SprotoTypeBase rpc, long? session, int tag) {
        if (!connected || !enabled) {
            return;
        }

        package pkg = new package();
        pkg.type = tag;

        if (session != null) {
            pkg.session = (long)session;
            sessionDict.Add((long)session, protocol[tag].Response.Value);
        }

        sendStream.Seek(0, SeekOrigin.Begin);
        int len = pkg.encode(sendStream);
        if (rpc != null) {
            len += rpc.encode(sendStream);
        }

        byte[] data = sendPack.pack(sendStream.Buffer, len);
        if (data.Length > MAX_PACK_LEN) {
            Console.WriteLine("data.Length > " + MAX_PACK_LEN + " => " + data.Length);
            return;
        }

        sendStream.Seek(0, SeekOrigin.Begin);
        sendStream.WriteByte((byte)(data.Length >> 8));
        sendStream.WriteByte((byte)data.Length);
        sendStream.Write(data, 0, data.Length);

        try {
            socket.Send(sendStream.Buffer, sendStream.Position, SocketFlags.None);
        }
        catch (Exception e) {
            Console.WriteLine(e.ToString());
        }
    }
}

