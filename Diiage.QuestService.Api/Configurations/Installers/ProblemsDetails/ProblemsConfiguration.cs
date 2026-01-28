using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using ProjectPierre.Common.Exceptions.Exceptions;
using ProblemDetailsOptions = Hellang.Middleware.ProblemDetails.ProblemDetailsOptions;

namespace Diiage.QuestService.Api.Configurations.Installers.ProblemsDetails;

public class ProblemsConfiguration(bool isDevEnv)
{
    private bool IsDevEnv { get; } = isDevEnv;

    public void ConfigureProblemDetails(ProblemDetailsOptions options)
    {
        options.IncludeExceptionDetails = (_, _) => IsDevEnv;

        // This will map NotImplementedException to the 501 Not Implemented status code.
        options.MapToStatusCode<NotImplementedException>(StatusCodes.Status501NotImplemented);

        // This will map HttpRequestException to the 503 Service Unavailable status code.
        options.MapToStatusCode<HttpRequestException>(StatusCodes.Status503ServiceUnavailable);

        // Map FluentValidation exceptions to 400
        options.Map<ValidationException>(ex => new ProblemDetails
        {
            Title = "Validation error",
            Status = StatusCodes.Status400BadRequest,
            Detail = string.Join("; ", ex.Errors.Select(e => e.ErrorMessage))
        });

        options.MapToStatusCode<BusinessException>(StatusCodes.Status400BadRequest);

        options.MapToStatusCode<NotFoundException>(StatusCodes.Status404NotFound);

        // Because exceptions are handled polymorphically, this will act as a "catch all" mapping, which is why it's added last.
        // If an exception other than NotImplementedException and HttpRequestException is thrown, this will handle it.
        options.MapToStatusCode<Exception>(StatusCodes.Status500InternalServerError);
    }
}