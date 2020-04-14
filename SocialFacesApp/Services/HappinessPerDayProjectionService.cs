using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using SocialFacesApp.Models;
using SocialFacesApp.Persistence.Contracts;
using SocialFacesApp.Services.Contracts;

namespace SocialFacesApp.Services
{
    public class HappinessPerDayProjectionService : IHappinessPerDayProjectionService
    {
        private readonly INormalizeHappinessPerDay _happinessPerDayNormalizer;

        private readonly IHappinessPerDayProjectionClient _projectionClient;

        public HappinessPerDayProjectionService(INormalizeHappinessPerDay happinessPerDayNormalizer,
            IHappinessPerDayProjectionClient projectionClient)
        {
            _happinessPerDayNormalizer = happinessPerDayNormalizer;
            _projectionClient = projectionClient;
        }

        public async Task UpdateAsync(IReadOnlyList<Document> changedFaces)
        {
            var normalizedResults = _happinessPerDayNormalizer.Perform(changedFaces);

            foreach (var normalizedResult in normalizedResults)
            {
                var happinessPerDayProjection = await GetHappinessPerDayProjectionByDay(
                    normalizedResult.PostedOn);

                if (happinessPerDayProjection == null)
                {
                    await _projectionClient.CreateDocumentAsync(normalizedResult);
                }
                else
                {
                    happinessPerDayProjection.UpdateHappinessInfo(normalizedResult);
                    await _projectionClient.UpsertDocumentAsync(happinessPerDayProjection);
                }
            }
        }

        private async Task<HappinessPerDayProjection> GetHappinessPerDayProjectionByDay(DateTime postedOn)
        {
            var feedOptions = new FeedOptions
            {
                MaxItemCount = 1
            };
            var feedResponse = await
                _projectionClient.CreateDocumentQuery<HappinessPerDayProjection>(feedOptions)
                    .Where(p => p.PostedOn == postedOn)
                    .Take(1)
                    .AsDocumentQuery()
                    .ExecuteNextAsync<HappinessPerDayProjection>();
            var happinessPerDayProjection = feedResponse.FirstOrDefault();
            return happinessPerDayProjection;
        }
    }
}