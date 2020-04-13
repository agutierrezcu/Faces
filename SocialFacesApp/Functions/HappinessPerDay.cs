using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace SocialFacesApp.Functions
{
    public static class HappinessPerDay
    {
        private const string AllHappinessPerDayQuery =
            "SELECT c.id as postedOn, c.peopleCount, c.happinessAmount, c.happinessAverage FROM c ORDER BY c.id DESC";

        [FunctionName("happinessPerDay")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            [CosmosDB(
                databaseName: Constants.CosmosDbDatabaseName,
                collectionName: Constants.CosmosDbHappinessPerDayCollectionName,
                ConnectionStringSetting = Constants.CosmosDbConnectionName,
                SqlQuery = AllHappinessPerDayQuery)] 
            IEnumerable<Document> happinessPerDayCollection,
            ILogger log)
        {
            return new OkObjectResult(happinessPerDayCollection);
        }
    }
}
