using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TruckFreight.Domain.Entities;

namespace TruckFreight.Application.Common.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<CargoRequest> CargoRequests { get; set; }
        DbSet<Driver> Drivers { get; set; }
        DbSet<Notification> Notifications { get; set; }
        DbSet<Payment> Payments { get; set; }
        DbSet<Trip> Trips { get; set; }
        DbSet<User> Users { get; set; }
        DbSet<Vehicle> Vehicles { get; set; }
        DbSet<SystemSettings> SystemSettings { get; set; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
} 