using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NetGameServer {
    class DataMgr {
        MySqlConnection sqlConn;

        //单例模式
        public static DataMgr instance;
        public DataMgr() {
            instance = this;
            Connect();
        }

        public void Connect() {
            //database
            string connStr = "Database=netgametest;Data Source=127.0.0.1;";
            connStr += "User Id=root;Password=963852741;port=3306;";
            sqlConn = new MySqlConnection(connStr);
            try {
                sqlConn.Open();
            } catch (Exception e) {
                Console.WriteLine("[DataMgr]Connect " + e.Message);
                return;
            }
        }
        //判断安全字符串 防止sql注入
        public bool IsSafeStr(string str) {
            return !Regex.IsMatch(str, @"[-|;|,|\/|\(|\)|\[|\]|\}|\{|%|@|\*|!|\']");
        }
        //是否存在该用户
        private bool CanRegister(string id) {
            //防sql注入
            if (!IsSafeStr(id)) {
                return false;
            }
            //query is exist
            string cmdStr = string.Format("select * from user where id = '{0}';", id);
            MySqlCommand cmd = new MySqlCommand(cmdStr, sqlConn);
            try {
                MySqlDataReader dataReader = cmd.ExecuteReader();
                bool hasRows = dataReader.HasRows;
                dataReader.Close();
                return !hasRows;
            } catch (Exception e) {
                Console.WriteLine("{DataMgr}CanRegister fail " + e.Message);
                return false;
            }
        }
        public bool Register(string id, string pw) {
            //防sql注入
            if (!IsSafeStr(id) || !IsSafeStr(pw)) {
                Console.WriteLine("[DataMgr]Regiser使用非法字符");
                return false;
            }
            // can register
            if (!CanRegister(id)) {
                Console.WriteLine("[DataMgr]Register !CanRegiser");
                return false;
            }
            // write in database user
            string cmdStr = string.Format("Insert into user set id = '{0}' ,pw = '{1}';", id, pw);
            MySqlCommand cmd = new MySqlCommand(cmdStr, sqlConn);
            try {
                cmd.ExecuteNonQuery();
                return true;
            } catch (Exception e) {

                Console.WriteLine("[DataMgr]Register " + e.Message);
                return false;
            }
        }
        public bool CreatePlayer(string id) {
            if (!IsSafeStr(id)) {
                return false;
            }

            IFormatter formatter = new BinaryFormatter();
            MemoryStream stream = new MemoryStream();
            PlayerData playerData = new PlayerData();
            try {
                formatter.Serialize(stream, playerData);
            } catch (Exception e) {

                Console.WriteLine("[DataMgr]CreatePlayer 序列化 " + e.Message);
                return false;
            }

            byte[] byteArr = stream.ToArray();
            //write to database
            string cmdStr = string.Format("insert into player set id = '{0}',data = @data", id);
            MySqlCommand cmd = new MySqlCommand(cmdStr, sqlConn);
            cmd.Parameters.Add("@data", MySqlDbType.Blob);
            cmd.Parameters[0].Value = byteArr;
            try {
                cmd.ExecuteNonQuery();
                return true;
            } catch (Exception e) {
                Console.WriteLine("[DataMgr]CreatePlayer 写入 " + e.Message);
                return false;
            }
        }
        public bool CheckPassWord(string id, string pw) {
            if (!IsSafeStr(id) || !IsSafeStr(pw)) {
                return false;
            }
            //query
            string cmdStr = string.Format("select * from user where id = '{0}' and pw = '{1}';", id, pw);
            MySqlCommand cmd = new MySqlCommand(cmdStr, sqlConn);
            try {
                MySqlDataReader dataReader = cmd.ExecuteReader();
                bool hasRows = dataReader.HasRows;
                dataReader.Close();
                return hasRows;
            } catch (Exception e) {
                Console.WriteLine("[DataMgr]CheckPassword " + e.Message);
                return false;
            }
        }
        public PlayerData GetPlayerData(string id) {
            PlayerData playerData = null;
            if (!IsSafeStr(id)) {
                return playerData;
            }
            string cmdStr = string.Format("select * from player where id = '{0}';", id);
            MySqlCommand cmd = new MySqlCommand(cmdStr, sqlConn);
            byte[] buffer = new byte[1];
            try {
                MySqlDataReader dataReader = cmd.ExecuteReader();
                if (!dataReader.HasRows) {
                    dataReader.Close();
                    return playerData;
                }
                dataReader.Read();

                long len = dataReader.GetBytes(1, 0, null, 0, 0);
                buffer = new byte[len];
                dataReader.GetBytes(1, 0, buffer, 0, (int)len);
                dataReader.Close();
            } catch (Exception e) {
                Console.WriteLine("[DataMgr]GetPlayerData 查询 " + e.Message);
                return playerData;
            }
            //deserialize
            MemoryStream stream = new MemoryStream(buffer);
            try {
                BinaryFormatter formatter = new BinaryFormatter();
                playerData = (PlayerData)formatter.Deserialize(stream);
                return playerData;
            } catch (Exception e) {
                Console.WriteLine("[DataMgr]GetPlayerData 反序列化 " + e.Message);
                return playerData;
            }
        }
        public bool SavePlayer(Player player) {
            string id = player.id;
            PlayerData playerData = player.data;
            //serialize
            IFormatter formatter = new BinaryFormatter();
            MemoryStream stream = new MemoryStream();
            try {
                formatter.Serialize(stream, playerData);
            } catch (Exception e) {

                Console.WriteLine("[DataMgr]SavePlayer 序列化 " + e.Message);
                return false;
            }
            byte[] byteArr = stream.ToArray();
            //write into database
            string formatStr = "update player set data = @data where id = '{0}';";
            string cmdStr = string.Format(formatStr, player.id);
            MySqlCommand cmd = new MySqlCommand(cmdStr, sqlConn);
            cmd.Parameters.Add("@data", MySqlDbType.Blob);
            cmd.Parameters[0].Value = byteArr;
            try {
                cmd.ExecuteNonQuery();
                return true;
            } catch (Exception e) {
                Console.WriteLine("[DataMgr]SavePlayer 写入 " + e.Message);
                return false;
            }
        }
    }
}
