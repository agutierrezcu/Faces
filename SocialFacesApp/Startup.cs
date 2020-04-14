using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using SocialFacesApp;
using SocialFacesApp.Extensions;
using SocialFacesApp.Options;
using SocialFacesApp.Persistence;
using SocialFacesApp.Persistence.Contracts;
using SocialFacesApp.Services;
using SocialFacesApp.Services.Contracts;

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
            var configuration = services.AddConfiguration(GetType().Assembly);

            services.AddCosmosDocumentClient(configuration[Constants.CosmosDbConnectionName]);

            var facesApiSection = configuration.GetSection("FacesApi");
            //services.Configure<FacesApiOptions>(facesApiSection);
            services.AddOptions<FacesApiOptions>().Bind(facesApiSection);

            //services.AddSingleton<IProvidePostedOnDate, TodayPostedOnProvider>();
            services.AddSingleton<IProvidePostedOnDate, RandomPostedOnProvider>();

            services.AddSingleton<IAnalyzePicture, PictureAnalyzer>();

            services.AddSingleton<INormalizeHappinessPerDay, HappinessPerDayNormalizer>();
            services.AddSingleton<IHappinessPerDayProjectionService, HappinessPerDayProjectionService>();
            services.AddSingleton<IHappinessPerDayProjectionClient, HappinessPerDayProjectionClient>();
        }
    }
}