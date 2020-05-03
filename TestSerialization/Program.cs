using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace TestSerialization {

    [Serializable]
    class Player {
        public int coin = 0;
        public int money = 0;
        public string name = "";
    }


    class Program {
        static void Main(string[] args) {
            funcDeSerializate();
            Console.Read();
        }

        static void funcSerializate() {
            Player player = new Player();
            player.coin = 1;
            player.money = 10;
            player.name = "Cjc";

            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream("data.bin", FileMode.Create, FileAccess.Write, FileShare.None);
            formatter.Serialize(stream, player);
            stream.Close();
        }

        static void funcDeSerializate() {
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream("data.bin", FileMode.Open, FileAccess.Read, FileShare.Read);
            Player player = (Player)formatter.Deserialize(stream);
            stream.Close();
            Console.WriteLine("coin {0}", player.coin);
            Console.WriteLine("money {0}", player.money);
            Console.WriteLine("name {0}", player.name);

        }
    }
}
