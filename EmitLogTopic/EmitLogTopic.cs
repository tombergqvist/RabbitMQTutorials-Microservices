using RabbitMQ.Client;
using System;
using System.Linq;

namespace EmitLogTopic
{
    class EmitLogTopic
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: "topic_logs", type: "topic");

                var routingKey = (args.Length > 0) ? args[0] : "anonymous.info";
                string message = Message(args);
                var body = System.Text.Encoding.UTF8.GetBytes(message);

                channel.BasicPublish(exchange: "topic_logs",
                                     routingKey: routingKey,
                                     basicProperties: null,
                                     body: body);

                Console.WriteLine(" [x] Sent '{0}':'{1}'", routingKey, message);
            }
        }

        private static string Message(string[] args)
        {
            return (args.Length > 1)
                          ? string.Join(" ", args.Skip(1).ToArray())
                          : "Hello World!";
        }
    }
}
