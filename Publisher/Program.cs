using StackExchange.Redis;
using System;

namespace Publisher
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("192.168.5.40:6379,password=666666,connectTimeout=5000,connectRetry=10,syncTimeout=10000,defaultDatabase=3");

            ISubscriber _sub = redis.GetSubscriber();


            while (true)
            {
                var msg = Console.ReadLine();
                _sub.Publish("messages", $"messages-{msg}");
                _sub.Publish("messages1", $"messages1-{msg}");
                //Console.WriteLine("ok");
            }



        }
    }
}
