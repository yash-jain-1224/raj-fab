using FluentValidation;
using RajFabAPI.DTOs;

namespace RajFabAPI.Validators
{
    public class CreateEstablishmentRegistrationDtoValidator : AbstractValidator<CreateEstablishmentRegistrationDto>
    {
        public CreateEstablishmentRegistrationDtoValidator()
        {
            RuleFor(x => x.EstablishmentDetails).NotNull().WithMessage("EstablishmentDetails is required.");

            When(x => x.EstablishmentDetails != null, () =>
            {
                RuleFor(x => x.EstablishmentDetails.Name)
                    .NotEmpty().WithMessage("EstablishmentName is required.")
                    .MaximumLength(500);
                RuleFor(x => x.EstablishmentDetails.LinNumber)
                    .MaximumLength(200);
            });

            // Validate at least one establishment type is present
            RuleFor(x => x)
                .Must(HasAtLeastOneType)
                .WithMessage("At least one establishment type must be provided (Factory / BeediCigarWorks / MotorTransportService / BuildingAndConstructionWork / NewsPaperEstablishment / AudioVisualWork / Plantation).");

            // Sample nested validations
            When(x => x.Factory != null, () =>
            {
                RuleFor(x => x.Factory.NumberOfWorker)
                    .GreaterThanOrEqualTo(0).When(f => f.Factory.NumberOfWorker.HasValue && f.Factory.NumberOfWorker > 0)
                    .WithMessage("Factory.NumberOfWorker must be > 0");
            });

            When(x => x.BeediCigarWorks != null, () =>
            {
                RuleFor(x => x.BeediCigarWorks.MaxNumberOfWorkerAnyDay)
                    .GreaterThanOrEqualTo(0).When(b => b.BeediCigarWorks.MaxNumberOfWorkerAnyDay.HasValue)
                    .WithMessage("BeediCigarWorks.MaxNumberOfWorkerAnyDay must be > 0");
            });

            // Person / contact validations
            When(x => x.MainOwnerDetail != null, () =>
            {
                RuleFor(x => x.MainOwnerDetail.Email).EmailAddress().When(p => !string.IsNullOrWhiteSpace(p.MainOwnerDetail.Email));
                RuleFor(x => x.MainOwnerDetail.Mobile).MaximumLength(50);
            });

            // Add further rules as needed for other nested DTOs...
        }

        private bool HasAtLeastOneType(CreateEstablishmentRegistrationDto dto)
        {
            return dto.Factory != null
                   || dto.BeediCigarWorks != null
                   || dto.MotorTransportService != null
                   || dto.BuildingAndConstructionWork != null
                   || dto.NewsPaperEstablishment != null
                   || dto.AudioVisualWork != null
                   || dto.Plantation != null;
        }
    }
}