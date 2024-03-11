using DiscordBot.Commands;
using DiscordBot.ExtensionMethods;
using DiscordBot.Models;
using DiscordBot.Options;
using DSharpPlus;
using DSharpPlus.CommandsNext;
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
        private readonly BotHandlers _botHandlers;
        private readonly DiscordClient _discordClient;
        private bool _start = true;

        public BotService(ILogger<BotService> logger, IHostApplicationLifetime applicationLifetime, IOptions<DiscordOptions> discordOptions, BotHandlers botHandlers, IServiceProvider serviceProvider)
        {
            var _discordOptions = discordOptions.Value;
            _logger = logger;
            _applicationLifetime = applicationLifetime;
            _botHandlers = botHandlers;
           
            _discordClient = new(new()
            {
                MinimumLogLevel = LogLevel.Debug,
                Token = _discordOptions.Token,
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.AllUnprivileged | DiscordIntents.MessageContents
            });

            var commands = _discordClient.UseCommandsNext(new CommandsNextConfiguration()
            {
                StringPrefixes = _discordOptions.CommandPrefix,
                Services = serviceProvider
            });

            commands.RegisterCommands<CurrencyCommands>();
        }

        public async Task StartAsync(CancellationToken token)
        {
            if (_start)
                await _discordClient.ConnectAsync();

            _discordClient.GuildDownloadCompleted += _botHandlers.GuildDownloadComplete;

            // _discordClient.MessageCreated += _botHandlers.MessageCreated;

            // Other startup things here
        }

        public async Task StopAsync(CancellationToken token)
        {
            await _discordClient.DisconnectAsync();
            // More cleanup possibly here
        }
    }
}
