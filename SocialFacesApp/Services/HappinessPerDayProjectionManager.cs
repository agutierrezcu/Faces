using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SocialFacesApp.Models;
using SocialFacesApp.Services.Contracts;

namespace SocialFacesApp.Services
{
    public class HappinessPerDayProjectionManager : IManageHappinessPerDayProjection
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

        private readonly INormalizeHappinessPerDay _happinessPerDayNormalizer;

        public HappinessPerDayProjectionManager(INormalizeHappinessPerDay happinessPerDayNormalizer)
        {
            _happinessPerDayNormalizer = happinessPerDayNormalizer;
        }

        public async Task UpdateAsync(IReadOnlyList<Document> changedFaces, IDocumentClient documentClient)
        {
            var normalizedResults = _happinessPerDayNormalizer.Perform(changedFaces);

            foreach (var normalizedResult in normalizedResults)
            {
                var happinessPerDayProjection = await GetHappinessPerDayProjectionByDay(
                    normalizedResult.PostedOn, documentClient);

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

        private async Task<HappinessPerDayProjection> GetHappinessPerDayProjectionByDay(DateTime postedOn,
            IDocumentClient documentClient)
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