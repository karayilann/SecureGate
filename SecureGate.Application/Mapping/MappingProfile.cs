using AutoMapper;
using SecureGate.Application.DTOs;
using SecureGate.Domain.Entities;

namespace SecureGate.Application.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<ApiKey, ApiKeyDto>()
            .ForMember(dest => dest.PlanName,
                opt => opt.MapFrom(src => src.Plan != null ? src.Plan.Name.ToString() : string.Empty))
            .ForMember(dest => dest.Status,
                opt => opt.MapFrom(src => src.Status.ToString()));
    }
}