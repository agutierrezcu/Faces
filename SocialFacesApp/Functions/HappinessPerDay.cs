using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SocialFacesApp.Models;

namespace SocialFacesApp.Functions
{
    public static class HappinessPerDay
    {
        private const string AllHappinessPerDayQuery =
            "SELECT c.id as postedOn, c.peopleCount, c.happinessAmount, c.happinessAverage FROM c ORDER BY c.id DESC";

        [FunctionName("HappinessPerDay")]
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
