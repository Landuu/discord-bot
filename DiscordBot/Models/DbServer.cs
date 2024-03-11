using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Models
{
    public partial class DbServer
    {
        public ObjectId Id { get; set; }

        public ulong ServerId { get; set; }

        public required string Name { get; set; }

        public IList<ulong> RegisteredUsers { get; set; } = [];

        public IList<DbChannel> Channels { get; set; } = [];
    }
}
