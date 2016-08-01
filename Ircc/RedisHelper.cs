﻿using System;
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
        private RedisKey USERS = "Users";
        private RedisKey RANKINGS = "Rankings";
        private RedisKey CURRENTUSERS = "CurrentUsers";
        private RedisKey nextUserId = "nextUserId";
        private string userPrefix = "user:";
        
        public RedisHelper(ConfigurationOptions configOptions)
        {
            if (null == configOptions)
                throw new ArgumentNullException("configOptions");
            configurationOptions = configOptions;
        }

        public long SignIn(string username, string password)
        {
            RedisValue userId = Database.HashGet(USERS, username);
            if (userId.IsNull)
                throw new Exception("Error: Sign in failed.");

            string realPassword = (string)Database.HashGet(userPrefix + (long)userId, "password");
            if (password == realPassword)
                throw new Exception("Error: Sign in failed.");

            Database.SetAdd(CURRENTUSERS, userId);
            return (long)userId;
        }

        public void SignOut(long userId)
        {
            if (!Database.SetRemove(CURRENTUSERS, userId))
                throw new Exception("Error: not in set");
        }

        public long CreateUser(string username, string password, bool isDummy, int chatCount)
        {
            if (Database.KeyExists(username))
                throw new Exception("Error: Sign up failed.");

            long userId = Database.StringIncrement(nextUserId);
            RedisKey user = userPrefix + userId;
            
            // create user's hashset
            HashEntry[] userData = { new HashEntry("username", username),
                                     new HashEntry("password", password),
                                     new HashEntry("isDummy", isDummy ? 1 : 0),
                                     new HashEntry("chatCount", chatCount) };
            
            Database.HashSet(user, userData);

            // add to "users" hashset (username to id mapping)
            HashEntry[] usernameMapping = { new HashEntry(username, userId) };
            Database.HashSet(USERS, usernameMapping);

            // add to ranking sorted set
            Database.SortedSetAdd(RANKINGS, userId, chatCount);

            return userId;
        }
        
        public void UpdateUser(long userId, HashEntry[] userInfo)
        {
            Database.HashSet(userPrefix + userId, userInfo);

            for(int i = 0; i < userInfo.Length; i++)
            {
                if(userInfo[i].Name == "chatCount")
                {
                    Database.SortedSetAdd(RANKINGS, userId, (int)userInfo[i].Value);
                    break;
                }
            }
        }

        public void DestroyUser(long userId)
        {
            string username = (string)Database.HashGet(userPrefix + (long)userId, "username");
            Database.KeyDelete(userPrefix + userId);
            Database.HashDelete(USERS, username);
            Database.SortedSetRemove(RANKINGS, userId);
        }

        public void GetAllTimeRankings(int endRank)
        {
            Database.SortedSetRangeByRank(RANKINGS, 0, endRank, Order.Descending);
        }

        public void GetAllTimeRankings(int startRank, int endRank)
        {
            Database.SortedSetRangeByRank(RANKINGS, startRank, endRank, Order.Descending);
        }

        /*
        public static HashEntry[] MakeHashEntry()
        {

        }

        
        public static void Update<T>(this T instance)
        {
            string jsonString = instance.ToString();

            Database.HashSet();

        }
        
        public void ClearItem(string key)
        {
            if (key == null)
                throw new ArgumentNullException("key");
            Database.KeyDelete(key);
        }
        */

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

    }
}