using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using SocialFacesApp.Models;
using SocialFacesApp.Options;
using SocialShared.Logging;

namespace SocialFacesApp.Functions
{
    public class PictureAnalyzer
    {
        private readonly FacesApiOptions _facesApiOptions;

        private readonly Random _intervalDaysRandom = new Random();

        public PictureAnalyzer(IOptions<FacesApiOptions> facesApiOptions)
        {
            _facesApiOptions = facesApiOptions.Value;
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
            using var scopedLogger = new ScopedLogger(logger, "C# Blob trigger function analyzing the new image and store results");
            try
            {
                var analysisResult = await GetAnalysisAsync(picture, logger);
                await AddNewFaceDocumentAsync(analysisResult, facesCollection, logger);
            }
            catch (Exception ex)
            {
                logger.LogError(null, ex);
            }
        }

        private async Task<string> GetAnalysisAsync(Stream picture, ILogger logger)
        {
            var client = new FaceClient(new ApiKeyServiceClientCredentials(_facesApiOptions.Key))
            {
                Endpoint = _facesApiOptions.Endpoint
            };
            var faceAttributeTypes = new List<FaceAttributeType>
                {
                    FaceAttributeType.Accessories,
                    FaceAttributeType.Age,
                    FaceAttributeType.Emotion,
                    FaceAttributeType.Gender
                };
            var detectedFacesResponse = await client.Face.DetectWithStreamWithHttpMessagesAsync(
                picture, returnFaceAttributes: faceAttributeTypes, recognitionModel: RecognitionModel.Recognition01);

            var readAsStringAsync = await detectedFacesResponse.Response.Content.ReadAsStringAsync();
            return readAsStringAsync;
        }

        private async Task AddNewFaceDocumentAsync(string analysisResult, IAsyncCollector<JObject> facesCollection,
            ILogger logger)
        {
            var postedOn = GetRandomPostedOnDate();
            var newFaceDocument = JObject.FromObject(
                new FacesAnalysisResult
                {
                    PostedOn = postedOn,
                    Descriptors = JArray.Parse(analysisResult)
                });
            await facesCollection.AddAsync(newFaceDocument);
        }

        private DateTime GetRandomPostedOnDate()
        {
            const int daysOffset = 5;
            var postedOn = DateTime.UtcNow.Date.AddDays(_intervalDaysRandom.Next(-daysOffset, daysOffset));
            return postedOn;
        }
    }
}
