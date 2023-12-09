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

public class RegisterMealTrigger
{
    [FunctionName("RegisterMealTrigger")]
    public async Task<IActionResult> RegisterMeal(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "children/{id}/meals")] HttpRequest req, string id,
        ILogger log)
    {
        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        Meal meal = JsonConvert.DeserializeObject<Meal>(requestBody);
        meal.Id = Guid.NewGuid().ToString();
        meal.ChildId = id;
        meal.Date = DateTime.Now;

        Container container = GetCosmosDBContainer("cloud services", "Meals");
        meal = await container.CreateItemAsync(meal, new PartitionKey(meal.ChildId));

        return new OkObjectResult(meal);
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

