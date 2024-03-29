﻿using DiscordBot;
using DiscordBot.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


var app = Host.CreateDefaultBuilder()
    // Add this later if doesn't work after publishing executable
    // .UseContentRoot(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))
    .UseConsoleLifetime()
    .ConfigureServices((hostContext, services) =>
    {
        // DI container
        services
            .AddHostedService<BotService>()
            .AddSingleton<Mongo>()
            .AddSingleton<BotHandlers>()
            .AddSingleton<MemoryStore>();



        // Options
        services.AddOptions<DiscordOptions>()
            .Bind(hostContext.Configuration.GetSection(nameof(DiscordOptions)));
        services.AddOptions<MongoOptions>()
            .Bind(hostContext.Configuration.GetSection(nameof(MongoOptions)));
    });



await app.RunConsoleAsync();
    