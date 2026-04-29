using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;

namespace ChurchAdmin.Api.Common;

public static class ValidationProblemDetailsFactory
{
    public static BadRequestObjectResult ToBadRequest(this ValidationResult validationResult)
    {
        Dictionary<string, string[]> errors = validationResult.Errors
            .GroupBy(error => error.PropertyName)
            .ToDictionary(
                group => group.Key,
                group => group.Select(error => error.ErrorMessage).ToArray());

        ValidationProblemDetails problemDetails = new ValidationProblemDetails(errors)
        {
            Title = "Validation failed",
            Status = StatusCodes.Status400BadRequest
        };

        return new BadRequestObjectResult(problemDetails);
    }
}