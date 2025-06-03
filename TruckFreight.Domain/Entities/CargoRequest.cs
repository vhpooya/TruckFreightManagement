using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TruckFreight.Domain.Common;
using TruckFreight.Domain.Enums;
using TruckFreight.Domain.ValueObjects;

namespace TruckFreight.Domain.Entities
{
    public class CargoRequest : BaseEntity
    {
        public Guid UserId { get; private set; }

        public CargoType Type { get; private set; }
        public string CargoName { get; private set; }
        public string Description { get; private set; }
        public Cartypeselect CartypeselectItem { get; set; }
      
        public double Weight { get; private set; }
        public string WeightUnit { get; private set; }
        public double Volume { get; private set; }
        public string VolumeUnit { get; private set; }

        [MaxLength(150)]
        public string ContactName { get; private set; }

        [MaxLength(15)]
        public string ContactPhone { get; private set; }

        [MaxLength(250)]
        [DataType(DataType.EmailAddress)]
        public string ContactEmail { get; private set; }
        public CargoStatus Status { get; private set; }

        [MaxLength(300)]
        public string PickupAddress { get; private set; } // محل بارگیری
        public double PickupLatitude { get; private set; }
        public double PickupLongitude { get; private set; }

        [MaxLength(300)]
        public string DeliveryAddress { get; private set; }  // محل تحویل بار
        public double DeliveryLatitude { get; private set; }
        public double DeliveryLongitude { get; private set; }
        public DateTime PickupTime { get; private set; }
        public DateTime? DeliveryTime { get; private set; }
        public Money Price { get; private set; }
        public string Notes { get; private set; }
     
        public DateTime? AcceptedAt { get; private set; }
        public DateTime? PickedUpAt { get; private set; }
        public DateTime? DeliveredAt { get; private set; }
        public DateTime? CancelledAt { get; private set; }
        public string CancellationReason { get; private set; }
        public Guid? DriverId { get; private set; }
        public Guid? TripId { get; private set; }

        // Navigation Properties
        public virtual User User { get; private set; }
        public virtual User Driver { get; private set; }
        public virtual Trip Trip { get; private set; }
        public virtual ICollection<CargoRequestDocument> Documents { get; private set; }
        public virtual ICollection<CargoRequestRating> Ratings { get; private set; }

        protected CargoRequest()
        {
            Documents = new HashSet<CargoRequestDocument>();
            Ratings = new HashSet<CargoRequestRating>();
            Status = CargoStatus.Pending;
        }

        public CargoRequest(
            Guid userId,
            string title,
            CargoType type,
            double weight,
            string weightUnit,
            double volume,
            string volumeUnit,
            string pickupAddress,
            double pickupLatitude,
            double pickupLongitude,
            string deliveryAddress,
            double deliveryLatitude,
            double deliveryLongitude,
            DateTime pickupTime,
            Money price,
            string contactName,
            string contactPhone,
            string contactEmail)
            : this()
        {
            UserId = userId;
            CargoName = title ?? throw new ArgumentNullException(nameof(title));
            Type = type;
            Weight = weight;
            WeightUnit = weightUnit ?? throw new ArgumentNullException(nameof(weightUnit));
            Volume = volume;
            VolumeUnit = volumeUnit ?? throw new ArgumentNullException(nameof(volumeUnit));
            PickupAddress = pickupAddress ?? throw new ArgumentNullException(nameof(pickupAddress));
            PickupLatitude = pickupLatitude;
            PickupLongitude = pickupLongitude;
            DeliveryAddress = deliveryAddress ?? throw new ArgumentNullException(nameof(deliveryAddress));
            DeliveryLatitude = deliveryLatitude;
            DeliveryLongitude = deliveryLongitude;
            PickupTime = pickupTime;
            Price = price ?? throw new ArgumentNullException(nameof(price));
            ContactName = contactName ?? throw new ArgumentNullException(nameof(contactName));
            ContactPhone = contactPhone ?? throw new ArgumentNullException(nameof(contactPhone));
            ContactEmail = contactEmail;
        }

        public void UpdateDescription(string description)
        {
            Description = description;
        }

        public void UpdateNotes(string notes)
        {
            Notes = notes;
        }

        public void Accept(Guid driverId, Guid vehicleId)
        {
            if (Status != CargoStatus.Pending)
                throw new InvalidOperationException("Cargo request can only be accepted when pending");

            Status = CargoStatus.Accepted;
            AcceptedAt = DateTime.UtcNow;
            DriverId = driverId;
           
        }

        public void PickUp()
        {
            if (Status != CargoStatus.Accepted)
                throw new InvalidOperationException("Cargo must be accepted before pickup");

            Status = CargoStatus.PickedUp;
            PickedUpAt = DateTime.UtcNow;
        }

        public void Deliver()
        {
            if (Status != CargoStatus.PickedUp)
                throw new InvalidOperationException("Cargo must be picked up before delivery");

            Status = CargoStatus.Delivered;
            DeliveredAt = DateTime.UtcNow;
            DeliveryTime = DateTime.UtcNow;
        }

        public void Cancel(string reason)
        {
            if (Status == CargoStatus.Delivered)
                throw new InvalidOperationException("Cannot cancel delivered cargo");

            Status = CargoStatus.Cancelled;
            CancelledAt = DateTime.UtcNow;
            CancellationReason = reason;
        }

        public void AssignToTrip(Guid tripId)
        {
            TripId = tripId;
        }

        public bool IsActive => Status >= CargoStatus.Pending && Status < CargoStatus.Delivered;
        public bool IsCompleted => Status == CargoStatus.Delivered;
    }

    public enum CargoStatus
    {
        Pending = 1,
        DriverAssigned = 2,
        InTransit = 3,
        Delivered = 4,
        Cancelled = 5,
        Failed = 6,
        PickedUp = 7,
        Accepted = 8
    }

}
