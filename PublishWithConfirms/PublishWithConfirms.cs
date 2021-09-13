using RabbitMQ.Client;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace PublishWithConfirms
{
    class PublishWithConfirms
    {
        private static readonly ConcurrentDictionary<ulong, string> outstandingConfirms = new ConcurrentDictionary<ulong, string>();

        static void cleanOutstandingConfirms(ulong sequenceNumber, bool multiple)
        {
            if (multiple)
            {
                Console.WriteLine("Multiple");
                var confirmed = outstandingConfirms.Where(k => k.Key <= sequenceNumber);
                Console.WriteLine(confirmed.Count());
                foreach (var entry in confirmed)
                {
                    var removed = outstandingConfirms.TryRemove(entry.Key, out _);
                    Console.WriteLine($"Confirmed: {entry.Key} {removed}");
                }
            }
            else
            {
                var removed = outstandingConfirms.TryRemove(sequenceNumber, out _);
                Console.WriteLine($"Single\nConfirmed: {sequenceNumber} {removed}");
            }
        }

        static void Main(string[] args)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ConfirmSelect();
                channel.QueueDeclare(queue: "task_queue",
                     durable: true,
                     exclusive: false,
                     autoDelete: false,
                     arguments: null);

                var properties = channel.CreateBasicProperties();
                //properties.Persistent = true;

                channel.BasicAcks += (sender, ea) => cleanOutstandingConfirms(ea.DeliveryTag, ea.Multiple);

                channel.BasicNacks += (sender, ea) =>
                {
                    outstandingConfirms.TryGetValue(ea.DeliveryTag, out string body);
                    Console.WriteLine($"Message with body {body} has been nack-ed. Sequence number: {ea.DeliveryTag}, multiple: {ea.Multiple}");
                    cleanOutstandingConfirms(ea.DeliveryTag, ea.Multiple);
                };

                foreach(var arg in args)
                {
                    outstandingConfirms.TryAdd(channel.NextPublishSeqNo, arg);
                    
                    var body = System.Text.Encoding.UTF8.GetBytes(arg);
                    channel.BasicPublish(exchange: "",
                                     routingKey: "task_queue",
                                     basicProperties: properties,
                                     body: body);

                    Console.WriteLine(" [x] Sent {0}", arg);
                }
            }
            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }
    }
}
