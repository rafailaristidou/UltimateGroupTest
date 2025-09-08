using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace SimpleInventory.Web.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;
        private readonly IWebHostEnvironment _environment;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger, IWebHostEnvironment environment)
        {
            _next = next;
            _logger = logger;
            _environment = environment;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred");
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/problem+json";

            var problem = new ProblemDetails
            {
                Instance = context.Request.Path,
                Extensions = { ["traceId"] = context.TraceIdentifier }
            };

            switch (exception)
            {
                case ArgumentException argEx:
                    problem.Title = "Bad Request";
                    problem.Status = (int)HttpStatusCode.BadRequest;
                    problem.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1";
                    problem.Detail = _environment.IsDevelopment() ? argEx.Message : "Invalid request parameters.";
                    break;

                case InvalidOperationException invOpEx:
                    problem.Title = "Invalid Operation";
                    problem.Status = (int)HttpStatusCode.BadRequest;
                    problem.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1";
                    problem.Detail = _environment.IsDevelopment() ? invOpEx.Message : "The requested operation is not valid.";
                    break;

                case Microsoft.EntityFrameworkCore.DbUpdateException dbEx:
                    problem.Title = "Database Error";
                    problem.Status = (int)HttpStatusCode.Conflict;
                    problem.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.8";
                    problem.Detail = _environment.IsDevelopment() ? dbEx.InnerException?.Message ?? dbEx.Message : "A database error occurred.";
                    break;

                default:
                    problem.Title = "Internal Server Error";
                    problem.Status = (int)HttpStatusCode.InternalServerError;
                    problem.Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1";
                    problem.Detail = _environment.IsDevelopment() ? exception.Message : "An unexpected error occurred. Please try again later.";
                    break;
            }

            context.Response.StatusCode = problem.Status ?? (int)HttpStatusCode.InternalServerError;

            var jsonResponse = JsonSerializer.Serialize(problem, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = _environment.IsDevelopment()
            });

            await context.Response.WriteAsync(jsonResponse);
        }
    }

    public static class GlobalExceptionMiddlewareExtensions
    {
        public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<GlobalExceptionMiddleware>();
        }
    }
}
