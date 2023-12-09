using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Cosmos;
using System.Collections.Generic;
using System.Linq;

namespace ExampleExam.Triggers;
public class GetChildrenTrigger
{
    [FunctionName("GetChildrenTrigger")]
    public async Task<IActionResult> GetChildren(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "children")] HttpRequest req,
        ILogger log)
    {
        Container container = GetCosmosDBContainer("cloud services", "Children");

        var query = new QueryDefinition("SELECT * FROM c");
        var iterator = container.GetItemQueryIterator<dynamic>(query);

        List<Child> children = new();

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            Child child = JsonConvert.DeserializeObject<Child>(response.Resource.FirstOrDefault().ToString());
            children.Add(child ?? null);
        }

        return new OkObjectResult(children);
    }

    private Container GetCosmosDBContainer(string databaseName, string containerName)
    {
        var connectionString = Environment.GetEnvironmentVariable("CosmosDBConnectionString");
        CosmosClientOptions options = new CosmosClientOptions() { ConnectionMode = ConnectionMode.Gateway };
        var cosmosClient = new CosmosClient(connectionString, options);

        var database = cosmosClient.GetDatabase(databaseName);
        var container = database.GetContainer(containerName);

        return container;
    }
}