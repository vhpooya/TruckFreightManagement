using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TruckFreight.Domain.Interfaces;
using TruckFreight.Persistence.Repositories;

namespace TruckFreight.Persistence
{
    public static class PersistenceServiceCollectionExtensions
    {
        public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

            services.AddScoped<IDriverRepository, DriverRepository>();
            services.AddScoped<ICargoOwnerRepository, CargoOwnerRepository>();
            services.AddScoped<ICargoRequestRepository, CargoRequestRepository>();

            return services;
        }
    }
} 