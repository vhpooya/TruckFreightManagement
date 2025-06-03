using System;
using TruckFreight.Domain.Common;

namespace TruckFreight.Domain.Entities
{
    public class TripRating : BaseEntity
    {
        public Guid TripId { get; private set; }
        public Guid UserId { get; private set; }
        public int Rating { get; private set; }
        public string Comment { get; private set; }
        public bool IsDriverRating { get; private set; }
        public bool IsCargoOwnerRating { get; private set; }
        public int? PunctualityRating { get; private set; }
        public int? ProfessionalismRating { get; private set; }
        public int? CommunicationRating { get; private set; }
        public int? VehicleConditionRating { get; private set; }
        public bool IsPublic { get; private set; }
        public DateTime? PublishedAt { get; private set; }
        public bool IsEdited { get; private set; }
        public DateTime? EditedAt { get; private set; }
        public string EditReason { get; private set; }

        // Navigation Properties
        public virtual Trip Trip { get; private set; }
        public virtual User User { get; private set; }

        protected TripRating() { }

        public TripRating(
            Guid tripId,
            Guid userId,
            int rating,
            string comment,
            bool isDriverRating,
            int? punctualityRating = null,
            int? professionalismRating = null,
            int? communicationRating = null,
            int? vehicleConditionRating = null,
            bool isPublic = true)
        {
            if (rating < 1 || rating > 5)
                throw new ArgumentException("Rating must be between 1 and 5", nameof(rating));

            if (punctualityRating.HasValue && (punctualityRating.Value < 1 || punctualityRating.Value > 5))
                throw new ArgumentException("Punctuality rating must be between 1 and 5", nameof(punctualityRating));

            if (professionalismRating.HasValue && (professionalismRating.Value < 1 || professionalismRating.Value > 5))
                throw new ArgumentException("Professionalism rating must be between 1 and 5", nameof(professionalismRating));

            if (communicationRating.HasValue && (communicationRating.Value < 1 || communicationRating.Value > 5))
                throw new ArgumentException("Communication rating must be between 1 and 5", nameof(communicationRating));

            if (vehicleConditionRating.HasValue && (vehicleConditionRating.Value < 1 || vehicleConditionRating.Value > 5))
                throw new ArgumentException("Vehicle condition rating must be between 1 and 5", nameof(vehicleConditionRating));

            TripId = tripId;
            UserId = userId;
            Rating = rating;
            Comment = comment;
            IsDriverRating = isDriverRating;
            IsCargoOwnerRating = !isDriverRating;
            PunctualityRating = punctualityRating;
            ProfessionalismRating = professionalismRating;
            CommunicationRating = communicationRating;
            VehicleConditionRating = vehicleConditionRating;
            IsPublic = isPublic;
            IsEdited = false;

            if (isPublic)
            {
                PublishedAt = DateTime.UtcNow;
            }
        }

        public void UpdateRating(
            int newRating,
            int? punctualityRating = null,
            int? professionalismRating = null,
            int? communicationRating = null,
            int? vehicleConditionRating = null,
            string editReason = null)
        {
            if (newRating < 1 || newRating > 5)
                throw new ArgumentException("Rating must be between 1 and 5", nameof(newRating));

            if (punctualityRating.HasValue && (punctualityRating.Value < 1 || punctualityRating.Value > 5))
                throw new ArgumentException("Punctuality rating must be between 1 and 5", nameof(punctualityRating));

            if (professionalismRating.HasValue && (professionalismRating.Value < 1 || professionalismRating.Value > 5))
                throw new ArgumentException("Professionalism rating must be between 1 and 5", nameof(professionalismRating));

            if (communicationRating.HasValue && (communicationRating.Value < 1 || communicationRating.Value > 5))
                throw new ArgumentException("Communication rating must be between 1 and 5", nameof(communicationRating));

            if (vehicleConditionRating.HasValue && (vehicleConditionRating.Value < 1 || vehicleConditionRating.Value > 5))
                throw new ArgumentException("Vehicle condition rating must be between 1 and 5", nameof(vehicleConditionRating));

            Rating = newRating;
            PunctualityRating = punctualityRating;
            ProfessionalismRating = professionalismRating;
            CommunicationRating = communicationRating;
            VehicleConditionRating = vehicleConditionRating;
            IsEdited = true;
            EditedAt = DateTime.UtcNow;
            EditReason = editReason;
        }

        public void UpdateComment(string newComment, string editReason = null)
        {
            Comment = newComment;
            IsEdited = true;
            EditedAt = DateTime.UtcNow;
            EditReason = editReason;
        }

        public void Publish()
        {
            if (!IsPublic)
            {
                IsPublic = true;
                PublishedAt = DateTime.UtcNow;
            }
        }

        public void Unpublish()
        {
            IsPublic = false;
            PublishedAt = null;
        }
    }
}
