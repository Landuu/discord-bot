using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Models
{
    public class DbChannel
    {
        public ObjectId Id { get; set; }

        public ulong ChannelId { get; set; }

        public required string Name { get; set; }

        public bool IsVoice { get; set; }
    }
}
