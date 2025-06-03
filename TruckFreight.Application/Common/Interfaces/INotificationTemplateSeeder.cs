using System.Threading.Tasks;

namespace TruckFreight.Application.Common.Interfaces
{
    public interface INotificationTemplateSeeder
    {
        /// <summary>
        /// Seeds default notification templates into the database
        /// </summary>
        Task SeedAsync();
    }
} 