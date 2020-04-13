using System.Collections.Generic;
using Microsoft.Azure.Documents;
using SocialFacesApp.Models;

namespace SocialFacesApp.Services.Contracts
{
    public interface INormalizeHappinessPerDay
    {
        IEnumerable<HappinessPerDayProjection> Perform(IReadOnlyCollection<Document> changedFaces);
    }
}