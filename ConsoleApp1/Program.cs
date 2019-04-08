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
        static void Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("Enter user ID:");
                string userid = Console.ReadLine();
                try
                {
                    General.LongPollServer.longPollResponse(userid);
                }
                catch (Exception o)
                {
                    Console.WriteLine("Request error: " + o.Message);
                }

            }
            Console.ReadKey();

        }
        
    }
}
