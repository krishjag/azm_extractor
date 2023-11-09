using Azure.Identity;
using Azure.Monitor.Query.Models;
using Azure.Monitor.Query;
using Azure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureMonitor.Extractor
{
    internal class ScannerQueryOptions
    {
        /// <summary>
        /// The metric names to query
        /// </summary>
        public string[] MetricNames { get; set; }

        public ScannerQueryOptions(string[] metricNames)
        {
            if (metricNames.Length == 0)
                throw new ArgumentException("No metric names were supplied. Alteast 1 metric name is expected.");

            MetricNames = metricNames.ToArray();            
        }

        /// <summary>
        /// The ARM Resource ID of the resource to query metrics for.
        /// </summary>
        public string ResourceId { get; set; }

        /// <summary>
        /// The Dimension to query. Generally this is a scalar, for example for Azure Event Hubs, 
        /// an Event Hub within a given Event Hubs Namespace is identified with "EntityName" as the name of the dimension
        /// </summary>
        public string? DimensionFilter { get; set; }

        public bool SplitOnDimenions { get; set; } = true;

        MetricAggregationType _metricAggregationType = MetricAggregationType.Count;
        public MetricAggregationType MetricAggregationType 
        { 
            get { return _metricAggregationType; } 
            set { _metricAggregationType = value; } 
        }

        TimeSpan _timeRange = TimeSpan.FromMinutes(30);
        public TimeSpan TimeRange
        {
            get { return _timeRange; }
            set { _timeRange = value; }
        }
    }

    internal class MetricsScannerClient : QueryClient
    {
        public MetricsScannerClient() : base(ScannerType.Metrics)
        {
        }

        public async Task<IReadOnlyList<MetricResult>> Scan(ScannerQueryOptions scannerQueryOptions)
        {
            ScannerQueryOptions sqo = scannerQueryOptions;

            string resourceId = sqo.ResourceId;            
            string filter = string.Empty;
            if(sqo.DimensionFilter != null && sqo.DimensionFilter.Length > 0 && !sqo.DimensionFilter.Equals("*")) 
            {
                filter = sqo.DimensionFilter;
            }
            else if (sqo.SplitOnDimenions)
            {
                filter = "*";
            }
                        
            var client = metricsQueryClient;

            var options = new MetricsQueryOptions
            {
                Aggregations =
                    {
                        sqo.MetricAggregationType,
                    },
                Filter = filter,
                TimeRange = sqo.TimeRange,
            };
            Response<MetricsQueryResult> result = await client.QueryResourceAsync(
                resourceId,
                sqo.MetricNames,
                options);

            return result.Value.Metrics;
        }
    }
}
