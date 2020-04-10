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
            ILogger log)
        {
            var newLine = Environment.NewLine;
            log.LogInformation(@$"C# Blob trigger function Processed blob{newLine} Name:{name} {newLine} Size: {picture.Length} Bytes");

            try
            {
                var analysisResult = await GetAnalysisAsync(picture);
                var newFaceDocument = CreateNewFaceDocument(analysisResult);
                await facesCollection.AddAsync(newFaceDocument);
            }
            catch (Exception ex)
            {
                log.LogError(null, ex);
            }
        }

        private async Task<string> GetAnalysisAsync(Stream picture)
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

        private JObject CreateNewFaceDocument(string analysisResult)
        {
            var postedOn = GetRandomPostedOnDate();
            var newFaceDocument = JObject.FromObject(
                new FacesAnalysisResult
                {
                    PostedOn = postedOn,
                    Descriptors = JArray.Parse(analysisResult)
                });
            return newFaceDocument;
        }

        private DateTime GetRandomPostedOnDate()
        {
            const int daysOffset = 2;
            var postedOn = DateTime.UtcNow.Date.AddDays(_intervalDaysRandom.Next(-daysOffset, daysOffset));
            return postedOn;
        }
    }
}
