using AutoMapper;

namespace TruckFreight.Application.Common.Models
{
    public interface IMapFrom<T>
    {
        void Mapping(Profile profile) => profile.CreateMap(typeof(T), GetType());
    }
} 