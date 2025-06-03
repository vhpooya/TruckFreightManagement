using System;
using System.Collections.Generic;
using TruckFreight.Domain.Common;
using TruckFreight.Domain.Enums;
using TruckFreight.Domain.ValueObjects;

namespace TruckFreight.Domain.Entities
{
    public class Trip : BaseEntity
    {
        public Guid CargoRequestId { get; private set; }
        public Guid DriverId { get; private set; }
      
        public string TripNumber { get; private set; }
        public TripStatus Status { get; private set; }
        public DateTime? AcceptedAt { get; private set; }
        public DateTime? StartedAt { get; private set; }
        public DateTime? LoadingStartedAt { get; private set; }
        public DateTime? LoadingCompletedAt { get; private set; }
        public DateTime? ArrivedAt { get; private set; }
        public DateTime? DeliveredAt { get; private set; }
        public DateTime? CompletedAt { get; private set; }
        public Money AgreedPrice { get; private set; }
        public Money ActualPrice { get; private set; }
        public string Notes { get; private set; }
        public string CancellationReason { get; private set; }
        public string ElectronicWaybillNumber { get; private set; }
        public bool IsSystemGeneratedWaybill { get; private set; }
        public string StartLocation { get; set; }
        public double StartLatitude { get; set; }
        public double StartLongitude { get; set; }
        public string EndLocation { get; set; }
        public double EndLatitude { get; set; }
        public double EndLongitude { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public double Distance { get; set; }
        public int Duration { get; set; }
        public decimal FuelCost { get; set; }
        public decimal TollCost { get; set; }
        public decimal TotalCost { get; set; }
        public string Route { get; set; }
        public bool IsActive { get; set; }
        public string CurrentLocation { get; set; }
        public double? CurrentLatitude { get; set; }
        public double? CurrentLongitude { get; set; }
        public DateTime? LastLocationUpdate { get; set; }

        // Navigation Properties
        public virtual CargoRequest CargoRequest { get; private set; }
        public virtual User Driver { get; private set; }
        public virtual Vehicle Vehicle { get; private set; }
        public virtual ICollection<TripTracking> TrackingHistory { get; private set; }
        public virtual ICollection<TripDocument> Documents { get; private set; }
        public virtual ICollection<TripRating> Ratings { get; private set; }
        public virtual ICollection<Cargo> Cargos { get; private set; }
        public virtual ICollection<Payment> Payments { get; private set; }
        public virtual Payment Payment { get; private set; }

        protected Trip()
        {
            TrackingHistory = new HashSet<TripTracking>();
            Documents = new HashSet<TripDocument>();
            Ratings = new HashSet<TripRating>();
            Cargos = new HashSet<Cargo>();
            Payments = new HashSet<Payment>();
        }

        public Trip(Guid cargoRequestId, Guid driverId,  Money agreedPrice)
            : this()
        {
            CargoRequestId = cargoRequestId;
            DriverId = driverId;
           
            TripNumber = GenerateTripNumber();
            AgreedPrice = agreedPrice ?? throw new ArgumentNullException(nameof(agreedPrice));
            Status = TripStatus.Assigned;
        }

        public void Accept()
        {
            if (Status != TripStatus.Assigned)
                throw new InvalidOperationException("Trip can only be accepted when assigned");

            Status = TripStatus.Accepted;
            AcceptedAt = DateTime.UtcNow;
        }

        public void Reject(string reason)
        {
            if (Status != TripStatus.Assigned)
                throw new InvalidOperationException("Trip can only be rejected when assigned");

            Status = TripStatus.Rejected;
            CancellationReason = reason;
        }

        public void Start()
        {
            if (Status != TripStatus.Accepted)
                throw new InvalidOperationException("Trip must be accepted before starting");

            Status = TripStatus.Started;
            StartedAt = DateTime.UtcNow;
        }

        public void StartLoading()
        {
            if (Status != TripStatus.Started)
                throw new InvalidOperationException("Trip must be started before loading");

            Status = TripStatus.Loading;
            LoadingStartedAt = DateTime.UtcNow;
        }

        public void CompleteLoading()
        {
            if (Status != TripStatus.Loading)
                throw new InvalidOperationException("Loading must be started before completing");

            Status = TripStatus.Loaded;
            LoadingCompletedAt = DateTime.UtcNow;
        }

        public void StartTransit()
        {
            if (Status != TripStatus.Loaded)
                throw new InvalidOperationException("Cargo must be loaded before starting transit");

            Status = TripStatus.InTransit;
        }

        public void Arrive()
        {
            if (Status != TripStatus.InTransit)
                throw new InvalidOperationException("Must be in transit before arriving");

            Status = TripStatus.Arrived;
            ArrivedAt = DateTime.UtcNow;
        }

        public void Deliver()
        {
            if (Status != TripStatus.Arrived)
                throw new InvalidOperationException("Must arrive before delivering");

            Status = TripStatus.Delivered;
            DeliveredAt = DateTime.UtcNow;
        }

        public void Complete(Money actualPrice = null)
        {
            if (Status != TripStatus.Delivered)
                throw new InvalidOperationException("Must deliver before completing");

            Status = TripStatus.Completed;
            CompletedAt = DateTime.UtcNow;
            ActualPrice = actualPrice ?? AgreedPrice;
        }

        public void Cancel(string reason)
        {
            if (Status == TripStatus.Completed || Status == TripStatus.Delivered)
                throw new InvalidOperationException("Cannot cancel completed or delivered trip");

            Status = TripStatus.Cancelled;
            CancellationReason = reason;
        }

        public void SetElectronicWaybill(string waybillNumber, bool isSystemGenerated = true)
        {
            ElectronicWaybillNumber = waybillNumber;
            IsSystemGeneratedWaybill = isSystemGenerated;
        }

        public void AddNotes(string notes)
        {
            Notes = notes;
        }

        private string GenerateTripNumber()
        {
            return $"TRP{DateTime.UtcNow:yyyyMMdd}{DateTime.UtcNow.Ticks % 10000:D4}";
        }

        public TimeSpan? GetTotalDuration()
        {
            if (StartedAt.HasValue && CompletedAt.HasValue)
                return CompletedAt.Value - StartedAt.Value;
            return null;
        }

        public TimeSpan? GetLoadingDuration()
        {
            if (LoadingStartedAt.HasValue && LoadingCompletedAt.HasValue)
                return LoadingCompletedAt.Value - LoadingStartedAt.Value;
            return null;
        }

        public bool IsInProgress => Status >= TripStatus.Accepted && Status < TripStatus.Completed;
        public bool IsCompleted => Status == TripStatus.Completed;
    }
}
