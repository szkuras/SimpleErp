using SimpleErp.Models;

namespace SimpleErp.Repository.TruckRepository;

public interface ITruckRepository : IRepository<Truck>
{
    bool CheckIfTruckWithCodeExists(string code);
}

