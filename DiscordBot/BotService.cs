using DiscordBot.Models;
using DiscordBot.Options;
using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
            await _mongo.GetDb().DropCollectionAsync("servers", token);
            await _mongo.GetDb().CreateCollectionAsync("servers", cancellationToken: token);
            var serversCollection = _mongo.GetDb().GetCollection<DbServer>("servers");

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
                            IsVoice = channel.Type == ChannelType.Text
                        });
                    }

                    servers.Add(server);
                }

                await serversCollection.InsertManyAsync(servers);
            };


            _discordClient.MessageCreated += async (s, e) =>
            {
                if (e.Message.Content.Equals("ping", StringComparison.InvariantCultureIgnoreCase))
                    await e.Message.RespondAsync("PONG!");

                if (e.Message.Content.Equals("jd", StringComparison.InvariantCultureIgnoreCase))
                    await e.Message.RespondAsync("ORKA :smOrc:");
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
