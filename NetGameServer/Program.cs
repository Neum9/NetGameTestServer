﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetGameServer {
    class Program {
        static void Main(string[] args) {
            DataMgr dataMgr = new DataMgr();

            //register
            bool ret = dataMgr.Register("Cjc", "123");
            if (ret) {
                Console.WriteLine("Register Done!");
            } else {
                Console.WriteLine("Error in registering!");
            }
            //create player
            ret = dataMgr.CreatePlayer("cjc");
            if (ret) {
                Console.WriteLine("Create player Done!");
            } else {
                Console.WriteLine("Error in creating player!");
            }
            //get player Data
            PlayerData pd = dataMgr.GetPlayerData("cjc");
            //change player Data
            pd.score += 10;
            //save data
            Player p = new Player();
            p.id = "cjc";
            p.data = pd;
            dataMgr.SavePlayer(p);
            //reload
            pd = dataMgr.GetPlayerData("cjc");
            Console.WriteLine("score " + pd.score);
            Console.Read();
        }
    }
}