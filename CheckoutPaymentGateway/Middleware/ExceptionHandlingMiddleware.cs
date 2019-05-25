namespace CheckoutPaymentGateway.Middleware
{
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using Exceptions;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    public class ExceptionHandlingMiddleware
    {
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        private readonly RequestDelegate _requestDelegate;

        public ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware> logger, RequestDelegate requestDelegate)
        {
            _logger = logger;
            _requestDelegate = requestDelegate;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _requestDelegate(context);
            }
            catch (Exception e)
            {
                if (!(e is ResourceNotFoundException) && !(e is TaskCanceledException))
                {
                    _logger.LogError(e, "Error when handling a request.");
                }

                await HandleExceptionAsync(context, e);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception e)
        {
            var code = HttpStatusCode.InternalServerError;
            var errorMessage = "Something went wrong. Please try again later.";

            if (e is TaskCanceledException)
            {
                code = HttpStatusCode.BadRequest;
                errorMessage = "Request cancelled by user.";
            }
            else if (e is ArgumentException)
            {
                code = HttpStatusCode.BadRequest;
                errorMessage = e.Message;
            }
            else if (e is ResourceNotFoundException)
            {
                code = HttpStatusCode.NotFound;
                errorMessage = e.Message;
            }

            var result = JsonConvert.SerializeObject(new { Error = errorMessage });
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int) code;

            await context.Response.WriteAsync(result);
        }
    }
}