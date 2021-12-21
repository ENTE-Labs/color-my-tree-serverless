using System;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace ColorMyTree.Services
{

    public class CacheConnectionService
    {
        private readonly Lazy<ConnectionMultiplexer> _lazyConnection;

        public CacheConnectionService(IConfiguration config)
        {
            var connectionString = config.GetConnectionString("CacheConnection");
            _lazyConnection = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(connectionString));
        }

        public IDatabase GetDatabase() => _lazyConnection.Value.GetDatabase();
    }
}
