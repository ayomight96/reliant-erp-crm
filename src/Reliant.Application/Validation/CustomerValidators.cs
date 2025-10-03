using FluentValidation;
using Reliant.Application.DTOs;

namespace Reliant.Application.Validation;

public class CreateCustomerRequestValidator : AbstractValidator<CreateCustomerRequest>
{
  public CreateCustomerRequestValidator()
  {
    RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
    RuleFor(x => x.Postcode).MaximumLength(20).When(x => !string.IsNullOrWhiteSpace(x.Postcode));
  }
}

public class UpdateCustomerRequestValidator : AbstractValidator<UpdateCustomerRequest>
{
  public UpdateCustomerRequestValidator()
  {
    RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
  }
}
