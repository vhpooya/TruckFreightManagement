
// TruckFreight.Domain/Entities/Rating.cs (Base for ratings)
using TruckFreight.Domain.Enums;

namespace TruckFreight.Domain.Entities
{
    public abstract class Rating : BaseEntity
    {
        public Guid TripId { get; protected set; }
        public Guid RatedById { get; protected set; }
        public Guid RatedUserId { get; protected set; }
        public int Score { get; protected set; }  // 1-5
        public string Comment { get; protected set; }
        public RatingType Type { get; protected set; }

        // Navigation Properties
        public virtual Trip Trip { get; protected set; }
        public virtual User RatedBy { get; protected set; }
        public virtual User RatedUser { get; protected set; }

        protected Rating() { }

        protected Rating(Guid tripId, Guid ratedById, Guid ratedUserId,
                        int score, string comment, RatingType type)
        {
            if (score < 1 || score > 5)
                throw new ArgumentException("Score must be between 1 and 5", nameof(score));

            TripId = tripId;
            RatedById = ratedById;
            RatedUserId = ratedUserId;
            Score = score;
            Comment = comment;
            Type = type;
        }

        public void UpdateRating(int score, string comment = null)
        {
            if (score < 1 || score > 5)
                throw new ArgumentException("Score must be between 1 and 5", nameof(score));

            Score = score;
            if (comment != null)
                Comment = comment;
        }
    }
}
