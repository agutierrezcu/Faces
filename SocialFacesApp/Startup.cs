using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using SocialFacesApp;
using SocialFacesApp.Extensions;
using SocialFacesApp.Options;
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
            var configurationRoot = services.AddConfiguration(GetType().Assembly);

            services.Configure<FacesApiOptions>(configurationRoot.GetSection("FacesApi"));

            //services.AddSingleton<IProvidePostedOnDate, TodayPostedOnProvider>();
            services.AddSingleton<IProvidePostedOnDate, RandomPostedOnProvider>();
            
            services.AddSingleton<IAnalyzePicture, PictureAnalyzer>();
            
            services.AddSingleton<INormalizeHappinessPerDay, HappinessPerDayNormalizer>();
            services.AddSingleton<IManageHappinessPerDayProjection, HappinessPerDayProjectionManager>();
        }
    }
}