using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Options
{
    internal class DiscordOptions
    {
        public required string Token { get; set; }

        public required string[] CommandPrefix { get; set; }
    }
}
