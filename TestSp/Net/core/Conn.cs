using Sproto;
using SprotoType;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;


public class Conn
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
    //对应的Player
    public Player player;


    //add
    public SprotoStream sendStream = new SprotoStream();
    public SprotoStream recvStream = new SprotoStream();

    private static ProtocolFunctionDictionary protocol = Protocol.Instance.Protocol;
    private static SprotoPack sendPack = new SprotoPack();

    public int receivePosition;

    public int m_index;

    //构造函数
    public Conn() {
        readBuff = new byte[BUFFER_SIZE];

    }
    //初始化
    public void Init(Socket socket, int index) {
        this.socket = socket;
        m_index = index;
        isUse = true;
        buffCount = 0;
        //心跳处理,稍后实现GetTimeStamp方法
        lastTickTime = Sys.GetTimeStamp();
        //TODOCJc 应该在登录的时候new
        player = new Player(m_index.ToString(), this);
        player.OnMsg();
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
        if (player != null) {
            //player.Logout();
            return;
        }
        Console.WriteLine("断开连接 " + GetAddress());
        socket.Shutdown(SocketShutdown.Both);
        socket.Close();
        isUse = false;
    }

    // 记录到seeionDict的时候需要记录Conn吗
    private static int MAX_PACK_LEN = (1 << 16) - 1;
    public void Send(SprotoTypeBase rpc, long? session, int? tag) {
        package pkg = new package();
        if (tag != null) {
            pkg.type = (long)tag;
        }

        if (session != null) {
            pkg.session = (long)session;
            if (tag != null) {
                NetCore.RecordSession((long)session, protocol[(int)tag].Response.Value);
            }
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
                NetCore.Enqueue(this, data);
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

}

