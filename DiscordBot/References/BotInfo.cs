using Discord;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Static
{
    class BotInfo
    {
        public static string Token {
            get
            {
                StreamReader reader = File.OpenText("Token.token");
                string line = reader.ReadLine();
                Console.Write("Token: " + line + "\n");
                return line;
            }
        }
        public static string Game = "-̸̷̨̨̢̯̰̣͔͍̼̯͔̺̦̾̃̋̌͆̍̏̋͋͋̂̾͊̄͠Dat A$$-̸̨̨̢̯̰̣͔͍̼̯͔̾̃̋̌͆̍̏̋͋͠";
        public static UserStatus Status = UserStatus.DoNotDisturb;
    }
}
