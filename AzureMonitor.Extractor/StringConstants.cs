using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureMonitor.Extractor
{
    internal static class StringConstants
    {
        public static readonly string az_subs_key = "AZURE_SUBSCRIPTIONS";
        public static readonly string rg_query_key = "ResourceGraphInfo:GraphQuery";
        public static readonly string metric_names = "MetricNames";
        public static readonly string metric_extracting_msg = "Extracting metrics for EventHub namespace";
        public static readonly string metric_extracted_msg = "Metrics extracted for EventHub namespace";
        public static readonly string entity_name_operator = "EntityName eq";
        public static readonly string db_hostname_key = "SQLDB_HostName";
        public static readonly string db_name_key = "SQLDB_DatabaseName";
        public static readonly string db_userid_key = "SQLDB_UserID";
        public static readonly string db_pwd_key = "SQLDB_Password";
    }
}
