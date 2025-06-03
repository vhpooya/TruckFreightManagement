using TruckFreight.Domain.Enums;

namespace TruckFreight.Domain.Entities
{
    public class CargoOwnerRating : Rating
    {
        public int PaymentTimelinessScore { get; private set; }
        public int CommunicationScore { get; private set; }
        public int CargoDescriptionAccuracyScore { get; private set; }
        public int LoadingFacilitiesScore { get; private set; }

        protected CargoOwnerRating() { }

        public CargoOwnerRating(Guid tripId, Guid driverId, Guid cargoOwnerId, int overallScore,
                               string comment, int paymentTimeliness, int communication,
                               int cargoAccuracy, int loadingFacilities)
            : base(tripId, driverId, cargoOwnerId, overallScore, comment, RatingType.CargoOwnerRating)
        {
            PaymentTimelinessScore = ValidateScore(paymentTimeliness);
            CommunicationScore = ValidateScore(communication);
            CargoDescriptionAccuracyScore = ValidateScore(cargoAccuracy);
            LoadingFacilitiesScore = ValidateScore(loadingFacilities);
        }

        public void UpdateDetailedScores(int paymentTimeliness, int communication,
                                       int cargoAccuracy, int loadingFacilities)
        {
            PaymentTimelinessScore = ValidateScore(paymentTimeliness);
            CommunicationScore = ValidateScore(communication);
            CargoDescriptionAccuracyScore = ValidateScore(cargoAccuracy);
            LoadingFacilitiesScore = ValidateScore(loadingFacilities);
        }

        private int ValidateScore(int score)
        {
            if (score < 1 || score > 5)
                throw new ArgumentException("Score must be between 1 and 5");
            return score;
        }
    }
}
