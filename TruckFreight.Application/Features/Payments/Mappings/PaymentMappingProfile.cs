using AutoMapper;
using TruckFreight.Application.Features.Payments.DTOs;
using TruckFreight.Domain.Entities;

namespace TruckFreight.Application.Features.Payments.Mappings
{
    public class PaymentMappingProfile : Profile
    {
        public PaymentMappingProfile()
        {
            CreateMap<Payment, PaymentDto>();
            CreateMap<Payment, PaymentInfo>()
                .ForMember(dest => dest.PaymentId, opt => opt.MapFrom(src => src.Id));
        }
    }
} 