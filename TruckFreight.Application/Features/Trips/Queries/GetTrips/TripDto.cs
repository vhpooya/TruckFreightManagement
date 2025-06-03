using System;
using System.Collections.Generic;
using AutoMapper;
using TruckFreight.Application.Common.Models;
using TruckFreight.Domain.Entities;

namespace TruckFreight.Application.Features.Trips.Queries.GetTrips
{
    public class TripDto : IMapFrom<Trip>
    {
        public Guid Id { get; set; }
        public Guid CargoRequestId { get; set; }
        public Guid VehicleId { get; set; }
        public Guid DriverId { get; set; }
        public decimal EstimatedDistance { get; set; }
        public string DistanceUnit { get; set; }
        public decimal EstimatedDuration { get; set; }
        public string DurationUnit { get; set; }
        public decimal EstimatedFuelConsumption { get; set; }
        public string FuelUnit { get; set; }
        public decimal EstimatedCost { get; set; }
        public string Currency { get; set; }
        public DateTime? EstimatedDepartureTime { get; set; }
        public DateTime? EstimatedArrivalTime { get; set; }
        public DateTime? ActualDepartureTime { get; set; }
        public DateTime? ActualArrivalTime { get; set; }
        public decimal? ActualDistance { get; set; }
        public decimal? ActualDuration { get; set; }
        public decimal? ActualFuelConsumption { get; set; }
        public decimal? ActualCost { get; set; }
        public string Notes { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? LastModifiedAt { get; set; }
        public string LastModifiedBy { get; set; }
        public DateTime? StartedAt { get; set; }
        public string StartedBy { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string CompletedBy { get; set; }
        public DateTime? CancelledAt { get; set; }
        public string CancelledBy { get; set; }
        public string CancellationReason { get; set; }
        public ICollection<TripDocumentDto> Documents { get; set; }
        public ICollection<TripRatingDto> Ratings { get; set; }
        public ICollection<TripTrackingDto> TrackingPoints { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Trip, TripDto>()
                .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()))
                .ForMember(d => d.Documents, opt => opt.MapFrom(s => s.Documents))
                .ForMember(d => d.Ratings, opt => opt.MapFrom(s => s.Ratings))
                .ForMember(d => d.TrackingPoints, opt => opt.MapFrom(s => s.TrackingPoints));
        }
    }

    public class TripDocumentDto : IMapFrom<TripDocument>
    {
        public Guid Id { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
        public string FileUrl { get; set; }
        public DateTime UploadedAt { get; set; }
        public string UploadedBy { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<TripDocument, TripDocumentDto>();
        }
    }

    public class TripRatingDto : IMapFrom<TripRating>
    {
        public Guid Id { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime RatedAt { get; set; }
        public string RatedBy { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<TripRating, TripRatingDto>();
        }
    }

    public class TripTrackingDto : IMapFrom<TripTracking>
    {
        public Guid Id { get; set; }
        public string Location { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public DateTime Timestamp { get; set; }
        public decimal? Speed { get; set; }
        public string SpeedUnit { get; set; }
        public decimal? FuelLevel { get; set; }
        public string FuelUnit { get; set; }
        public string Notes { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<TripTracking, TripTrackingDto>();
        }
    }
} 