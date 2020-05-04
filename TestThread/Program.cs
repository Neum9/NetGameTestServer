using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestThread {
    class Program {
        static string str = "";
        static void Main(string[] args) {
            Thread t1 = new Thread(Add1);
            t1.Start();
            Thread t2 = new Thread(Add2);
            t2.Start();

            Thread.Sleep(1000);
            Console.WriteLine(str);
            Console.Read();
        }

        public static void Add1() {
            lock (str) {
                for (int i = 0; i < 20; i++) {
                    Thread.Sleep(10);
                    str += "A";
                }
            }
        }
        public static void Add2() {
            lock (str) {
                for (int i = 0; i < 20; i++) {
                    Thread.Sleep(10);
                    str += "B";
                }
            }
        }
    }
}
