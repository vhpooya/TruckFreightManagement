// In TruckFreightSystem.Application/Interfaces/Persistence/IUnitOfWork.cs
using TruckFreight.Domain.Interfaces;

namespace TruckFreightSystem.Application.Interfaces.Persistence
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository Users { get; }
        IDriverRepository Drivers { get; }
        ICargoOwnerRepository CargoOwners { get; }
        ITruckRepository Trucks { get; }
        ICargoRepository Cargos { get; }
        ITripRepository Trips { get; }
        IRatingRepository Ratings { get; }
        IDocumentRepository Documents { get; }
        INotificationRepository Notifications { get; }
        IWaybillRepository Waybills { get; }
        ISystemConfigurationRepository SystemConfigurations { get; }

        Task<int> CompleteAsync(); // Save changes
    }
}