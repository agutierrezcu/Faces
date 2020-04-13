using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace SocialFacesApp.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IConfigurationRoot AddConfiguration(this IServiceCollection services, Assembly assembly)
        {
            var providers = new List<IConfigurationProvider>();

            var configServiceDescriptors =
                services.Where(descriptor => descriptor.ServiceType == typeof(IConfiguration))
                    .ToList();
            foreach (var descriptor in configServiceDescriptors)
            {
                if (!(descriptor.ImplementationInstance is IConfigurationRoot existingConfiguration))
                {
                    continue;
                }

                providers.AddRange(existingConfiguration.Providers);
                services.Remove(descriptor);
            }

            var serviceProvider = services.BuildServiceProvider();
            var executionContext = serviceProvider.GetService<IOptions<ExecutionContextOptions>>().Value;

            var builder = new ConfigurationBuilder()
                .SetBasePath(executionContext.AppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            var aspCoreEnvironment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            if (string.Equals(aspCoreEnvironment, "Development",
                StringComparison.InvariantCultureIgnoreCase))
            {
                builder.AddUserSecrets(assembly, optional: true);
            }
            else
            {
                var azureServiceTokenProvider = new AzureServiceTokenProvider();
                var authenticationCallback = new KeyVaultClient.AuthenticationCallback(
                    azureServiceTokenProvider.KeyVaultTokenCallback);
                var keyVaultClient = new KeyVaultClient(authenticationCallback);
                var defaultKeyVaultSecretManager = new DefaultKeyVaultSecretManager();
                builder.AddAzureKeyVault("https://socialnetworkapp-0-kv.vault.azure.net/",
                    keyVaultClient, defaultKeyVaultSecretManager);
            }

            var config = builder.Build();
            providers.AddRange(config.Providers);

            var configurationRoot = new ConfigurationRoot(providers);
            services.AddSingleton<IConfiguration>(configurationRoot);
            return configurationRoot;
        }
    }
}