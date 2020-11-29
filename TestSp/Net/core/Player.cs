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

    //构造函数
    public Player(string id, Conn conn) {
        this.id = id;
        this.conn = conn;
        //tempData = new PlayerTempData();
    }


    ////踢下线
    //public static bool KickOff(string id, ProtocolBase proto) {
    //    Conn[] conns = ServNet.instance.conns;
    //    for (int i = 0; i < conns.Length; i++) {
    //        if (conns[i] == null) {
    //            continue;
    //        }
    //        if (!conns[i].isUse) {
    //            continue;
    //        }
    //        if (conns[i].player == null) {
    //            continue;
    //        }
    //        if (conns[i].player.id == id) {
    //            lock (conns[i].player) {
    //                if (proto != null) {
    //                    conns[i].player.Send(proto);
    //                }
    //                return conns[i].player.Logout();
    //            }
    //        }
    //    }
    //    return true;
    //}

    ////下线
    //public bool Logout() {
    //    //事件处理

    //    ServNet.instance.handlePlayerEvent.OnLogout(this);

    //    // 保存
    //    if (!DataMgr.instance.SavePlayer(this)) {
    //        return false;
    //    }
    //    // 下线
    //    conn.player = null;
    //    conn.Close();
    //    return true;
    //}
}
