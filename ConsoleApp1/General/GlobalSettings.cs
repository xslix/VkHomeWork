using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;


namespace ConsoleApp1.General
{
    class GlobalSettings
    {
       // public static string token = "df62472825dd9c14a92933a21625badc80890cf2c7f381e3e433ffcc4b0e9983ab059b238719f197aaa1d"; //noblesse
        public static string token = "13befc5b949cea250af4026b3c9f88eeb8ab6ce46d191f4f814d24430144dd621b15774906b35a720888a"; //ns
        public static string version = "5.92";
        public static Dictionary<string, Player> players
            = new Dictionary<string, Player>
        {
            ["14787658"] = new Player( "Ilya", "Elisan", 11, 0, 0),
            ["78250101"] = new Player("Stas", "Agent", 1, 0, 1),
            ["48865001"] = new Player("Egor", "Itan",  0, 0, 0),
        };

    }
    class Player
    {
        public string name;
        public string nick;
        public int prem;
        public int mask;
        public int inFriends;

        public Player (string name, string nick, int prem, int mask, int inFriends)
        {
            this.name = name;
            this.nick = nick;
            this.prem = prem;
            this.mask = mask;
            this.inFriends = inFriends;
        }
    }
}
