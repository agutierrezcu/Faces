using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SocialFacesApp.Models;
using SocialShared.Logging;

namespace SocialFacesApp.Functions
{
    public static class HappinessPerDayNormalizer
    {
        private static readonly Uri HappinessPerDayUri =
            UriFactory.CreateDocumentCollectionUri("SocialNetwork", "happinessPerDay");

        private static readonly RequestOptions CreateDocumentOptions = new RequestOptions
        {
            JsonSerializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            }
        };

        [FunctionName("HappinessPerDayNormalizer")]
        public static async Task Run(
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
                var normalizedResults = NormalizeHappinessPerDayProjection(changedFaces);

                foreach (var normalizedResult in normalizedResults)
                {
                    var happinessPerDayProjection = await GetHappinessPerDayProjectionByDay(documentClient,
                        normalizedResult.PostedOn);

                    if (happinessPerDayProjection == null)
                    {
                        await documentClient.CreateDocumentAsync(HappinessPerDayUri, normalizedResult, CreateDocumentOptions);
                    }
                    else
                    {
                        happinessPerDayProjection.UpdateHappinessInfo(normalizedResult);
                        await documentClient.UpsertDocumentAsync(HappinessPerDayUri, happinessPerDayProjection, CreateDocumentOptions);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(null, ex);
            }
        }

        private static IEnumerable<HappinessPerDayProjection> NormalizeHappinessPerDayProjection(IReadOnlyCollection<Document> changedFaces)
        {
            if (changedFaces.Count == 0 )
            {
                return Enumerable.Empty<HappinessPerDayProjection>();
            }

            var normalizedResults = changedFaces
                .Select(d => JsonConvert.DeserializeObject<FacesAnalysisResult>(d.ToString()))
                .GroupBy(p => p.PostedOn)
                    .Select(g =>
                {
                    var allDescriptors = g.SelectMany(r => r.Descriptors).ToList();
                    return new HappinessPerDayProjection
                        {
                            PostedOn = g.Key,
                            PeopleCount = allDescriptors.Count,
                            HappinessAmount = allDescriptors.Sum(d => (decimal)d.SelectToken("faceAttributes.emotion.happiness")),
                        };
                })
                .ToList();
            return normalizedResults;
        }

        private static async Task<HappinessPerDayProjection> GetHappinessPerDayProjectionByDay(IDocumentClient documentClient, DateTime postedOn)
        {
            var feedOptions = new FeedOptions
            {
                MaxItemCount = 1    
            };
            var feedResponse = await
                documentClient.CreateDocumentQuery<HappinessPerDayProjection>(HappinessPerDayUri, feedOptions)
                    .Where(p => p.PostedOn == postedOn)
                    .Take(1)
                    .AsDocumentQuery()
                    .ExecuteNextAsync<HappinessPerDayProjection>();
            var happinessPerDayProjection = feedResponse.FirstOrDefault();
            return happinessPerDayProjection;
        }   
    }
}
