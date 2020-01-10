using MySql.Data.MySqlClient;
using NUnit.Framework;
using StackExchange.Redis;
using System;
using System.Threading;
using Dapper;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace RedisLock.NUnitTest
{
    public class Tests
    {
        //mysql连接
        public MySqlConnection con;
        public IDatabase redisDb;
        [SetUp]
        public void Setup()
        {
            con = new MySqlConnection("Server=192.168.5.222;Port=3306;database=jy_log_db;uid=root;pwd=000000;ConvertZeroDateTime=True;Charset=utf8;SslMode=None;AllowUserVariables=True");
            //连接redis
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("192.168.5.40:6379,password=666666,connectTimeout=5000,connectRetry=10,syncTimeout=10000,defaultDatabase=3");
            redisDb = redis.GetDatabase();
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

        /// <summary>
        /// 重置money为0
        /// </summary>
        /// <returns></returns>
        private async Task ResetMoney() {
            await con.ExecuteAsync("update aaa set money=0");
        }

        /// <summary>
        /// 查询money
        /// </summary>
        /// <returns></returns>
        private async Task<int> QueryMoney() {
          return  await con.QueryFirstAsync<int>("select money from aaa limit 1");
        }

        /// <summary>
        /// 测试一条记录(使用redis锁)
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task Test1Async()
        {
            await ResetMoney();
            List<Task> list = new List<Task>();
            var money = await QueryMoney();

            #region 测试一条记录(使用redis锁)

            Assert.AreEqual(0, money);
            await Add1Money(true);
            money = await QueryMoney();
            Assert.AreEqual(1, money);
            #endregion
             

            Assert.Pass();
        }

        /// <summary>
        /// 测试50并发(使用redis锁)
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task Test2Async()
        {
            await ResetMoney();
            List<Task> list = new List<Task>();
            var money = await QueryMoney();

            #region 测试50并发(使用redis锁)
            for (int i = 0; i < 50; i++)
            {
                list.Add(Task.Run(async () =>
                {
                    await Add1Money(true);
                }));
            }
            Task.WaitAll(list.ToArray());

            money = await QueryMoney();
            Assert.AreEqual(50, money);
            list.Clear();
            #endregion
             


            Assert.Pass();
        }

        /// <summary>
        /// 测试1000并发(使用redis锁)
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task Test3Async()
        {
            await ResetMoney();
            List<Task> list = new List<Task>();
            var money = await QueryMoney();

            #region 测试1000并发(使用redis锁)
            for (int j = 0; j < 20; j++)
            {


                for (int i = 0; i < 50; i++)
                {
                    list.Add(Task.Run(async () =>
                    {
                        await Add1Money(true);
                    }));
                }
                Task.WaitAll(list.ToArray());
                list.Clear();
            }

            money = await QueryMoney();
            Assert.AreEqual(1000, money);

            #endregion

             

            Assert.Pass();
        }


        /// <summary>
        /// 测试100并发(不使用redis锁)
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task Test4Async()
        {
            await ResetMoney();
            List<Task> list = new List<Task>();
            var money = await QueryMoney();


            #region 测试100并发(不使用redis锁)
            for (int i = 0; i < 100; i++)
            {
                var task = new Task(async () =>
                {
                    await Add1Money(false);

                });
                task.Start();
                list.Add(task);

            }

            Task.WaitAll(list.ToArray());

            money = await QueryMoney();
            Assert.AreNotEqual(1000, money);
            #endregion


            Assert.Pass();
        }

         

        public async Task Add1Money(bool useLock)
        {



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