using SimpleErp.Models;
using SimpleErp.Models.Contract;

namespace SimpleErp.Services;

public interface ITruckService
{
    bool CheckIfTruckWithCodeExists(string code);
    IEnumerable<TruckResponse> GetTrucksForUser(Guid userId);
    Task<TruckResponse> GetTruckByIdForUser(Guid id, Guid userId);
    IEnumerable<TruckResponse> GetFilteredTrucksForUser(TruckFilterOptions filterOptions, Guid userId);
    Task<TruckResponse> AddTruckForUser(TruckRequest truckRequest, Guid userId);
    Task<TruckResponse?> UpdateTruckForUser(Guid truckId, TruckRequest truckRequest, Guid userId);
    Task DeleteTruckForUser(Guid truckId, Guid userId);
}
