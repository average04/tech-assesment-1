using FluentValidation;
using FluentValidation.Validators;

namespace CustomerOnboarding.Application.Customers.Commands;

public class CreateCustomerValidator : AbstractValidator<CreateCustomerCommand>
{
    private const string PngDataUrlPrefix = "data:image/png;base64,";

    public CreateCustomerValidator()
    {
        RuleFor(x => x.Request.FirstName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Request.LastName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Request.Email)
            .NotEmpty()
            .EmailAddress(EmailValidationMode.Net4xRegex);

        RuleFor(x => x.Request.PhoneNumber)
            .NotEmpty()
            .Matches(@"^\+?[0-9]{7,15}$")
            .WithMessage("PhoneNumber must be 7-15 digits, optionally prefixed with '+'.");

        RuleFor(x => x.Request.SignatureBase64)
            .NotEmpty()
            .Must(BeValidPngDataUrl)
            .WithMessage("SignatureBase64 must be a base64-encoded PNG data URL.");
    }

    private static bool BeValidPngDataUrl(string? value) =>
        !string.IsNullOrWhiteSpace(value) && value.StartsWith(PngDataUrlPrefix, StringComparison.Ordinal);
}
