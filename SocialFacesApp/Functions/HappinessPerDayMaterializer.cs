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

namespace SocialFacesApp.Functions
{
    public static class HappinessPerDayMaterializer
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

        [FunctionName("HappinessPerDayMaterializer")]
        public static async Task Run(
            [CosmosDBTrigger(
                databaseName: Constants.CosmosDbDatabaseName,
                collectionName: Constants.CosmosDbFacesCollectionName,
                ConnectionStringSetting = Constants.CosmosDbConnectionName,
                LeaseCollectionName = Constants.CosmosLeaseCollectionName,
                CreateLeaseCollectionIfNotExists = true)]
            IReadOnlyList<Document> changedFaces,
            [CosmosDB(
                databaseName: "SocialNetwork",
                collectionName: "happinessPerDay",
                ConnectionStringSetting = "CosmosDB")]
            DocumentClient documentClient,
            ILogger log)
        {
            try
            {
                var newMaterializedResults = MaterializeHappinessPerDayProjection(changedFaces);

                foreach (var newMaterializedResult in newMaterializedResults)
                {
                    var happinessPerDayProjection = await GetHappinessPerDayProjectionByDay(documentClient,
                        newMaterializedResult.PostedOn);

                    if (happinessPerDayProjection == null)
                    {
                        await documentClient.CreateDocumentAsync(HappinessPerDayUri, newMaterializedResult, CreateDocumentOptions);
                    }
                    else
                    {
                        happinessPerDayProjection.UpdateHappinessInfo(newMaterializedResult);
                        await documentClient.UpsertDocumentAsync(HappinessPerDayUri, happinessPerDayProjection, CreateDocumentOptions);
                    }
                }
            }
            catch (Exception ex)
            {
                log.LogError(null, ex);
            }
        }

        private static IEnumerable<HappinessPerDayProjection> MaterializeHappinessPerDayProjection(IReadOnlyCollection<Document> changedFaces)
        {
            if (changedFaces.Count == 0 )
            {
                return Enumerable.Empty<HappinessPerDayProjection>();
            }

            var newMaterializedResults = changedFaces
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
            return newMaterializedResults;
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
