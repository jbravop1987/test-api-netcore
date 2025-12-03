using System.Net;
using test_api.Core.Responses;

namespace test_api.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Una excepción no controlada ha ocurrido: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var response = new ApiResponse();

            switch (exception)
            {
                case KeyNotFoundException keyNotFound:
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    response = ApiResponse.ErrorResponse(keyNotFound.Message);
                    break;

                case InvalidOperationException invalidOp:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response = ApiResponse.ErrorResponse(invalidOp.Message);
                    break;

                case ArgumentException argEx:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response = ApiResponse.ErrorResponse(argEx.Message);
                    break;

                default:
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    response = ApiResponse.ErrorResponse(
                        "Ha ocurrido un error interno. Por favor, intente más tarde.",
                        new List<string> { exception.Message }
                    );
                    break;
            }

            return context.Response.WriteAsJsonAsync(response);
        }
    }
}
