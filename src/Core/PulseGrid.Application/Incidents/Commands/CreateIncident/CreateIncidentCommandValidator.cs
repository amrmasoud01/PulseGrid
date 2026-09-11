namespace PulseGrid.Application.Incidents.Commands.CreateIncident;

using FluentValidation;

public class CreateIncidentCommandValidator : AbstractValidator<CreateIncidentCommand>
{
    public CreateIncidentCommandValidator()
    {
        RuleFor(v => v.ServiceId)
            .NotEmpty();

        RuleFor(v => v.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(v => v.Severity)
            .IsInEnum();

        RuleFor(v => v.InitialMessage)
            .MaximumLength(2000)
            .When(v => !string.IsNullOrWhiteSpace(v.InitialMessage));
    }
}
