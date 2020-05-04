using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetGameServer {
    [Serializable]
    public class PlayerData {
        public int score = 0;
        public PlayerData() {
            score = 100;
        }
    }
}
