using Azure;
using Azure.Identity;
using Azure.Monitor.Query.Models;
using Azure.ResourceManager;
using Azure.ResourceManager.EventHubs;
using Azure.ResourceManager.ResourceGraph;
using Azure.ResourceManager.ResourceGraph.Models;
using Microsoft.Identity.Client;
using System.ComponentModel;
using System.Data;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json;
using System.Text.Json.Nodes;

using Microsoft.Data.SqlClient;
using System.Data.Common;
using System.ComponentModel.DataAnnotations;

namespace AzureMonitor.Extractor
{
    internal class ResourceGraphClient
    {
        ArmClient _armClient = null;
        IList<string> _subcriptions = new List<string>();
        public ResourceGraphClient(ArmClient armClient, IList<string> subscriptions)
        {
            _armClient = armClient;
            _subcriptions = subscriptions;
        }

        public async Task<List<ResourceGraphResponseModel>> FetchResources(string graphQuery)
        {
            var tenantResource = _armClient.GetTenants().GetAll();
            var tenant = tenantResource.FirstOrDefault();
            
            ResourceQueryContent content = new ResourceQueryContent(graphQuery);
            foreach (String subscription in _subcriptions)
                content.Subscriptions.Add(subscription);          

            ResourceQueryResult resourceQueryResult = tenant.GetResourcesAsync(content).Result;
                       
            var resourceGraphResponse = await JsonSerializer.DeserializeAsync<List<ResourceGraphResponseModel>>(resourceQueryResult.Data.ToStream());

            return resourceGraphResponse;
        }
    }
}
