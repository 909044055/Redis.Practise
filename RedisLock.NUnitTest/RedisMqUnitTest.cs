using NUnit.Framework;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Redis.NUnitTest
{
    public class RedisMqUnitTest
    {
        private IDatabase _redisDb;
        private ISubscriber _sub;
        [SetUp]
        public void Setup()
        {
            //连接redis
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("192.168.5.40:6379,password=666666,connectTimeout=5000,connectRetry=10,syncTimeout=10000,defaultDatabase=3");
            _redisDb = redis.GetDatabase();
            _sub = redis.GetSubscriber();
        }


        [Test]
        public async Task Test1()
        {
           

            Assert.Pass();
        }

        [Test]
        public async Task Test2()
        {

           

            Assert.Pass();
        }

    }
}
