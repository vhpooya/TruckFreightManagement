using TruckFreight.Domain.ValueObjects;
using TruckFreight.Domain.Enums;

namespace TruckFreight.Domain.Entities
{
    public class CargoRequestBid : BaseEntity
    {
        public Guid CargoRequestId { get; private set; }
        public Guid DriverId { get; private set; }
        public Money BidAmount { get; private set; }
        public string Message { get; private set; }
        public DateTime ExpiresAt { get; private set; }
        public bool IsAccepted { get; private set; }
        public bool IsRejected { get; private set; }
        public DateTime? AcceptedAt { get; private set; }
        public DateTime? RejectedAt { get; private set; }

        // Navigation Properties
        public virtual CargoRequest CargoRequest { get; private set; }
        public virtual Driver Driver { get; private set; }

        protected CargoRequestBid() { }

        public CargoRequestBid(Guid cargoRequestId, Guid driverId, Money bidAmount, 
                              string message = null, int validityHours = 24)
        {
            CargoRequestId = cargoRequestId;
            DriverId = driverId;
            BidAmount = bidAmount ?? throw new ArgumentNullException(nameof(bidAmount));
            Message = message;
            ExpiresAt = DateTime.UtcNow.AddHours(validityHours);
        }

        public void Accept()
        {
            if (IsExpired)
                throw new InvalidOperationException("Cannot accept expired bid");

            IsAccepted = true;
            AcceptedAt = DateTime.UtcNow;
        }

        public void Reject()
        {
            if (IsExpired)
                throw new InvalidOperationException("Cannot reject expired bid");

            IsRejected = true;
            RejectedAt = DateTime.UtcNow;
        }

        public bool IsExpired => DateTime.UtcNow > ExpiresAt;
        public bool IsPending => !IsAccepted && !IsRejected && !IsExpired;
    }
}
