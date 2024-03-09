using DiscordBot.ExtensionMethods;
using DiscordBot.Models;
using DSharpPlus;
using DSharpPlus.EventArgs;
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

        public BotHandlers(Mongo mongo)
        {
            _mongo = mongo;
        }


        public async Task GuildDownloadComplete(DiscordClient discordClient, GuildDownloadCompletedEventArgs e)
        {
            await _mongo.GetCollection<DbServer>().DeleteManyAsync(Builders<DbServer>.Filter.Empty);
            var servers = new List<DbServer>();

            var guilds = e.Guilds.Values;
            foreach (var guild in guilds)
            {
                var server = new DbServer()
                {
                    ServerId = guild.Id,
                    Name = guild.Name,
                };

                var channels = guild.Channels.Values;
                foreach (var channel in channels)
                {
                    if (channel.IsCategory || channel.IsThread) continue;
                    if (channel.Type != ChannelType.Text && channel.Type != ChannelType.Voice) continue;

                    server.Channels.Add(new()
                    {
                        ChannelId = channel.Id,
                        Name = channel.Name,
                        IsVoice = channel.Type == ChannelType.Voice
                    });
                }

                servers.Add(server);
            }

            await _mongo.GetCollection<DbServer>().InsertManyAsync(servers);
        }

        public async Task MessageCreated(DiscordClient discordClient, MessageCreateEventArgs e)
        {
            if (e.Message.IsCommand("ping"))
            {
                await e.Message.RespondAsync("PONG!");
            }
            else if (e.Message.IsMentioned("jd"))
            {
                await e.Message.RespondAsync("ORKA <a:cursed:801198310055477298>");
            }
            else if (e.Message.IsCommand("halo"))
            {
                var serverId = e.Message.Channel.GuildId;
                var server = await _mongo.GetDbServer(serverId);

                if (server == null)
                {
                    await e.Message.RespondAsync("<:madge:797551865675644949>");
                    return;
                }

                var usersInChannels = new Dictionary<string, IEnumerable<string>>();
                var voiceChannels = server.Channels.Where(x => x.IsVoice);
                foreach (var channel in voiceChannels)
                {
                    var currentChannel = await discordClient.GetChannelAsync(channel.ChannelId);
                    var users = currentChannel.Users;
                    if (users.Count > 0)
                        usersInChannels.Add(channel.Name, users.Select(x => x.DisplayName));
                }

                if (usersInChannels.Count > 0)
                {
                    string msg = "NIECH TO PIERON STRZELI! KTOŚ JEST NA DISCORDZIE NIEMOŻLIWE <:peepoG:757657584508469329>:";
                    foreach (var uic in usersInChannels)
                        msg += $"\n - {uic.Key}: {string.Join(", ", uic.Value)}";
                    await e.Message.RespondAsync(msg);
                }
                else
                {
                    await e.Message.RespondAsync("Nie ma nikogo na discordzie, chyba każdy znalazł sobie lepszych kolegów <:sadge:753604078499790849>");
                }
            }
        }
    }
}
