using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;

namespace HelperBot
{
    public class PlayerInfo
    {
        public TSPlayer Player = null;
        public int Kills { get; set; }
        public int Deaths { get; set; }
        public PTime PlayTime { get; set; }
        public string Biography { get; set; }
    }
}
