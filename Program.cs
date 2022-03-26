using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text.Json;

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

                    var rootobjectjson = new Rootobject();
                    rootobjectjson = JsonSerializer.Deserialize<Rootobject>(message);
                    Console.WriteLine(rootobjectjson.action);
                    //Console.WriteLine(Convert.ToDateTime(rootobjectjson.timestamp));//Конвертация теперь в классе
                    Console.WriteLine(rootobjectjson.timestamp);
                    Console.WriteLine(rootobjectjson.data.LeasingCalculation);

                };

                channel.BasicConsume(queue: "FM", autoAck: true, consumer: consumer);

                Console.ReadLine();

            }

        }

        public class Rootobject
        {
            public DateTime timestamp { get; set; }
            public string action { get; set; }
            public string model { get; set; }
            public Data data { get; set; }
            public string comment { get; set; }
        }

        public class Data
        {
            public string Number { get; set; }
            public string LeasingCalculation { get; set; }
            public DateTime Date { get; set; }
            public string BldBlp { get; set; }
            public int Summary { get; set; }
        }
        public static void SendToMS_SQL(string[] Inform)
        {
            private static SqlConnection sqlConnection = null;
            sqlConnection = new SqlConnection("Data Source=DESKTOP-H7V76C2;Initial Catalog=TEST_BD;Integrated Security=True");
            sqlConnection.Open();

            SqlCommand sqlCommand = new SqlCommand("INSERT INTO [FM_data]([action],[timestamp],[LeasingCalculation],[Summary]) VALUES ('action','timestamp','LeasingCalculation','Summary')", sqlConnection);
            sqlCommand.ExecuteNonQuery();

            sqlConnection.Close();
        }
    }
}
