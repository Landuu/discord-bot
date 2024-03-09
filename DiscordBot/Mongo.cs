using DiscordBot.Models;
using DiscordBot.Options;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot
{
    internal class Mongo
    {
        public MongoClient Client { get; private set; }

        private readonly MongoOptions _options;

        public Mongo(IOptions<MongoOptions> mongoOptions)
        {
            _options = mongoOptions.Value;

            var settings = MongoClientSettings.FromConnectionString(_options.ConnectionString);
            settings.ServerApi = new ServerApi(ServerApiVersion.V1);
            Client = new(settings);
        }

        public IMongoDatabase GetDb()
        {
            return Client.GetDatabase(_options.DatabaseName);
        }

        public IMongoCollection<T> GetCollection<T>()
        {
            string collectionName;
            if (typeof(T) == typeof(DbServer))
                collectionName = "servers";
            else
                throw new ArgumentException("TYPE");

            return GetDb().GetCollection<T>(collectionName);
        }

        public async Task<DbServer?> GetDbServer(ulong? id)
        {
            return await GetCollection<DbServer>()
                .Find(x => x.ServerId == id)
                .FirstOrDefaultAsync();
        }
    }
}
