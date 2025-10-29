using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace MyApi.Middlewares
{
    public class GlobalException
    {
        private readonly RequestDelegate next;
        private readonly ILogger<GlobalException> logger;

        public GlobalException(RequestDelegate next, ILogger<GlobalException> logger)
        {
            {
                this.next = next;
                this.logger = logger;
            }
        }

        public async Task InvokeAsync(HttpContext context)
        {

            try
            {
                await next(context);

            }
            catch (Exception ex)
            {

                logger.LogError("eccezione ddvdd");

                await HandleException(context, ex);

            }
        }

        private static Task HandleException(HttpContext context, Exception ex)
        {
            var (statusCode, title) = ex switch
            {
                HttpRequestException => (StatusCodes.Status404NotFound, "Not Found in the fog"),
                InvalidProgramException => (StatusCodes.Status406NotAcceptable, "Invalid Exception Progrma"),
                ValidationException => (StatusCodes.Status400BadRequest, "Bad Request"),
                _ => (StatusCodes.Status500InternalServerError, "Server Error")
            };

            var traceId = Activity.Current?.Id ?? context.TraceIdentifier;

            var problemDetails = new ProblemDetails()
            {
                Detail = ex.Message,
                Instance = context.Request.Path,
                Status = statusCode,
                Title = title
            };

            problemDetails.Extensions["TRACEID"] = traceId;

            return context.Response.WriteAsJsonAsync(problemDetails);
        }


    }
}
