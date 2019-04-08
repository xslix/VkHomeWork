using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;


namespace ConsoleApp1.General
{
    class FileWorker
    {
        public static void WriteDB()
        {
            using (StreamWriter sw = new StreamWriter(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/nsdb.txt"))
            {
                Logger.WriteCommand("== DB WRITING START ==");
                foreach (var p in General.GlobalSettings.players)
                {
                    sw.WriteLine(p.Key + " " 
                               + p.Value.name + " " 
                               + p.Value.nick + " " 
                               + p.Value.prem + " "  
                               + p.Value.mask + " " 
                               + p.Value.inFriends);
                    
                }
                sw.WriteLine();
                sw.Flush();
                Logger.WriteCommand("== DB WRITING END ==");
                
            }
        }
        public static void ReadDB()
        {
            using (StreamReader sw = new StreamReader(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/nsdb.txt", true))
            {
                Logger.WriteCommand("== DB READING START ==");
                GlobalSettings.players.Clear(); 
                string line;
                Player p;
               while ((line = sw.ReadLine()) != null && line.Length > 1)
                {
                    Logger.WriteCommand(line);
                    string[] words = line.Trim().Split();
                    p = new Player(words[1], words[2], Convert.ToInt32(words[3]), Convert.ToInt32(words[4]), Convert.ToInt32(words[5]));
                    GlobalSettings.players.Add(words[0], p);
                }
                sw.Close();
                Logger.WriteCommand("== DB READING END ==");
            }
            
        }

    }
}
