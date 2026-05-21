using AutoMapper;
using CustomerOnboarding.Application.Customers.Dtos;
using CustomerOnboarding.Domain.Customers;

namespace CustomerOnboarding.Application.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Customer, CustomerDto>();
    }
}
