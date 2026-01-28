using System.Net.Mime;
using System.Text.Json;

namespace PierreProject.Core.Api.Configurations.Middleware;

public class InvalidRequestBodyMiddleware(RequestDelegate next, ILogger<InvalidRequestBodyMiddleware> logger)
{
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (BadHttpRequestException ex) when (ex.StatusCode == StatusCodes.Status400BadRequest)
        {
            // This commonly occurs when the JSON is malformed or the body can't be read
            await WriteBadRequestAsync(context, ex);
        }
        catch (JsonException ex)
        {
            await WriteBadRequestAsync(context, ex);
        }
    }

    private async Task WriteBadRequestAsync(HttpContext context, Exception ex)
    {
        try
        {
            if (!context.Response.HasStarted)
            {
                logger.LogWarning(ex, "Invalid request body detected");
                context.Response.Clear();
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                context.Response.ContentType = MediaTypeNames.Application.Json;

                var problem = new
                {
                    type = "https://httpstatuses.com/400",
                    title = "Invalid request",
                    status = StatusCodes.Status400BadRequest,
                    detail = $"Request body is invalid: {ex.Message}"
                };

                await context.Response.WriteAsJsonAsync(problem);
            }
        }
        catch (Exception writeEx)
        {
            logger.LogError(writeEx, "Failed to write invalid body problem details response");
            throw; // Let upstream handlers deal with this
        }
    }
}