using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace Ircc
{
    class RedisHelper
    {
        private static ConfigurationOptions configurationOptions;
        
        public RedisHelper(ConfigurationOptions configOptions)
        {
            if (null == configOptions)
                throw new ArgumentNullException("configOptions");
            configurationOptions = configOptions;
        }

        private static IDatabase Database
        {
            get
            {
                return Connection.GetDatabase();
            }
        }

        private static ConnectionMultiplexer Connection
        {
            get
            {
                return LazyConnection.Value;
            }
        }

        private static readonly Lazy<ConnectionMultiplexer> LazyConnection
            = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(configurationOptions));

        public void ClearItem(string key)
        {
            if (key == null)
                throw new ArgumentNullException("key");
            Database.KeyDelete(key);
        }
    }
}
