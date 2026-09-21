using CustomerCampaign.Application.Common.Exceptions;
using CustomerCampaign.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace CustomerCampaign.Api.ErrorHandling
{
    public class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken ct)
        {
            var (status, title) = exception switch
            {
                DomainException => (StatusCodes.Status422UnprocessableEntity, "Business rule violation"),
                NotFoundException => (StatusCodes.Status404NotFound, "Resource not found"),
                ForbiddenAccessException => (StatusCodes.Status403Forbidden, "Forbidden"),
                ExternalServiceException => (StatusCodes.Status503ServiceUnavailable, "External service unavailable"),
                UnauthorizedException => (StatusCodes.Status401Unauthorized, "Authentication failed"),
                _ => (StatusCodes.Status500InternalServerError, "Unexpected error")
            };

            if (status == StatusCodes.Status500InternalServerError)
                logger.LogError(exception, "Unhandled exception");
            else
                logger.LogWarning("Request failed with {Status}: {Message}", status, exception.Message);

            httpContext.Response.StatusCode = status;

            return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                Exception = exception,
                ProblemDetails = new ProblemDetails
                {
                    Status = status,
                    Title = title,
                    Detail = status == StatusCodes.Status500InternalServerError
                        ? "An unexpected error occurred."
                        : exception.Message
                }
            });
        }
    }
}
