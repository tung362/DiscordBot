using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.References
{
    class Tools
    {
        //Add extra spaces to string
        public static string SpaceToString(string text, int maxSize)
        {
            string newText = text;
            if(text.Length < maxSize)
            {
                int amountNeeded = maxSize - text.Length;
                for (int i = 0; i < amountNeeded; i++) newText += " ";
            }
            return newText;
        }
    }
}
