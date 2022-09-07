using System.Text.Json;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Journal.Domain.Exceptions;

namespace Journal.Api.Pipelines;

public class GrpcExceptionHandler : Interceptor
{
    private readonly ILogger<GrpcExceptionHandler> _logger;

    public GrpcExceptionHandler(ILogger<GrpcExceptionHandler> logger)
    {
        _logger = logger;
    }

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request, ServerCallContext context, UnaryServerMethod<TRequest, TResponse> continuation)
    {
        TResponse response;
        try
        {
            response = await base.UnaryServerHandler(request, context, continuation);
        }
        catch (CustomException ex)
        {
            if (ex is PropertyValidationException propertyValidationException)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, propertyValidationException.Message));
            }

            if (ex is NotFoundException notFoundException)
            {
                throw new RpcException(new Status(StatusCode.NotFound, notFoundException.Message));
            }

            throw;
        }
        catch (Exception ex)
        {
            Log(ex, request, context, _logger);
            throw;
        }

        return response;
    }

    private void Log<TRequest>(Exception exception, TRequest request, ServerCallContext context, ILogger<GrpcExceptionHandler> logger) where TRequest : class
    {
        string requestAsText = RequestAsText(request, context);
        logger.LogError(exception, "{RequestAsText}{NewLine1}{NewLine2}{ExceptionMessage}", requestAsText, Environment.NewLine, Environment.NewLine, exception.Message);
    }

    private string RequestAsText<TRequest>(TRequest request, ServerCallContext context) where TRequest : class
    {
        string rawRequestBody = JsonSerializer.Serialize(request);

        IEnumerable<string> headerLine = context.RequestHeaders
                                                .Where(h => h.Key != "Authentication")
                                                .Select(pair => $"{pair.Key} => {string.Join("|", pair.Value.ToList())}");
        string headerText = string.Join(Environment.NewLine, headerLine);

        string message =
            $"Request: {context.Host}/{context.Method}{Environment.NewLine}" +
            $"Headers: {Environment.NewLine}{headerText}{Environment.NewLine}" +
            $"Content : {Environment.NewLine}{rawRequestBody}";

        return message;
    }
}