// Exception Handling Middleware - Global exception handler for the API
// Catches unhandled exceptions and returns user-friendly error responses

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
        
        // Main middleware execution method
        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                // Continue to next middleware in pipeline
                await next(httpContext);
            }
            catch(Exception ex)
            {
                // Generate unique error ID for tracking
                var errorId = Guid.NewGuid();
                
                // Log the exception with error ID
                logger.LogError(ex, $"{errorId} : {ex.Message}");
                
                // Set response status and content type
                httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                httpContext.Response.ContentType = "application/json";

                // Create user-friendly error response
                var error = new
                {
                    Id = errorId,
                    ErrorMassage = "Something went wrong. We are looking into resolving it. Thank you for your patience."
                };
                
                // Send error response to client
                await httpContext.Response.WriteAsJsonAsync(error);
            }
        }
    }
}