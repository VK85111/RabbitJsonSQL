using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text.Json;
using SQL_Solution.Model;
using SQL_Solution.SQLToolsNameSpace;

namespace RabbitMQ_simple_SQL_Solution
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var factory = new ConnectionFactory() { HostName = "localhost", UserName = "Keks", Password = "Kokoko" };
            
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "FM",
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                var consumer = new EventingBasicConsumer(channel);
                
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);

                    var rootobjectjson = new RootObject();
                    rootobjectjson = JsonSerializer.Deserialize<RootObject>(message);
                    
                    SQLTools.DeleteFromMS_SQL(rootobjectjson);
                    //Можно обращаться прямо, без using в шапке
                    SQL_Solution.SQLToolsNameSpace.SQLTools.SendToMS_SQL(rootobjectjson);

                };

                channel.BasicConsume(queue: "FM", autoAck: true, consumer: consumer);
 
                Console.ReadLine();

            }

        }

    }
}
