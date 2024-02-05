using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SimpleErp.Authorization;
using SimpleErp.Models;
using SimpleErp.Models.Contract;
using SimpleErp.Services;

namespace SimpleErp.Controllers;

[ApiController]
[Route("[controller]")]
public class TrucksController (ITruckService truckService, IUserService userService, IValidator<TruckRequest> truckRequestValidator) : ControllerBase
{
    [ApiKey]
    [HttpGet]
    public async Task<IEnumerable<TruckResponse>> GetTrucks()
    {
        var userId = await GetAuthorizedUserId();

        return truckService.GetTrucksForUser(userId);
    }

    [ApiKey]
    [HttpGet("{id}")]
    public async Task<ActionResult<TruckResponse>> GetTruckById(Guid id)
    {
        var userId = await GetAuthorizedUserId();

        var truck = await truckService.GetTruckByIdForUser(id, userId);

        if (truck == null)
        {
            return NotFound();
        }

        return truck;
    }

    [ApiKey]
    [HttpGet("filtered")]
    public async Task<IEnumerable<TruckResponse>> GetFilteredTrucks([FromQuery] TruckFilterOptions filterOptions)
    {
        var userId = await GetAuthorizedUserId();
        var filteredTrucks = truckService.GetFilteredTrucksForUser(filterOptions, userId);

        return filteredTrucks.AsEnumerable();
    }

    [ApiKey]
    [HttpPost]
    public async Task<ActionResult<TruckResponse>> AddTruck(TruckRequest truck)
    {
        var userId = await GetAuthorizedUserId();

        var validationResult = truckRequestValidator.Validate(truck);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors.Select(error => error.ErrorMessage));
        }

        var addedTruck = await truckService.AddTruckForUser(truck, userId);

        if (addedTruck != null)
        {
            return Ok(addedTruck);
        }

        return BadRequest();
    }

    [ApiKey]
    [HttpPut("{id}")]
    public async Task<ActionResult<TruckResponse>> UpdateTruck([FromRoute] Guid id, TruckRequest truck)
    {
        var userId = await GetAuthorizedUserId();
        
        var validationResult = truckRequestValidator.Validate(truck);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors.Select(error => error.ErrorMessage));
        }

        var updatedTruck = await truckService.UpdateTruckForUser(id, truck, userId);
        if (updatedTruck != null)
        {
            return Ok(updatedTruck);
        }

        return NotFound();
    }

    [ApiKey]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTruck([FromRoute] Guid id)
    {
        var userId = await GetAuthorizedUserId();

        await truckService.DeleteTruckForUser(id, userId);
        return NoContent();
    }

    private async Task<Guid> GetAuthorizedUserId()
    {
        var user = await userService.GetByApiKey(HttpContext.Request.Headers["X-API-Key"]);
        return user.Id;
    }
}
