using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using SocialFacesApp.Models;
using SocialFacesApp.Services.Contracts;
using SocialShared.Logging;

namespace SocialFacesApp.Functions
{
    public class PictureAnalyzer
    {
        private readonly IAnalyzePicture _pictureAnalyzer;

        private readonly IProvidePostedOnDate _postedOnDateProvider;

        public PictureAnalyzer(IAnalyzePicture pictureAnalyzer, IProvidePostedOnDate postedOnDateProvider)
        {
            _pictureAnalyzer = pictureAnalyzer;
            _postedOnDateProvider = postedOnDateProvider;
        }

        [FunctionName("PictureAnalyzer")]
        public async Task Run(
            [BlobTrigger("pictures/{name}", Connection = "AzureWebJobsStorage")]
            Stream picture,
            [CosmosDB(
                databaseName: Constants.CosmosDbDatabaseName,
                collectionName: Constants.CosmosDbFacesCollectionName,
                ConnectionStringSetting = Constants.CosmosDbConnectionName)]
            IAsyncCollector<JObject> facesCollection,
            string name,
            ILogger logger)
        {
            using var scopedLogger = new ScopedLogger(logger, $"C# Blob trigger function analyzing the new {name} image and store results");
            try
            {
                var analysisResult = await _pictureAnalyzer.ProcessAsync(picture);
                var postedOn = _postedOnDateProvider.Get();

                var newFaceDocument = JObject.FromObject(
                    new FacesAnalysisResult
                    {
                        PostedOn = postedOn,
                        Descriptors = JArray.Parse(analysisResult)
                    });
                await facesCollection.AddAsync(newFaceDocument);
            }
            catch (Exception ex)
            {
                logger.LogError(null, ex);
            }
        }
    }
}
