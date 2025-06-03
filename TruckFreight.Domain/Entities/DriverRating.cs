using TruckFreight.Domain.Enums;

namespace TruckFreight.Domain.Entities
{
    public class DriverRating : Rating
    {
        public int DrivingSkillScore { get; private set; }
        public int PunctualityScore { get; private set; }
        public int CommunicationScore { get; private set; }
        public int VehicleConditionScore { get; private set; }

        protected DriverRating() { }

        public DriverRating(Guid tripId, Guid cargoOwnerId, Guid driverId, int overallScore,
                           string comment, int drivingSkill, int punctuality, 
                           int communication, int vehicleCondition)
            : base(tripId, cargoOwnerId, driverId, overallScore, comment, RatingType.DriverRating)
        {
            DrivingSkillScore = ValidateScore(drivingSkill);
            PunctualityScore = ValidateScore(punctuality);
            CommunicationScore = ValidateScore(communication);
            VehicleConditionScore = ValidateScore(vehicleCondition);
        }

        public void UpdateDetailedScores(int drivingSkill, int punctuality, 
                                       int communication, int vehicleCondition)
        {
            DrivingSkillScore = ValidateScore(drivingSkill);
            PunctualityScore = ValidateScore(punctuality);
            CommunicationScore = ValidateScore(communication);
            VehicleConditionScore = ValidateScore(vehicleCondition);
        }

        private int ValidateScore(int score)
        {
            if (score < 1 || score > 5)
                throw new ArgumentException("Score must be between 1 and 5");
            return score;
        }
    }
}
