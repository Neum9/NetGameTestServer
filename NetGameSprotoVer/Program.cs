using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetGameServer
{
    class Program
    {
        static void Main(string[] args) {
            ServNet servNet = new ServNet();
            servNet.Start("127.0.0.1", 1234);

            while (true) {
                string str = Console.ReadLine();
                switch (str) {
                    case "quit":
                        servNet.Close();
                        return;
                }
            }
        }
    }
}
