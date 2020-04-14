using System;
using System.Data.Common;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace SocialFacesApp.Extensions
{
    public static partial class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCosmosDocumentClient(this IServiceCollection services, string connectionString)
        {
            services.TryAddSingleton<IDocumentClient>(provider =>
            {
                var cosmosDBConnectionString = new CosmosDbConnectionString(connectionString);
                return new DocumentClient(cosmosDBConnectionString.ServiceEndpoint, cosmosDBConnectionString.AuthKey,
                    new JsonSerializerSettings
                    {
                        ContractResolver = new CamelCasePropertyNamesContractResolver()
                    });
            });

            return services;
        }

        internal class CosmosDbConnectionString
        {
            public CosmosDbConnectionString(string connectionString)
            {
                // Use this generic builder to parse the connection string
                var builder = new DbConnectionStringBuilder
                {
                    ConnectionString = connectionString
                };

                if (builder.TryGetValue("AccountKey", out object key))
                {
                    AuthKey = key.ToString();
                }

                if (builder.TryGetValue("AccountEndpoint", out object uri))
                {
                    ServiceEndpoint = new Uri(uri.ToString());
                }
            }

            public Uri ServiceEndpoint { get; set; }

            public string AuthKey { get; set; }
        }
    }
}