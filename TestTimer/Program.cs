using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace TestTimer {
    class Program {
        static void Main(string[] args) {
            Timer timer = new Timer();
            timer.AutoReset = true;
            timer.Interval = 1000;
            timer.Elapsed += new ElapsedEventHandler(Tick);
            timer.Start();

            Console.Read();
        }

        public static void Tick(object sender, System.Timers.ElapsedEventArgs e) {
            Console.WriteLine("每秒执行一次!");
        }
    }
}
