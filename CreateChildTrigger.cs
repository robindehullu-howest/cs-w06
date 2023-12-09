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

namespace ExamExample.Triggers;

public class CreateChildTrigger
{
    [FunctionName("CreateChildTrigger")]
    public async Task<IActionResult> CreateChild(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "children")] HttpRequest req,
        ILogger log)
    {
        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        Child child = JsonConvert.DeserializeObject<Child>(requestBody);

        Container container = GetCosmosDBContainer("cloud services", "Children");
        child = await container.CreateItemAsync(child, new PartitionKey(child.Class));

        return new OkObjectResult(child);
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

