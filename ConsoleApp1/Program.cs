using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.Common;
using System.Timers;



namespace ConsoleApp1
{
    class Program
    {
        private static System.Timers.Timer saveTimer;
        static void Main(string[] args)
        {
            General.Logger.WriteCommand("=== PROGRAM EXECUTE ===");
            General.FileWorker.ReadDB();

            saveTimer = new System.Timers.Timer(1200000);
            saveTimer.Elapsed += reserveSaveDB;
            saveTimer.AutoReset = true;
            saveTimer.Enabled = true;
            try
            {
                General.LongPollServer.longPollResponse();
            }
            catch (Exception o)
            {
                General.Logger.WriteCommand("=== THE PROGRAM ENDS WITH AN ERROR ===");
                General.Logger.WriteCommand(o.ToString());
            }
            finally
            {
                General.FileWorker.WriteDB();
            }
        }
        private static void reserveSaveDB(Object source, ElapsedEventArgs e)
        {
            General.Logger.WriteCommand("=== RESERVE SAVING ===");
            General.FileWorker.WriteDB();
        }
    }
}
