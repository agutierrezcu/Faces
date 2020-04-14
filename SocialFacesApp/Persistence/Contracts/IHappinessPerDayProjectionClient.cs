using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace SocialFacesApp.Persistence.Contracts
{
    public interface IHappinessPerDayProjectionClient
    {
        IOrderedQueryable<T> CreateDocumentQuery<T>(FeedOptions feedOptions = null);

        Task<ResourceResponse<Document>> CreateDocumentAsync(
            object document,
            RequestOptions options = null,
            bool disableAutomaticIdGeneration = false,
            CancellationToken cancellationToken = default);

        public Task<ResourceResponse<Document>> UpsertDocumentAsync(
            object document,
            RequestOptions options = null,
            bool disableAutomaticIdGeneration = false,
            CancellationToken cancellationToken = default);
    }
}