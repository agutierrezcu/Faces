using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.Documents;
using Newtonsoft.Json;
using SocialFacesApp.Models;
using SocialFacesApp.Services.Contracts;

namespace SocialFacesApp.Services
{
    public class HappinessPerDayNormalizer : INormalizeHappinessPerDay
    {
        public IEnumerable<HappinessPerDayProjection> Perform(IReadOnlyCollection<Document> changedFaces)
        {
            if (changedFaces.Count == 0)
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
    }
}