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

namespace ExampleExam.Triggers;

public class GetMealsOfClassTrigger
{
    [FunctionName("GetMealsOfClassTrigger")]
    public async Task<IActionResult> GetMealsOfClass(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "classes/{id}/meals")] HttpRequest req, string id,
        ILogger log)
    {
        Container childrenContainer = GetCosmosDBContainer("cloud services", "Children");
        Container mealsContainer = GetCosmosDBContainer("cloud services", "Meals");

        var childrenQuery = new QueryDefinition("SELECT * FROM c");
        var childrenIterator = childrenContainer.GetItemQueryIterator<Child>(childrenQuery, requestOptions: new QueryRequestOptions() { PartitionKey = new PartitionKey(id) });

        List<Meal> meals = new();

        while (childrenIterator.HasMoreResults)
        {
            var response = await childrenIterator.ReadNextAsync();
            foreach (Child child in response)
            {
                var mealsQuery = new QueryDefinition("SELECT * FROM c");
                var mealsIterator = mealsContainer.GetItemQueryIterator<Meal>(mealsQuery, requestOptions: new QueryRequestOptions() { PartitionKey = new PartitionKey(child.Id) });

                while (mealsIterator.HasMoreResults)
                {
                    var mealsResponse = await mealsIterator.ReadNextAsync();
                    foreach (Meal meal in mealsResponse)
                    {
                        meals.Add(meal);
                    }
                }
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

