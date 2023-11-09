using Azure.Monitor.Query.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;

namespace AzureMonitor.Extractor
{
    public class SqlDBHelper
    {
        static ILogger? _logger;
        static IConfiguration _configuration;
        public SqlDBHelper(ILogger logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        string GetSqlDatabaseConnectionString()
        {
            SqlConnectionStringBuilder sqlConnectionStringBuilder = new SqlConnectionStringBuilder();
            sqlConnectionStringBuilder.DataSource = _configuration[StringConstants.db_hostname_key];
            sqlConnectionStringBuilder.InitialCatalog = _configuration[StringConstants.db_name_key];
            sqlConnectionStringBuilder.UserID = _configuration[StringConstants.db_userid_key];
            sqlConnectionStringBuilder.Password = _configuration[StringConstants.db_pwd_key];

            return sqlConnectionStringBuilder.ToString();
        }

        public void SaveMetricsToDatabase(Tuple<MetricResult[], String, String> metricResults)
        {
            string insertCommand = @"INSERT INTO [dbo].[EventHub_Metrics]
                                           ([eventhub_namespace_id]
                                           ,[eventhub_namespace_name]
                                           ,[entity_name]
                                           ,[time_stamp]
                                           ,[metric_name]
                                           ,[aggregation_type]
                                           ,[metric_value]
                                           ,[insert_time_stamp])
                                     VALUES
                                           (@eventhub_namespace_id
                                           ,@eventhub_namespace_name
                                           ,@entity_name
                                           ,@time_stamp
                                           ,@metric_name
                                           ,@aggregation_type
                                           ,@metric_value
                                           ,@insert_time_stamp)";

            using (SqlConnection connection = new SqlConnection(GetSqlDatabaseConnectionString()))
            {
                if (connection.State != ConnectionState.Open)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        try
                        {
                            connection.Open();
                            break;
                        }
                        catch (Exception ex)
                        {
                            Thread.Sleep(1000);
                        }
                    }
                }
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    using (SqlCommand command = new SqlCommand())
                    {
                        command.CommandText = insertCommand;
                        command.CommandType = CommandType.Text;
                        command.Connection = connection;
                        command.Transaction = transaction;

                        try
                        {
                            int recordCounter = 0;
                            var metricResultTuple = metricResults;
                            _logger.LogInformation($"Saving metrics to database for {metricResultTuple.Item3}");
                            foreach (var metricResult in metricResultTuple.Item1)
                                foreach (MetricTimeSeriesElement mtsElement in metricResult.TimeSeries)
                                    foreach (MetricValue value in mtsElement.Values)
                                    {
                                        recordCounter++;
                                        command.Parameters.Clear();
                                        command.Parameters.AddWithValue("@eventhub_namespace_id", metricResultTuple.Item2);
                                        command.Parameters.AddWithValue("@eventhub_namespace_name", metricResultTuple.Item3);
                                        command.Parameters.AddWithValue("@entity_name", mtsElement.Metadata["EntityName"]);
                                        command.Parameters.AddWithValue("@time_stamp", value.TimeStamp);
                                        command.Parameters.AddWithValue("@metric_name", metricResult.Name);
                                        command.Parameters.AddWithValue("@aggregation_type", "count");
                                        command.Parameters.AddWithValue("@metric_value", value.Count);
                                        command.Parameters.AddWithValue("@insert_time_stamp", DateTimeOffset.Now);
                                        command.ExecuteNonQuery();                                                                                
                                    }                            
                            transaction.Commit();
                            _logger.LogInformation($"{recordCounter} records added");
                        }
                        catch (Exception ex)
                        {                            
                            transaction.Rollback();
                            _logger.LogInformation(ex.Message);
                            _logger.LogInformation("Transaction rolled back.");
                        }
                    }
                }
            }
        }
    }
}