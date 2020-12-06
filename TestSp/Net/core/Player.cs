using Sproto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Player
{
    public string id;
    public Conn conn;
    ////数据
    //public PlayerData data;
    ////临时数据
    //public PlayerTempData tempData;

    private delegate void onNet(SprotoTypeBase sp, long session);
    private Dictionary<int, onNet> m_onNetList;

    //构造函数
    public Player(string id, Conn conn) {
        this.id = id;
        this.conn = conn;
        initMsg();
        //tempData = new PlayerTempData();
    }


    public void initMsg() {
        m_onNetList = new Dictionary<int, onNet>();
        m_onNetList.Add(Protocol.foobar.Tag, onFoobar);
        m_onNetList.Add(Protocol.playopt.Tag, OnPlayOpt);
    }

    public void OnMsg() {
        EventManager.instance.AddHandler(EVENTKEY.net_Recv, OnNetRecv);
    }

    // 处理消息
    public void OnNetRecv(SprotoTypeBase sp, long session, int tag) {
        m_onNetList[tag](sp, session);
    }
    public void onFoobar(SprotoTypeBase sp, long session) {
        SprotoType.foobar.response obj2 = new SprotoType.foobar.response();
        obj2.ok = true;
        conn.Send(obj2, session, null);
    }
    public void OnPlayOpt(SprotoTypeBase sp, long session) {
        SprotoType.playopt.request req = sp as SprotoType.playopt.request;
        SprotoType.playopt.response resp = new SprotoType.playopt.response();
        resp.ret = 0;
        resp.optUnit = req.optUnit;
        Console.WriteLine("OnPlayOpt: type :" + resp.optUnit.type + " value: " + resp.optUnit.param);
        Console.WriteLine("OnPlayOpt session " + session);
        conn.Send(resp, session, null);
    }
}
