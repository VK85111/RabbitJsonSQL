using System;
using System.Text;
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
            using (var connection = factory.CreateConnection()) //Простыми словами в понятиях 1С про using?
            using (var channel = connection.CreateModel()) //Я так понял это из-за connection в котором сидит channel
            //Почему не просто var channel = connection.CreateModel() в стиле 1С?
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
                    //Console.WriteLine(Convert.ToDateTime(rootobjectjson.timestamp));//Конвертация теперь в классе
                    
                    //string[] arrayToSQL = new string[4];//Как сделать массив с разными типами внутри?
                    ////Видимо никак
                    //arrayToSQL[0] = rootobjectjson.action;
                    //arrayToSQL[1] = "tmstmp";//rootobjectjson.timestamp; 
                    //arrayToSQL[2] = rootobjectjson.data.leasingcalculation;
                    //arrayToSQL[3] = "5000";//rootobjectjson.data.summary;

                    SQL_tools.DeleteFromMS_SQL(rootobjectjson);
                    SQL_tools.SendToMS_SQL(rootobjectjson);

                };

                channel.BasicConsume(queue: "FM", autoAck: true, consumer: consumer);

                //Console.WriteLine(consumer.Model); //а где ea и почему M у Model большая?
                
                Console.ReadLine();

            }

        }
        //Имеет ли смысл вывести эти сущности (типа метаданные справочники) в отдельные *.cs файлы? Как принято?
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
            public string GUID { get; set; }
            public string number { get; set; }
            public string leasingcalculation { get; set; }
            public DateTime date { get; set; }
            public string bldblp { get; set; }
            public decimal summary { get; set; }

        }

        public static class SQL_tools //Может это уже новый Namespace? Типа своя подсистема.
        {
            public static void DeleteFromMS_SQL(Rootobject args)
            {   
                //Копипаста, вывести в отдельную процедуру
                SqlConnection sqlConnection = null;
                sqlConnection = new SqlConnection(GetConnectionStringByName("SQL_URL"));
                sqlConnection.Open();

                SqlCommand sqlCommand = new SqlCommand("DELETE FROM [FM_data] WHERE[GUID] = @GUID", sqlConnection);
                sqlCommand.Parameters.AddWithValue("@GUID", args.data.GUID);
                sqlCommand.ExecuteNonQuery();

            }
            public static void SendToMS_SQL(Rootobject args)
            {
                SqlConnection sqlConnection = null;
                sqlConnection = new SqlConnection(GetConnectionStringByName("SQL_URL"));
                sqlConnection.Open();

                //Получить объяснение NuGet package, почему так по разному подключаются библиотеки?

                //SqlCommand sqlCommand0 = new SqlCommand("DELETE FROM [FM_data] WHERE[GUID] = @GUID", sqlConnection);
                ////Почему не переопределилась sqlCommand?
                ////Можно ли сделать одну сущность sqlCommand, но менять только текст запроса? Чтоб параметры остались.
                //sqlCommand0.Parameters.AddWithValue("@GUID", args.data.GUID);
                //sqlCommand0.ExecuteNonQuery(); //Альтернативы этого NonQuery?

                SqlCommand sqlCommand = new SqlCommand($"INSERT INTO [FM_data]([GUID], [action], [timestamp], [number], " +
                    $"[leasingcalculation], [date], [bldblp], [summary]) " +
                    $"VALUES (@GUID, @action, @timestamp, @number, @leasingcalculation, @date, @bldblp, @summary)", sqlConnection);
                //sqlCommand.Parameters.AddRange(parameters); //Нужен пример этого исполнения
                sqlCommand.Parameters.AddWithValue("@GUID", args.data.GUID);
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
