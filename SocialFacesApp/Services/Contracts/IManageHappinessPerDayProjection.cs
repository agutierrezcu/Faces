using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;

namespace SocialFacesApp.Services.Contracts
{
    public interface IManageHappinessPerDayProjection
    {
        Task UpdateAsync(IReadOnlyList<Document> changedFaces, IDocumentClient documentClient);
    }
}