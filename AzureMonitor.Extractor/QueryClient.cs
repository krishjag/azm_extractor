using Azure.Identity;
using Azure.Monitor.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureMonitor.Extractor
{
    public enum ScannerType
    {
        Logs,
        Metrics
    }

    internal class QueryClient
    {
        internal LogsQueryClient? logsQueryClient;
        internal MetricsQueryClient? metricsQueryClient;
        public QueryClient(ScannerType scannerType) {

            switch (scannerType)
            {
                case ScannerType.Logs:
                    logsQueryClient = new LogsQueryClient(new DefaultAzureCredential()); break;
                case ScannerType.Metrics:
                    metricsQueryClient = new MetricsQueryClient(new DefaultAzureCredential()); break;
                default: break;
            }
        }
    }
}
