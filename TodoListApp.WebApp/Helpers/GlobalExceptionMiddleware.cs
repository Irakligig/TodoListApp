using System.Net;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace TodoListApp.WebApp.Helpers
{
    public class GlobalExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;

        public GlobalExceptionHandlerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (InvalidOperationException ex)
            {
                // JWT missing / not logged in
                Console.WriteLine($"[GlobalExceptionHandler] InvalidOperationException: {ex.Message}");

                // Prevent redirect loop
                if (!context.Request.Path.StartsWithSegments("/Auth/Login"))
                {
                    context.Response.Redirect("/Auth/Login");
                }
                else
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Unauthorized. Please log in.");
                }
            }
            catch (HttpRequestException ex)
            {
                // HTTP errors from services
                Console.WriteLine($"[GlobalExceptionHandler] HttpRequestException: {ex.Message}");

                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await context.Response.WriteAsync("An error occurred while communicating with the API.");
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (System.Exception ex)
            {
                // Catch all other unhandled exceptions
                Console.WriteLine($"[GlobalExceptionHandler] Exception: {ex.Message}");

                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                await context.Response.WriteAsync("An unexpected error occurred. Please try again later.");
            }
#pragma warning restore CA1031
        }
    }

    public static class GlobalExceptionHandlerMiddlewareExtensions
    {
        public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<GlobalExceptionHandlerMiddleware>();
        }
    }
}
