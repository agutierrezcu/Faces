﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SocialFacesApp;
using SocialFacesApp.Options;

[assembly: FunctionsStartup(typeof(Startup))]

namespace SocialFacesApp
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            ConfigureServices(builder.Services);
        }

        public void ConfigureServices(IServiceCollection services)
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
                builder.AddUserSecrets(GetType().Assembly, optional: true);
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

            services.Configure<FacesApiOptions>(configurationRoot.GetSection("FacesApi"));
        }
    }
}