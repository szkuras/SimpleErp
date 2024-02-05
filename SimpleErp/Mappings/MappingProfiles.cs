using AutoMapper;
using SimpleErp.Models;
using SimpleErp.Models.Contract;

namespace SimpleErp.Mappings;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<TruckRequest, Truck>()
            .ConstructUsing(src => new Truck { Id = Guid.NewGuid() });
        CreateMap<Truck, TruckResponse>();
    }
}
