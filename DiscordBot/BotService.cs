using DiscordBot.ExtensionMethods;
using DiscordBot.Models;
using DiscordBot.Options;
using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot
{
    internal class BotService : IHostedService
    {
        private readonly ILogger<BotService> _logger;
        private readonly IHostApplicationLifetime _applicationLifetime;
        private readonly Mongo _mongo;
        private readonly DiscordClient _discordClient;
        private bool _start = true;

        public BotService(ILogger<BotService> logger, IHostApplicationLifetime applicationLifetime, IOptions<DiscordOptions> discordOptions, Mongo mongo)
        {
            var _discordOptions = discordOptions.Value;
            _logger = logger;
            _applicationLifetime = applicationLifetime;
            _mongo = mongo;
           
            _discordClient = new(new()
            {
                MinimumLogLevel = LogLevel.Debug,
                Token = _discordOptions.Token,
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.AllUnprivileged | DiscordIntents.MessageContents
            });
        }

        public async Task StartAsync(CancellationToken token)
        {
            var serversCollection = _mongo.GetServersCollection();
            await serversCollection.DeleteManyAsync(Builders<DbServer>.Filter.Empty, token);

            if (_start)
                await _discordClient.ConnectAsync();

            _discordClient.GuildDownloadCompleted += async (s, e) =>
            {
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
                    foreach(var channel in channels)
                    {
                        if (channel.IsCategory || channel.IsThread) continue;
                        if(channel.Type != ChannelType.Text && channel.Type != ChannelType.Voice) continue;

                        server.Channels.Add(new()
                        {
                            ChannelId = channel.Id,
                            Name = channel.Name,
                            IsVoice = channel.Type == ChannelType.Voice
                        });
                    }

                    servers.Add(server);
                }

                await serversCollection.InsertManyAsync(servers);
            };


            _discordClient.MessageCreated += async (s, e) =>
            {
                if (e.Message.IsCommand("ping"))
                {
                    await e.Message.RespondAsync("PONG!");
                }
                else if (e.Message.IsMentioned("jd"))
                {
                    await e.Message.RespondAsync("ORKA <a:cursed:801198310055477298>");
                }
                else if(e.Message.IsCommand("halo"))
                {
                    var serverId = e.Message.Channel.GuildId;
                    var server = await _mongo.GetServersCollection().Find(x => x.ServerId == serverId).FirstOrDefaultAsync();
                    if (server == null)
                    {
                        await e.Message.RespondAsync("<:madge:797551865675644949>");
                        return;
                    }

                    var usersInChannels = new Dictionary<string, IEnumerable<string>>();
                    var voiceChannels = server.Channels.Where(x => x.IsVoice);
                    foreach (var channel in voiceChannels)
                    {
                        var currentChannel = await _discordClient.GetChannelAsync(channel.ChannelId);
                        var users = currentChannel.Users;
                        if (users.Count > 0)
                            usersInChannels.Add(channel.Name, users.Select(x => x.DisplayName));
                    }

                    if(usersInChannels.Count > 0)
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
            };

            _discordClient.ChannelUpdated += async (s, e) =>
            {
                var c = e.Guild.Channels;
            };
            // Other startup things here
        }

        public async Task StopAsync(CancellationToken token)
        {
            await _discordClient.DisconnectAsync();
            // More cleanup possibly here
        }
    }
}
