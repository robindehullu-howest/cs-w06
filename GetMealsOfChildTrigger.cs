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

public class GetMealsOfChildTrigger
{
    [FunctionName("GetMealsOfChildTrigger")]
    public async Task<IActionResult> GetMealsOfChild(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "children/{id}/meals")] HttpRequest req, string id,
        ILogger log)
    {
        Container container = GetCosmosDBContainer("cloud services", "Meals");

        var query = new QueryDefinition("SELECT * FROM c");
        var iterator = container.GetItemQueryIterator<Meal>(query, requestOptions: new QueryRequestOptions() { PartitionKey = new PartitionKey(id) });

        List<Meal> meals = new();

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            foreach (var meal in response)
            {
                meals.Add(meal);
            }
        }

        return new OkObjectResult(meals);
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
