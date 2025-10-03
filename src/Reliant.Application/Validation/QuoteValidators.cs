using System.Collections.Generic;
using FluentValidation;
using Reliant.Application.DTOs;

namespace Reliant.Application.Validation;

public class QuoteItemCreateRequestValidator : AbstractValidator<QuoteItemCreateRequest>
{
  private static readonly HashSet<string> Materials = new(StringComparer.OrdinalIgnoreCase)
  {
    "uPVC",
    "Aluminium",
    "Composite",
  };

  private static readonly HashSet<string> Glazings = new(StringComparer.OrdinalIgnoreCase)
  {
    "double",
    "triple",
  };

  private static readonly HashSet<string> Install = new(StringComparer.OrdinalIgnoreCase)
  {
    "Standard",
    "Complex",
  };

  public QuoteItemCreateRequestValidator()
  {
    RuleFor(x => x.ProductId).GreaterThan(0);
    RuleFor(x => x.WidthMm).InclusiveBetween(300, 4000);
    RuleFor(x => x.HeightMm).InclusiveBetween(300, 4000);

    RuleFor(x => x.Material)
      .Must(m => !string.IsNullOrWhiteSpace(m) && Materials.Contains(m))
      .WithMessage("Material must be one of: uPVC, Aluminium, Composite.");

    RuleFor(x => x.Glazing)
      .Must(g => !string.IsNullOrWhiteSpace(g) && Glazings.Contains(g))
      .WithMessage("Glazing must be one of: double, triple.");

    When(
      x => !string.IsNullOrWhiteSpace(x.InstallComplexity),
      () =>
      {
        RuleFor(x => x.InstallComplexity!)
          .Must(i => Install.Contains(i))
          .WithMessage("InstallComplexity must be Standard or Complex.");
      }
    );

    RuleFor(x => x.Qty).GreaterThan(0);

    When(
      x => x.UnitPrice.HasValue,
      () =>
      {
        RuleFor(x => x.UnitPrice!.Value).GreaterThan(0m);
      }
    );
  }
}

public class CreateQuoteRequestValidator : AbstractValidator<CreateQuoteRequest>
{
  public CreateQuoteRequestValidator()
  {
    RuleFor(x => x.CustomerId).GreaterThan(0);
    RuleFor(x => x.Items).NotEmpty();
    RuleForEach(x => x.Items).SetValidator(new QuoteItemCreateRequestValidator());
  }
}
