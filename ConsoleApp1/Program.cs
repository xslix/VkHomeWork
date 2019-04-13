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
                    var res = General.LongPollServer.longPollResponse(userid);
                    foreach (var group in res)
                    {
                        Console.WriteLine(group.Item1 + " Comments: " + group.Item2.ToString() + " Likes: " + group.Item3.ToString());
                    }
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
