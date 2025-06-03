using System;
using System.Collections.Generic;
using AutoMapper;
using TruckFreight.Application.Common.Models;
using TruckFreight.Domain.Entities;

namespace TruckFreight.Application.Features.CargoRequests.Queries.GetCargoRequests
{
    public class CargoRequestDto : IMapFrom<CargoRequest>
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string PickupLocation { get; set; }
        public string DeliveryLocation { get; set; }
        public decimal Weight { get; set; }
        public string WeightUnit { get; set; }
        public decimal Length { get; set; }
        public decimal Width { get; set; }
        public decimal Height { get; set; }
        public string DimensionUnit { get; set; }
        public string CargoType { get; set; }
        public string SpecialInstructions { get; set; }
        public DateTime? PreferredPickupDate { get; set; }
        public DateTime? PreferredDeliveryDate { get; set; }
        public decimal? InsuranceValue { get; set; }
        public string InsuranceCurrency { get; set; }
        public bool IsFragile { get; set; }
        public bool IsHazardous { get; set; }
        public bool RequiresRefrigeration { get; set; }
        public string ContactName { get; set; }
        public string ContactPhone { get; set; }
        public string ContactEmail { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? LastModifiedAt { get; set; }
        public string LastModifiedBy { get; set; }
        public DateTime? AcceptedAt { get; set; }
        public string AcceptedBy { get; set; }
        public DateTime? RejectedAt { get; set; }
        public string RejectedBy { get; set; }
        public string RejectionReason { get; set; }
        public ICollection<CargoRequestDocumentDto> Documents { get; set; }
        public ICollection<CargoRequestRatingDto> Ratings { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<CargoRequest, CargoRequestDto>()
                .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()))
                .ForMember(d => d.Documents, opt => opt.MapFrom(s => s.Documents))
                .ForMember(d => d.Ratings, opt => opt.MapFrom(s => s.Ratings));
        }
    }

    public class CargoRequestDocumentDto : IMapFrom<CargoRequestDocument>
    {
        public Guid Id { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
        public string FileUrl { get; set; }
        public DateTime UploadedAt { get; set; }
        public string UploadedBy { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<CargoRequestDocument, CargoRequestDocumentDto>();
        }
    }

    public class CargoRequestRatingDto : IMapFrom<CargoRequestRating>
    {
        public Guid Id { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime RatedAt { get; set; }
        public string RatedBy { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<CargoRequestRating, CargoRequestRatingDto>();
        }
    }
} 