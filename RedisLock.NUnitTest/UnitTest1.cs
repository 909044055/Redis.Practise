using MySql.Data.MySqlClient;
using NUnit.Framework;
using StackExchange.Redis;
using System;
using System.Threading;
using Dapper;
using System.Threading.Tasks;

namespace RedisLock.NUnitTest
{
    public class Tests
    {
        public MySqlConnection con;

        [SetUp]
        public void Setup()
        {
            con = new MySqlConnection("Server=192.168.5.222;Port=3306;database=jy_log_db;uid=root;pwd=000000;ConvertZeroDateTime=True;Charset=utf8;SslMode=None;AllowUserVariables=True");

            con.Open();
            con.Execute(@"CREATE TABLE IF NOT EXISTS `aaa` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `money` int(11) NOT NULL DEFAULT 0,
  PRIMARY KEY(`id`)
) ENGINE = InnoDB AUTO_INCREMENT = 2 DEFAULT CHARSET = utf8; 
truncate table aaa;
INSERT INTO `aaa` VALUES ('1', '0');
");




        }

        [Test]
        public async Task Test1Async()
        {
            var money = con.QueryFirst<int>("select money from aaa limit 1");
            Assert.AreEqual(0, money);
            await Add1Money(true);
            money = con.QueryFirst<int>("select money from aaa limit 1");
            Assert.AreEqual(1, money);


            await Task.Run(async () =>
            {

                for (int i = 0; i < 50; i++)
                {
                    await Add1Money(true);
                }

            });

            money = con.QueryFirst<int>("select money from aaa limit 1");
            Assert.AreEqual(51, money);


            await Task.Run(async () =>
            {
                for (int j = 0; j < 20; j++)
                {
                    for (int i = 0; i < 50; i++)
                    {
                        await Add1Money(true);
                    }

                }
            });
            money = con.QueryFirst<int>("select money from aaa limit 1");
            Assert.AreEqual(1051, money);


            await Task.Run(async () =>
            {

                for (int i = 0; i < 100; i++)
                {
                    await Add1Money(false);
                }

            });
            money = con.QueryFirst<int>("select money from aaa limit 1");
            Assert.AreNotEqual(1151, money);

            Assert.Pass();
        }


        public async Task Add1Money(bool useLock)
        {
            //连接redis
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("192.168.5.40:6379,password=666666,connectTimeout=5000,connectRetry=10,syncTimeout=10000,defaultDatabase=3");
            var redisDb = redis.GetDatabase();


            var guid = Guid.NewGuid().ToString();


            if (useLock)
            {
                //重试次数
                int retry = 10000;
                while (!await redisDb.LockTakeAsync("add_money_user001", guid, TimeSpan.FromSeconds(20)) && retry > 0)
                {
                    Thread.Sleep(10);
                    retry--;
                }
            }

            try
            {
                MySqlConnection connection = new MySqlConnection("Server=192.168.5.222;Port=3306;database=jy_log_db;uid=root;pwd=000000;ConvertZeroDateTime=True;Charset=utf8;SslMode=None;AllowUserVariables=True");

                connection.Open();

                var money = connection.QueryFirst<int>("select money from aaa limit 1");
                money = money + 1;

                await connection.ExecuteAsync("update aaa set money=@money", new { money });
                connection.Close();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                await redisDb.LockReleaseAsync("add_money_user001", guid);
            }
        }

    }
}