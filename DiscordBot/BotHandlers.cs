using DiscordBot.ExtensionMethods;
using DiscordBot.Models;
using DiscordBot.Options;
using DSharpPlus;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot
{
    internal class BotHandlers
    {
        private readonly Mongo _mongo;
        private readonly DiscordOptions _discordOptions;

        public BotHandlers(Mongo mongo, IOptions<DiscordOptions> discordOptions)
        {
            _mongo = mongo;
            _discordOptions = discordOptions.Value;
        }


        public async Task GuildDownloadComplete(DiscordClient discordClient, GuildDownloadCompletedEventArgs e)
        {
            var serversCollection = _mongo.GetCollection<DbServer>();
            var currentServers = await serversCollection.Find(x => true).ToListAsync();

            foreach (var guild in e.Guilds.Values)
            {
                var guildChannels = guild.Channels.Values
                    .Where(x => !x.IsCategory)
                    .Where(x => !x.IsThread)
                    .Where(x => x.Type == ChannelType.Text || x.Type == ChannelType.Voice)
                    .Select(gc => new DbChannel()
                    {
                        ChannelId = gc.Id,
                        Name = gc.Name,
                        IsVoice = gc.Type == ChannelType.Voice
                    })
                    .ToList();

                var server = currentServers.FirstOrDefault(x => x.ServerId == guild.Id);

                if(server != null)
                {
                    var updateFilter = Builders<DbServer>.Filter
                        .Eq(s => s.ServerId, server.ServerId);

                    var update = Builders<DbServer>.Update
                        .Set(s => s.Name, guild.Name)
                        .Set(s => s.Channels, guildChannels);

                    await serversCollection.UpdateOneAsync(updateFilter, update);
                }
                else
                {
                    server = new DbServer()
                    {
                        ServerId = guild.Id,
                        Name = guild.Name,
                        Channels = guildChannels
                    };
                    await serversCollection.InsertOneAsync(server);
                }
            }
        }

        public async Task MessageCreated(DiscordClient discordClient, MessageCreateEventArgs e)
        {
            bool isCommand = false;
            foreach (var prefix in _discordOptions.CommandPrefix)
                if (e.Message.Content.StartsWith(prefix, StringComparison.InvariantCultureIgnoreCase))
                    isCommand = true;

            if (isCommand)
                return;

            if (e.Message.IsMentioned("jd"))
            {
                await e.Message.RespondAsync("ORKA <a:cursed:801198310055477298>");
            }
            
        }
    }
}
