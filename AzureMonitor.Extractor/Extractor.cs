using Azure.Identity;
using Azure.Monitor.Query.Models;
using Azure.ResourceManager;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureMonitor.Extractor
{
    public class Extractor
    {
        public static void Extract(ILogger logger, IConfiguration configuration, CancellationToken stoppingToken)
        {                        
            IList<string> subscriptions = new List<string>() { configuration[StringConstants.az_subs_key] };            
            string resourceGraphQuery = configuration[StringConstants.rg_query_key];                                    
            string[] metricNames = configuration[StringConstants.metric_names].Split(',');

            ArmClient armClient = new ArmClient(new DefaultAzureCredential());
            ResourceGraphClient client = new ResourceGraphClient(armClient, subscriptions);
            var resourceGraphQueryResults = client.FetchResources(resourceGraphQuery).Result;//.Take(1).ToList(); //Used when testing;

            Task[] apiInvocationTasks = new Task[resourceGraphQueryResults.Count];

            for (int i = 0; i < apiInvocationTasks.Length; i++)
            {
                if (stoppingToken.IsCancellationRequested)
                    return;

                var eventHubResource = resourceGraphQueryResults[i];

                apiInvocationTasks[i] = Task.Run(() =>
                {
                    logger.LogInformation($"{StringConstants.metric_extracting_msg} {eventHubResource.name}");

                    MetricResult[] metrics = FetchMetricResults(armClient, eventHubResource, metricNames);

                    string saveToTableToken = configuration["SQLDB_SaveToTable"];
                    bool saveToTable = false;

                    if(bool.TryParse(saveToTableToken, out saveToTable) && saveToTable)
                    {
                        new SqlDBHelper(logger, configuration).SaveMetricsToDatabase(new Tuple<MetricResult[], string, string>(metrics, eventHubResource.id, eventHubResource.name));
                    }                    

                    logger.LogInformation($"{StringConstants.metric_extracted_msg} {eventHubResource.name}.");
                });
            }

            Task.WaitAll(apiInvocationTasks);            
        }

        static MetricResult[] FetchMetricResults(ArmClient armClient, ResourceGraphResponseModel eventHubResource, string[] metricNames)
        {
            List<MetricResult> results = new List<MetricResult>();

            /*Azure Monitor only returns data for 1 metric when fetching data for all dimensions of a metric.
             * Let's take an example.
             * 
             * Event Hub Namespace: 
             *      some-namespace-eastus-dev-hub1
             * Event Hubs in this Namespace (Dimensions):
             *      somehub1
             *      somehub2
             *      somehub3             
             * Event Hub Metrics:
             *      SuccessfulRequests,
             *      ServerErrors,
             *      UserErrors,
             *      QuotaExceededErrors,
             *      ThrottledRequests,
             *      IncomingRequests,
             *      IncomingMessages,
             *      OutgoingMessages,
             *      IncomingBytes,
             *      OutgoingBytes,
             *      ActiveConnections,
             *      ConnectionsOpened,
             *      ConnectionsClosed,
             *      CaptureBacklog,
             *      CapturedMessages,
             *      CapturedBytes,
             *      Size    
             *
             * When splitting by dimension, that is by setting the $filter parameter to [EntityName eq '*'], where * represents all dimensions or hubs,
             * then Azure Monitor can return only 1 metric. But if you set the $filter parameter to specific hub, say [EntityName eq 'cdc.cdc-aggregated-v1-json.dlq'],
             * then you can supply multiple metric names as comma-separated values. ex. IncomingMessage,OutgoingMessages
             * 
             * Since we always split by dimension and iterate through each metric, the algorithmic time is always O(n) for a given metric.
            */
            foreach (var metric in metricNames)
            {
                string eventHubNamespaceResourceId = eventHubResource.id;
                MetricsScannerClient metricsScannerClient = new MetricsScannerClient();
                ScannerQueryOptions scannerQueryOptions = new ScannerQueryOptions(new[] { metric } );
                scannerQueryOptions.DimensionFilter = $"{StringConstants.entity_name_operator} '*'";
                scannerQueryOptions.ResourceId = eventHubNamespaceResourceId;
                var metricResponse = metricsScannerClient.Scan(scannerQueryOptions).Result.ToArray();
                results.AddRange(metricResponse);
            }

            return results.ToArray();
        }
    }
}
