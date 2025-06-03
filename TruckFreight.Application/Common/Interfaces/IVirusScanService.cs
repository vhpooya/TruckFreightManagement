using System.IO;
using System.Threading.Tasks;

namespace TruckFreight.Application.Common.Interfaces
{
    public interface IVirusScanService
    {
        Task<bool> ScanFileAsync(Stream fileStream, string fileName);
        Task<bool> IsServiceAvailableAsync();
    }
} 