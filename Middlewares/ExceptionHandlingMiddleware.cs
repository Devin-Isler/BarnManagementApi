using System.Net;

namespace BarnManagementApi.Middlewares
{
    public class ExceptionHandlingMiddleware
    {   
        private readonly ILogger<ExceptionHandlingMiddleware> logger;
        private readonly RequestDelegate next;
        public ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware> logger, RequestDelegate next)
        {
            this.logger = logger;
            this.next = next;
        }
        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await next(httpContext);
            }
            catch(Exception ex)
            {
                var errorId = Guid.NewGuid();
                // Log This Exception
                logger.LogError(ex, $"{errorId} : {ex.Message}");
                // Return a Custom Error Response
                httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                httpContext.Response.ContentType = "application/json";

                var error = new
                {
                    Id = errorId,
                    ErrorMassage = "Something went wrong. We are looking into resolving it. Thank you for your patience."
                };
                await httpContext.Response.WriteAsJsonAsync(error);

            }
        }
    }
}