using System.Linq.Expressions;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SimpleErp.Models;
using SimpleErp.Models.Contract;
using SimpleErp.Repository.TruckRepository;

namespace SimpleErp.Services;

public class TruckService (ITruckRepository truckRepository, IMapper mapper, ILogger<TruckService> logger) : ITruckService
{
    public bool CheckIfTruckWithCodeExists(string code)
    {
        return truckRepository.CheckIfTruckWithCodeExists(code);
    }

    public IEnumerable<TruckResponse> GetTrucksForUser(Guid userId)
    {
        var trucks = truckRepository.Get(x => x.UserId == userId);
        return mapper.Map<IEnumerable<TruckResponse>>(trucks);
    }

    public async Task<TruckResponse> GetTruckByIdForUser(Guid id, Guid userId)
    {
        if (id == Guid.Empty || userId == Guid.Empty)
        {
            return null;
        }

        var truck = await truckRepository.GetByIdAsync(id);
        if (truck != null && truck.UserId == userId)
        {
            return mapper.Map<TruckResponse>(truck);
        }
        return null;
    }

    public IEnumerable<TruckResponse> GetFilteredTrucksForUser(TruckFilterOptions filterOptions, Guid userId)
    {
        var trucks = truckRepository.Get(x =>
            x.UserId == userId &&
            (string.IsNullOrEmpty(filterOptions.Status) || x.Status.ToString() == filterOptions.Status) &&
            (string.IsNullOrEmpty(filterOptions.SearchTerm) ||
            x.Name.Contains(filterOptions.SearchTerm) || x.Description.Contains(filterOptions.SearchTerm)));

        if (!string.IsNullOrEmpty(filterOptions.SortBy))
        {
            switch (filterOptions.SortBy.ToLower())
            {
                case "code":
                    trucks = ApplySorting(trucks, x => x.Code, filterOptions.Descending);
                    break;
                case "name":
                    trucks = ApplySorting(trucks, x => x.Name, filterOptions.Descending);
                    break;
                case "status":
                    trucks = ApplySorting(trucks, x => x.Status, filterOptions.Descending);
                    break;
                case "description":
                    trucks = ApplySorting(trucks, x => x.Description, filterOptions.Descending);
                    break;
                default:
                    break;
            }
        }

        return mapper.Map<IEnumerable<TruckResponse>>(trucks);
    }

    public async Task<TruckResponse?> AddTruckForUser(TruckRequest truckRequest, Guid userId)
    {
        try
        {
            if(truckRepository.CheckIfTruckWithCodeExists(truckRequest.Code))
            {
                throw new InvalidOperationException($"Truck with code {truckRequest.Code} already exists.");
            }

            var truckToAdd = mapper.Map<Truck>(truckRequest);

            truckToAdd.UserId = userId;

            var addedTruck = await truckRepository.AddAsync(truckToAdd);
            return mapper.Map<TruckResponse>(addedTruck);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogError(ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            return null;
        }
    }

    public async Task<TruckResponse?> UpdateTruckForUser(Guid truckId, TruckRequest truckRequest, Guid userId)
    {
        var truckToUpdate = await truckRepository.GetByIdAsync(truckId);
        if (truckToUpdate != null && truckToUpdate.UserId == userId)
        {
            try
            {
                truckToUpdate = mapper.Map<Truck>(truckRequest);
                var updatedTruck = await truckRepository.UpdateAsync(truckToUpdate);
                return mapper.Map<TruckResponse>(updatedTruck);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                throw;
            }
        }

        return null;
    }

    public async Task DeleteTruckForUser(Guid truckId, Guid userId)
    {
        var truckToDelete = await truckRepository.GetByIdAsync(truckId);
        if (truckToDelete != null && truckToDelete.UserId == userId)
        {
            await truckRepository.DeleteAsync(truckId);
        }
    }

    private IQueryable<Truck> ApplySorting<TKey>(IQueryable<Truck> query, Expression<Func<Truck, TKey>> keySelector, bool descending)
    {
        return descending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
    }
}
