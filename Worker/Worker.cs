using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;

namespace Worker
{
    class Worker
    {
        static void Main(string[] args)
        {
            var host = Environment.GetEnvironmentVariable("hostname");
            var factory = new ConnectionFactory() { HostName = host };

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "task_queue",
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

                channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (sender, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = System.Text.Encoding.UTF8.GetString(body);

                    int dots = message.Split('.').Length - 1;
                    Console.Write("Received: {0}", message.Trim('.'));

                    for(int i=0; i<dots; i++)
                    {
                        System.Threading.Thread.Sleep(1000);
                        Console.Write(".");
                    }
                    Console.WriteLine();

                    // Note: it is possible to access the channel via
                    //       ((EventingBasicConsumer)sender).Model here
                    channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                };

                channel.BasicConsume(queue: "task_queue",
                                     autoAck: false,
                                     consumer: consumer);

                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();
            }
        }
    }
}