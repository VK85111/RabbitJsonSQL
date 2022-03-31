using System.Configuration;
using System.Data.SqlClient;

namespace SQL_Solution.SQLToolsNameSpace
{
    public static class SQLTools
    {
        public static void DeleteFromMS_SQL(SQL_Solution.Model.RootObject args)
        {
            SqlConnection sqlConnection = null;
            using (sqlConnection = new SqlConnection(GetConnectionStringByName("SQL_URL")))
            {
                sqlConnection.Open();
                SqlCommand sqlCommand = new SqlCommand("DELETE FROM [FM_data] WHERE[GUID] = @GUID", sqlConnection);
                SqlParameter sqlParameter = sqlCommand.Parameters.AddWithValue("@GUID", args.data.GUID);
                sqlCommand.ExecuteNonQuery();
            }


        }
        public static void SendToMS_SQL(SQL_Solution.Model.RootObject args)
        {
            SqlConnection sqlConnection = null;
            sqlConnection = new SqlConnection(GetConnectionStringByName("SQL_URL"));
            sqlConnection.Open();

            SqlCommand sqlCommand = new SqlCommand($"INSERT INTO [FM_data]([GUID], [action], [timestamp], [number], " +
                $"[leasingcalculation], [date], [bldblp], [summary]) " +
                $"VALUES (@GUID, @action, @timestamp, @number, @leasingcalculation, @date, @bldblp, @summary)", sqlConnection);
            sqlCommand.Parameters.AddWithValue("@GUID", args.data.GUID);
            sqlCommand.Parameters.AddWithValue("@action", args.action);
            sqlCommand.Parameters.AddWithValue("@timestamp", args.timestamp);
            sqlCommand.Parameters.AddWithValue("@leasingcalculation", args.data.leasingcalculation);
            sqlCommand.Parameters.AddWithValue("@number", args.data.number);
            sqlCommand.Parameters.AddWithValue("@date", args.data.date);
            sqlCommand.Parameters.AddWithValue("@bldblp", args.data.bldblp);
            sqlCommand.Parameters.AddWithValue("@summary", args.data.summary);
            sqlCommand.ExecuteNonQuery();

            sqlConnection.Close();//Можно без Close но с Using() Очистка мусора

        }

        private static string GetConnectionStringByName(string name)
        {
            string returnValue = null;

            ConnectionStringSettings settings =
                ConfigurationManager.ConnectionStrings[name];

            if (settings != null)
                returnValue = settings.ConnectionString;

            return returnValue;
        }
    }
}
