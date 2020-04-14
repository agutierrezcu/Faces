using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;

namespace SocialFacesApp.Services.Contracts
{
    public interface IHappinessPerDayProjectionService
    {
        Task UpdateAsync(IReadOnlyList<Document> changedFaces);
    }
}