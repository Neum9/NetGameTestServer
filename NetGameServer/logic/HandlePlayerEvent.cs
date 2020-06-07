using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetGameServer {
    // 玩家事件类
    class HandlePlayerEvent {
        // 上线
        public void OnLogin(Player player) {

        }
        // 下线
        public void OnLogout(Player player) {

        }
    }

    // 处理连接协议
    partial class HandleConnMsg {
        //心跳
        //协议参数:无
        public void MsgHeatBeat(Conn conn, ProtocolBase protoBase) {
            conn.lastTickTime = Sys.GetTimeStamp();
            Console.WriteLine("[更新心跳时间]" + conn.GetAddress());
        }

        //注册
        //协议参数:str用户名,str密码
        //返回协议:-1表示失败 0表示成功
        public void MsgRegister(Conn conn, ProtocolBase protoBase) {
            //获取数值
            int start = 0;
            ProtocolBytes protocol = (ProtocolBytes)protoBase;
            string protoName = protocol.GetString(start, ref start);
            string id = protocol.GetString(start, ref start);
            string pw = protocol.GetString(start, ref start);
            string strFormat = "[收到注册协议]" + conn.GetAddress();
            Console.WriteLine(strFormat + " 用户名:" + id + " 密码" + pw);
            //构建返回协议
            protocol = new ProtocolBytes();
            protocol.AddString("Register");
            //注册
            if (DataMgr.instance.Register(id, pw)) {
                protocol.AddInt(0);
            } else {
                protocol.AddInt(-1);
            }
            //创建角色
            DataMgr.instance.CreatePlayer(id);
            //返回协议给客户端
            conn.Send(protocol);
        }

        //登录
        //协议参数:str用户名,str密码
        //返回协议:-1表示失败 0表示成功
        public void MsgLogin(Conn conn,ProtocolBase protoBase) {
            //获取数值
            int start = 0;
            ProtocolBytes protocol = (ProtocolBytes)protoBase;
            string protoName = protocol.GetString(start, ref start);
            string id = protocol.GetString(start, ref start);
            string pw = protocol.GetString(start, ref start);
            string strFormat = "[收到登录协议]" + conn.GetAddress();
            Console.WriteLine(strFormat + " 用户名 : " + id + " 密码: " + pw);
            //构建返回协议
            ProtocolBytes protocolRet = new ProtocolBytes();
            protocolRet.AddString("Login");
            //验证
            if (!DataMgr.instance.CheckPassWord(id,pw)) {
                protocolRet.AddInt(-1);
                conn.Send(protocolRet);
                return;
            }
            //是否已经登录
            //TODOCJc
        }
    }

    // 处理角色协议
    class HandlePlayerMsg {

    }
}
