using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.ExtensionMethods
{
    internal static class DSharpExtensions
    {
        public static bool IsMentioned(this DiscordMessage message, params string[] words)
        {
            foreach (var word in words)
                if (message.Content.Contains(word, StringComparison.CurrentCultureIgnoreCase)) 
                    return true;
            return false;
        }
    }
}
