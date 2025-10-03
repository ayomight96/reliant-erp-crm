using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Reliant.Api.Endpoints;

internal static class OpenApiOps
{
  public static OpenApiOperation Summarize(
    this OpenApiOperation op,
    string summary,
    string? description = null
  )
  {
    op.Summary = summary;
    if (!string.IsNullOrWhiteSpace(description))
      op.Description = description;
    return op;
  }

  public static OpenApiOperation RequestExample(this OpenApiOperation op, string json)
  {
    op.RequestBody = new OpenApiRequestBody
    {
      Content =
      {
        ["application/json"] = new OpenApiMediaType
        {
          Example = OpenApiAnyFactory.CreateFromJson(json),
        },
      },
    };
    return op;
  }

  public static OpenApiOperation AddQueryParam(
    this OpenApiOperation op,
    string name,
    string description,
    string? example = null
  )
  {
    op.Parameters ??= new List<OpenApiParameter>();
    op.Parameters.Add(
      new OpenApiParameter
      {
        Name = name,
        In = ParameterLocation.Query,
        Required = false,
        Description = description,
        Example = example is null ? null : new OpenApiString(example),
      }
    );
    return op;
  }
}
