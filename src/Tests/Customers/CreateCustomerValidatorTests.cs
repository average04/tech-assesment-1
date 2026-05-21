using CustomerOnboarding.Application.Customers.Commands;
using CustomerOnboarding.Application.Customers.Dtos;
using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace CustomerOnboarding.Tests.Customers;

public class CreateCustomerValidatorTests
{
    private readonly CreateCustomerValidator _sut = new();

    private static CreateCustomerCommand Valid() => new(new CreateCustomerRequest(
        FirstName: "Ada",
        LastName: "Lovelace",
        Email: "ada@example.com",
        PhoneNumber: "+15551234567",
        SignatureBase64: "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNkYAAAAAYAAjCB0C8AAAAASUVORK5CYII="));

    [Fact]
    public void Valid_input_passes()
    {
        _sut.TestValidate(Valid()).ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void First_name_required(string firstName)
    {
        var cmd = Valid() with { Request = Valid().Request with { FirstName = firstName } };
        _sut.TestValidate(cmd).ShouldHaveValidationErrorFor("Request.FirstName");
    }

    [Fact]
    public void First_name_max_100()
    {
        var cmd = Valid() with { Request = Valid().Request with { FirstName = new string('a', 101) } };
        _sut.TestValidate(cmd).ShouldHaveValidationErrorFor("Request.FirstName");
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    [InlineData("missing@tld")]
    public void Email_must_be_valid(string email)
    {
        var cmd = Valid() with { Request = Valid().Request with { Email = email } };
        _sut.TestValidate(cmd).ShouldHaveValidationErrorFor("Request.Email");
    }

    [Theory]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData("123")]
    public void Phone_must_be_valid(string phone)
    {
        var cmd = Valid() with { Request = Valid().Request with { PhoneNumber = phone } };
        _sut.TestValidate(cmd).ShouldHaveValidationErrorFor("Request.PhoneNumber");
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-a-data-url")]
    [InlineData("data:image/jpeg;base64,abc")]
    public void Signature_must_be_png_data_url(string sig)
    {
        var cmd = Valid() with { Request = Valid().Request with { SignatureBase64 = sig } };
        _sut.TestValidate(cmd).ShouldHaveValidationErrorFor("Request.SignatureBase64");
    }
}
