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
            //Как хранить эту строку с настройками в App.config?
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "FM",
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                var consumer = new EventingBasicConsumer(channel);
                //почему тип не задали для model, ea?
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);

                    var rootobjectjson = new Rootobject();
                    rootobjectjson = JsonSerializer.Deserialize<Rootobject>(message);
                    Console.WriteLine(rootobjectjson.action);
                    //Console.WriteLine(Convert.ToDateTime(rootobjectjson.timestamp));//Конвертация теперь в классе
                    Console.WriteLine(rootobjectjson.timestamp);
                    Console.WriteLine(rootobjectjson.data.summary);
                    Console.WriteLine(rootobjectjson.data.leasingcalculation);

                    //string[] arrayToSQL = new string[4];//Как сделать массив с разными типами внутри?
                    ////Видимо никак
                    //arrayToSQL[0] = rootobjectjson.action;
                    //arrayToSQL[1] = "tmstmp";//rootobjectjson.timestamp; 
                    //arrayToSQL[2] = rootobjectjson.data.leasingcalculation;
                    //arrayToSQL[3] = "5000";//rootobjectjson.data.summary;

                    SQL_tools.SendToMS_SQL(rootobjectjson);

                };

                channel.BasicConsume(queue: "FM", autoAck: true, consumer: consumer);

                Console.WriteLine(consumer.Model); //а где ea и почему M большая?
                
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
            public string number { get; set; }
            public string leasingcalculation { get; set; }
            public DateTime date { get; set; }
            public string bldblp { get; set; }
            public decimal summary { get; set; }

        }

        public static class SQL_tools
        {
            public static void SendToMS_SQL(Rootobject args)
            {
                SqlConnection sqlConnection = null;
                sqlConnection = new SqlConnection(GetConnectionStringByName("SQL_URL"));
                sqlConnection.Open();

                SqlCommand sqlCommand = new SqlCommand($"INSERT INTO [FM_data]([action], [timestamp], [number], " +
                    $"[leasingcalculation], [date], [bldblp], [summary]) " +
                    $"VALUES (@action, @timestamp, @number, @leasingcalculation, @date, @bldblp, @summary)", sqlConnection);
                //sqlCommand.Parameters.AddRange(parameters); //Нужен пример этого исполнения
                sqlCommand.Parameters.AddWithValue("@action", args.action);
                sqlCommand.Parameters.AddWithValue("@timestamp", args.timestamp);
                sqlCommand.Parameters.AddWithValue("@leasingcalculation", args.data.leasingcalculation);
                sqlCommand.Parameters.AddWithValue("@number", args.data.number);
                sqlCommand.Parameters.AddWithValue("@date", args.data.date);
                sqlCommand.Parameters.AddWithValue("@bldblp", args.data.bldblp);
                sqlCommand.Parameters.AddWithValue("@summary", args.data.summary);
                sqlCommand.ExecuteNonQuery();

                sqlConnection.Close();

            }

            private static string GetConnectionStringByName(string name)
            {
                // Assume failure.
                string returnValue = null;

                // Look for the name in the connectionStrings section.
                ConnectionStringSettings settings =
                    ConfigurationManager.ConnectionStrings[name];

                // If found, return the connection string.
                if (settings != null)
                    returnValue = settings.ConnectionString;

                return returnValue;
            }
        }
    }
}
