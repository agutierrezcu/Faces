using System.IO;
using System.Threading.Tasks;

namespace SocialFacesApp.Services.Contracts
{
    public interface IAnalyzePicture
    {
        Task<string> ProcessAsync(Stream picture);
    }
}