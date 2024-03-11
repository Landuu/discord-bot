using DiscordBot.Models;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using MongoDB.Driver;
using MongoDB.Driver.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Commands
{
    internal class CurrencyCommands : BaseCommandModule
    {
        public required Mongo Mongo { private get; set; }

        [Command]
        public async Task Zmierzmi(CommandContext ctx)
        {
            string? param = null;
            if(param == null)
            {
                var srv = await Mongo.GetDbServer(ctx.Guild.Id);
                if(!srv.RegisteredUsers.Contains(ctx.Member.Id))
                {
                    srv.RegisteredUsers.Add(ctx.Member.Id);
                    var updateFilter = Builders<DbServer>.Filter.Eq(s => s.Id, srv.Id);
                    var update = Builders<DbServer>.Update.Set(s => s.RegisteredUsers, srv.RegisteredUsers);
                    await Mongo.GetCollection<DbServer>().UpdateOneAsync(updateFilter, update);
                    await ctx.RespondAsync($"📜🍆");
                }
                else
                {
                    await ctx.RespondAsync("Jesteś już w programie 📏🍆");
                }
            }
            else if(param != null && param.Equals("off", StringComparison.InvariantCultureIgnoreCase))
            {
                var srv = await Mongo.GetDbServer(ctx.Guild.Id);
                srv.RegisteredUsers.Remove(ctx.Member.Id);
                var updateFilter = Builders<DbServer>.Filter.Eq(s => s.Id, srv.Id);
                var update = Builders<DbServer>.Update.Set(s => s.RegisteredUsers, srv.RegisteredUsers);
                await Mongo.GetCollection<DbServer>().UpdateOneAsync(updateFilter, update);
                await ctx.RespondAsync($"❌📜");
            }
            else
            {
                await ctx.RespondAsync("💀 Nieprawidłowe argumenty, wpisz !zmierzmi off, aby wyłączyć mierzenie");
            }
        }

        [Command]
        public async Task Halo(CommandContext ctx)
        {
            var serverId = ctx.Message.Channel.GuildId;
            var server = await Mongo.GetDbServer(serverId);

            if (server == null)
            {
                await ctx.Message.RespondAsync("<:madge:797551865675644949>");
                return;
            }

            var usersInChannels = new Dictionary<string, IEnumerable<string>>();
            var voiceChannels = server.Channels.Where(x => x.IsVoice);
            foreach (var channel in voiceChannels)
            {
                var currentChannel = await ctx.Client.GetChannelAsync(channel.ChannelId);
                var users = currentChannel.Users;
                if (users.Count > 0)
                    usersInChannels.Add(channel.Name, users.Select(x => x.DisplayName));
            }

            if (usersInChannels.Count > 0)
            {
                string msg = "NIECH TO PIERON STRZELI! KTOŚ JEST NA DISCORDZIE NIEMOŻLIWE <:peepoG:757657584508469329>:";
                foreach (var uic in usersInChannels)
                    msg += $"\n - {uic.Key}: {string.Join(", ", uic.Value)}";
                await ctx.RespondAsync(msg);
            }
            else
            {
                await ctx.RespondAsync("Nie ma nikogo na discordzie, chyba każdy znalazł sobie lepszych kolegów <:sadge:753604078499790849>");
            }
        }
    }
}
