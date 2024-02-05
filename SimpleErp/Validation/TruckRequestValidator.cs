using FluentValidation;
using SimpleErp.Models;
using SimpleErp.Models.Contract;
using SimpleErp.Services;

namespace SimpleErp.Validation;

public class TruckRequestValidator : AbstractValidator<TruckRequest>
{
    private readonly ITruckService _truckService;

    public TruckRequestValidator(ITruckService truckService)
    {
        _truckService = truckService;

        RuleFor(truck => truck.Code)
            .NotEmpty().WithMessage("Code must not be empty")
            .Must(BeAlphanumeric).WithMessage("Code must be alphanumeric")
            .Must(IsUniqueCode).WithMessage("Code must be unique");

        RuleFor(truck => truck.Name)
            .NotEmpty().WithMessage("Name must not be empty");

        RuleFor(truck => truck.Status)
            .IsInEnum().WithMessage("Invalid status")
            .Must((request, status) => IsValidStatusTransition(request.Status, status))
            .WithMessage("Invalid status transition");

        RuleFor(truck => truck.Description)
            .MaximumLength(255).WithMessage("Description must be at most 255 characters");
    }

    private bool BeAlphanumeric(string code)
    {
        return !string.IsNullOrWhiteSpace(code) && code.All(char.IsLetterOrDigit);
    }

    private bool IsUniqueCode(string code)
    {
        return !_truckService.CheckIfTruckWithCodeExists(code);
    }

    private bool IsValidStatusTransition(TruckStatus currentStatus, TruckStatus newStatus)
    {
        switch (currentStatus)
        {
            case TruckStatus.OutOfService:
                return true;

            case TruckStatus.Loading:
                return newStatus == TruckStatus.ToJob;

            case TruckStatus.ToJob:
                return newStatus == TruckStatus.AtJob;

            case TruckStatus.AtJob:
                return newStatus == TruckStatus.Returning;

            case TruckStatus.Returning:
                return newStatus == TruckStatus.Loading;

            default:
                return false;
        }
    }
}
