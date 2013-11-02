using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MinecraftServerStatusConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var ServerStatus = new MinecraftServerStatus("changeme.net");

            if (ServerStatus.Online)
            {
                Console.WriteLine("Server is online.");
                Console.WriteLine("MOTD: " + ServerStatus.MOTD);
                Console.WriteLine("Players Online:  " + ServerStatus.PlayersOnline);
                Console.WriteLine("Player Capacity: " + ServerStatus.PlayersMax);
            }
            else
            {
                Console.WriteLine("Server not online!");
            }

            Console.ReadKey();
        }
    }
}
