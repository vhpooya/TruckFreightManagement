using System;

namespace TruckFreight.Domain.Entities
{
    public class CargoRequestRating
    {
        public string Id { get; set; }
        public string CargoId { get; set; }
        public string RaterId { get; set; }
        public string RatedUserId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public Cargo Cargo { get; set; }
        public ApplicationUser Rater { get; set; }
        public ApplicationUser RatedUser { get; set; }
    }
} 