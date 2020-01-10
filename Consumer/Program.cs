using StackExchange.Redis;
using System;

namespace Consumer
{
    class Program
    {
        static void Main(string[] args)
        {

            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("192.168.5.40:6379,password=666666,connectTimeout=5000,connectRetry=10,syncTimeout=10000,defaultDatabase=3");

            ISubscriber _sub = redis.GetSubscriber();

            Console.WriteLine("启动消费者");
            Console.WriteLine("订阅messages");

            _sub.Subscribe("messages").OnMessage(channelMessage =>
            {
                Console.WriteLine((string)channelMessage.Message);
            });

            Console.WriteLine("订阅messages1");

            _sub.Subscribe("messages1").OnMessage(channelMessage =>
            {
                Console.WriteLine((string)channelMessage.Message);
            });

            Console.ReadKey();
        }
    }
}
