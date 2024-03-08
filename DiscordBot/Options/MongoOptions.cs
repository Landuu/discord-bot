using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Options
{
    internal class MongoOptions
    {
        public required string ConnectionString { get; set; }

        public required string DatabaseName { get; set; }
    }
}
