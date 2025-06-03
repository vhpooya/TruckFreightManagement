using System;
using System.Collections.Generic;

namespace TruckFreight.Application.Features.Ratings.DTOs
{
    public class CreateRatingDto
    {
        public string DeliveryId { get; set; }
        public string RatedEntityId { get; set; }
        public string RatedEntityType { get; set; } // "Driver", "Customer", "Company"
        public double Rating { get; set; }
        public string Comment { get; set; }
        public Dictionary<string, double> CategoryRatings { get; set; } // For detailed ratings by category
    }

    public class RatingDto
    {
        public string Id { get; set; }
        public string DeliveryId { get; set; }
        public string RatedEntityId { get; set; }
        public string RatedEntityType { get; set; }
        public string RatedByName { get; set; }
        public string RatedById { get; set; }
        public double Rating { get; set; }
        public string Comment { get; set; }
        public Dictionary<string, double> CategoryRatings { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsVerified { get; set; }
        public string VerificationStatus { get; set; }
        public string VerificationComment { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public string VerifiedBy { get; set; }
    }

    public class RatingListDto
    {
        public List<RatingDto> Items { get; set; }
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }

    public class RatingSummaryDto
    {
        public double AverageRating { get; set; }
        public int TotalRatings { get; set; }
        public Dictionary<double, int> RatingDistribution { get; set; }
        public Dictionary<string, double> CategoryAverages { get; set; }
        public List<RatingDto> RecentRatings { get; set; }
        public double RatingTrend { get; set; } // Percentage change in average rating
    }

    public class RatingFilterDto
    {
        public string RatedEntityId { get; set; }
        public string RatedEntityType { get; set; }
        public string DeliveryId { get; set; }
        public double? MinRating { get; set; }
        public double? MaxRating { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool? IsVerified { get; set; }
        public string VerificationStatus { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SortBy { get; set; }
        public bool SortDescending { get; set; }
    }
} 