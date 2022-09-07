using System.Text;
using System.Text.Json;
using Journal.Domain.Exceptions;

namespace Journal.Api.Pipelines;

public class ApiExceptionHandler
{
    private readonly RequestDelegate _next;

    public ApiExceptionHandler(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext httpContext, ILogger<ApiExceptionHandler> logger)
    {
        try
        {
            httpContext.Request.EnableBuffering();
            await _next.Invoke(httpContext);
        }
        catch (CustomException e)
        {
            await CreateResponseAsync(e, httpContext);
        }
        catch (Exception e)
        {
            await LogAsync(e, logger, httpContext);
            await CreateResponseAsync(e, httpContext);
        }
    }

    private async Task CreateResponseAsync(Exception exception, HttpContext httpContext)
    {
        int statusCode = StatusCodes.Status500InternalServerError;
        object payload = new { Message = "Unexpected error occurs" };

        if (exception is PropertyValidationException propertyValidationException)
        {
            statusCode = StatusCodes.Status400BadRequest;
            payload = new { Message = propertyValidationException.Message };
        }
        if (exception is NotFoundException notFoundException)
        {
            statusCode = StatusCodes.Status404NotFound;
            payload = new { Message = notFoundException.Message };
        }

        string errorHttpContentStr = JsonSerializer.Serialize(payload);

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/json";
        await httpContext.Response.WriteAsync(errorHttpContentStr);
    }

    private async Task LogAsync(Exception exception, ILogger<ApiExceptionHandler> logger, HttpContext httpContext)
    {
        string requestAsText = await RequestAsTextAsync(httpContext);
        logger.LogError(exception, "{RequestAsText}{NewLine1}{NewLine2}{ExceptionMessage}", requestAsText, Environment.NewLine, Environment.NewLine, exception.Message);
    }

    private static async Task<string> RequestAsTextAsync(HttpContext httpContext)
    {
        string rawRequestBody = await GetRawBodyAsync(httpContext.Request);

        IEnumerable<string> headerLine = httpContext.Request
                                                    .Headers
                                                    .Where(h => h.Key != "Authentication")
                                                    .Select(pair => $"{pair.Key} => {string.Join("|", pair.Value.ToList())}");
        string headerText = string.Join(Environment.NewLine, headerLine);

        string message =
            $"Request: {httpContext.Request.Scheme}://{httpContext.Request.Host}{httpContext.Request.Path}{httpContext.Request.QueryString}{Environment.NewLine}" +
            $"Headers: {Environment.NewLine}{headerText}{Environment.NewLine}" +
            $"Content : {Environment.NewLine}{rawRequestBody}";

        return message;
    }

    private static async Task<string> GetRawBodyAsync(HttpRequest request, Encoding? encoding = null)
    {
        request.Body.Position = 0;
        using var reader = new StreamReader(request.Body, encoding ?? Encoding.UTF8);
        string body = await reader.ReadToEndAsync().ConfigureAwait(false);
        request.Body.Position = 0;

        return body;
    }
}