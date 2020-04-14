using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using SocialFacesApp.Persistence.Contracts;

namespace SocialFacesApp.Persistence
{
    public class HappinessPerDayProjectionClient : IHappinessPerDayProjectionClient
    {
        private static readonly Uri HappinessPerDayUri =
            UriFactory.CreateDocumentCollectionUri(
                Constants.CosmosDbDatabaseName, Constants.CosmosDbHappinessPerDayCollectionName);

        private readonly IDocumentClient _documentClient;

        public HappinessPerDayProjectionClient(IDocumentClient documentClient)
        {
            _documentClient = documentClient;
        }

        public IOrderedQueryable<T> CreateDocumentQuery<T>(FeedOptions feedOptions = null)
        {
            return _documentClient.CreateDocumentQuery<T>(HappinessPerDayUri, feedOptions);
        }

        public async Task<ResourceResponse<Document>> CreateDocumentAsync(object document, RequestOptions options = null, bool disableAutomaticIdGeneration = false,
            CancellationToken cancellationToken = default)
        {
            return await _documentClient.CreateDocumentAsync(HappinessPerDayUri, document, options,
                disableAutomaticIdGeneration, cancellationToken);
        }

        public async Task<ResourceResponse<Document>> UpsertDocumentAsync(object document, RequestOptions options = null,
            bool disableAutomaticIdGeneration = false, CancellationToken cancellationToken = default)
        {
            return await _documentClient.UpsertDocumentAsync(HappinessPerDayUri, document, options,
                disableAutomaticIdGeneration, cancellationToken);
        }
    }
}