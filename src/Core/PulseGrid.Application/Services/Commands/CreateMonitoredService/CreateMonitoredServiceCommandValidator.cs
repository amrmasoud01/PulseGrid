namespace PulseGrid.Application.Services.Commands.CreateMonitoredService;

using FluentValidation;

public class CreateMonitoredServiceCommandValidator : AbstractValidator<CreateMonitoredServiceCommand>
{
    public CreateMonitoredServiceCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(150);

        RuleFor(x => x.TargetUrl)
            .NotEmpty()
            .MaximumLength(2048)
            .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
            .WithMessage("TargetUrl must be a valid absolute URL.");

        RuleFor(x => x.IntervalSeconds)
            .GreaterThan(0);

        RuleFor(x => x.TimeoutMs)
            .GreaterThan(0);
    }
}
