using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using Microsoft.Extensions.Options;
using SocialFacesApp.Options;
using SocialFacesApp.Services.Contracts;

namespace SocialFacesApp.Services
{
    public class PictureAnalyzer : IAnalyzePicture
    {
        private readonly FacesApiOptions _facesApiOptions;

        public PictureAnalyzer(IOptions<FacesApiOptions> facesApiOptions)
        {
            _facesApiOptions = facesApiOptions.Value;
        }

        public async Task<string> ProcessAsync(Stream picture)
        {
            var apiKeyServiceClientCredentials = new ApiKeyServiceClientCredentials(_facesApiOptions.Key);
            var client = new FaceClient(apiKeyServiceClientCredentials)
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
    }
}