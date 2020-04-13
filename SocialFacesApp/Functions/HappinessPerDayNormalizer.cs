using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SocialFacesApp.Services;
using SocialFacesApp.Services.Contracts;
using SocialShared.Logging;

namespace SocialFacesApp.Functions
{
    public class HappinessPerDayNormalizer
    {
        private readonly IManageHappinessPerDayProjection _happinessPerDayProjection;

        public HappinessPerDayNormalizer(IManageHappinessPerDayProjection happinessPerDayProjection)
        {
            _happinessPerDayProjection = happinessPerDayProjection;
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
            [CosmosDB(
                databaseName: Constants.CosmosDbDatabaseName,
                collectionName: Constants.CosmosDbHappinessPerDayCollectionName,
                ConnectionStringSetting = Constants.CosmosDbConnectionName)]
            DocumentClient documentClient,
            ILogger logger)
        {
            using var scopedLogger = new ScopedLogger(logger, "C# CosmosDB trigger function processed changes feed and updating happiness per day projection.");
            try
            {
                await _happinessPerDayProjection.UpdateAsync(changedFaces, documentClient);
            }
            catch (Exception ex)
            {
                logger.LogError(null, ex);
            }
        }
    }
}
