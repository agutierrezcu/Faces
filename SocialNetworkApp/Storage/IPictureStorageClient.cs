using System.IO;
using System.Threading.Tasks;

namespace SocialNetworkApp.Storage
{
    public interface IPictureStorageClient
    {
        Task SaveAsync(Stream picture);
    }
}