using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SocialFacesApp.Services.Contracts;
using SocialShared.Logging;

namespace SocialFacesApp.Functions
{
    public class HappinessPerDayNormalizer
    {
        private readonly IHappinessPerDayProjectionService _happinessPerDayProjectionService;

        public HappinessPerDayNormalizer(IHappinessPerDayProjectionService happinessPerDayProjectionService)
        {
            _happinessPerDayProjectionService = happinessPerDayProjectionService;
        }

        [FunctionName("HappinessPerDayNormalizer")]
        public async Task Run(
            [CosmosDBTrigger(
                databaseName: Constants.CosmosDbDatabaseName,
                collectionName: Constants.CosmosDbFacesCollectionName,
                ConnectionStringSetting = Constants.CosmosDbConnectionName,
                LeaseCollectionName = Constants.CosmosLeaseCollectionName,
                CreateLeaseCollectionIfNotExists = true)]
            IReadOnlyList<Document> changedFaces,
            ILogger logger)
        {
            using var scopedLogger = new ScopedLogger(logger, "C# CosmosDB trigger function processed changes feed and updating happiness per day projection.");
            try
            {
                await _happinessPerDayProjectionService.UpdateAsync(changedFaces);
            }
            catch (Exception ex)
            {
                logger.LogError(null, ex);
            }
        }
    }
}
