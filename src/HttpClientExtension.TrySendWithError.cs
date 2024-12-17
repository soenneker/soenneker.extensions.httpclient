using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Soenneker.Extensions.Object;
using Soenneker.Extensions.ValueTask;

namespace Soenneker.Extensions.HttpClient;

public static partial class HttpClientExtension
{
    public static async ValueTask<(TSuccessResponse? SuccessResponse, TErrorResponse? ErrorResponse)> TrySendWithError<TSuccessResponse, TErrorResponse>(this System.Net.Http.HttpClient client, string uri, ILogger? logger = null, CancellationToken cancellationToken = default)
    {
        using var requestMessage = new System.Net.Http.HttpRequestMessage(HttpMethod.Get, uri);

        return await TrySendWithError<TSuccessResponse, TErrorResponse>(client, requestMessage, logger, cancellationToken).NoSync();
    }

    public static async ValueTask<(TSuccessResponse? SuccessResponse, TErrorResponse? ErrorResponse)> TrySendWithError<TSuccessResponse, TErrorResponse>(this System.Net.Http.HttpClient client, 
        HttpMethod httpMethod, string uri, object? request = null, ILogger? logger = null, CancellationToken cancellationToken = default)
    {
        using var requestMessage = new System.Net.Http.HttpRequestMessage(httpMethod, uri);

        if (request != null)
            requestMessage.Content = request.TryToHttpContent();

        return await TrySendWithError<TSuccessResponse, TErrorResponse>(client, requestMessage, logger, cancellationToken).NoSync();
    }

    public static async ValueTask<(TSuccessResponse? SuccessResponse, TErrorResponse? ErrorResponse)> TrySendWithError<TSuccessResponse, TErrorResponse>(this System.Net.Http.HttpClient client, 
        System.Net.Http.HttpRequestMessage request, ILogger? logger = null, CancellationToken cancellationToken = default)
    {
        try
        {
            return await SendWithError<TSuccessResponse?, TErrorResponse?>(client, request, cancellationToken).NoSync();
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Exhausted all retry attempts for the HTTP request, returning null");
            return default;
        }
    }
}