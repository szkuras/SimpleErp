using SimpleErp.Models;

namespace SimpleErp.Repository.TruckRepository;

public class TruckRepository(AppDbContext dbContext) : Repository<Truck>(dbContext), ITruckRepository
{
    public bool CheckIfTruckWithCodeExists(string code)
    {
        return dbContext.Trucks.Any(x => x.Code == code);
    }
}

