using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace BirdAudioAnalysis
{
    public interface IAudioDatabaseDownloader
    {
        Task<string[]> DownloadAudioForBird(string scientificName);
    }
}