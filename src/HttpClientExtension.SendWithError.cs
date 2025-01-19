using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Soenneker.Extensions.HttpResponseMessage;
using Soenneker.Extensions.Object;
using Soenneker.Extensions.Task;
using Soenneker.Extensions.ValueTask;

namespace Soenneker.Extensions.HttpClient;

public static partial class HttpClientExtension
{
    public static async ValueTask<(TSuccessResponse? SuccessResponse, TErrorResponse? ErrorResponse)> SendWithError<TSuccessResponse, TErrorResponse>(this System.Net.Http.HttpClient client, string uri, ILogger? logger = null,
        CancellationToken cancellationToken = default)
    {
        using var requestMessage = new System.Net.Http.HttpRequestMessage(HttpMethod.Get, uri);
        return await SendWithError<TSuccessResponse, TErrorResponse>(client, requestMessage, logger, cancellationToken).NoSync();
    }

    public static async ValueTask<(TSuccessResponse? SuccessResponse, TErrorResponse? ErrorResponse)> SendWithError<TSuccessResponse, TErrorResponse>(this System.Net.Http.HttpClient client, 
        HttpMethod httpMethod, string uri, object? request = null, ILogger? logger = null, CancellationToken cancellationToken = default)
    {
        using var requestMessage = new System.Net.Http.HttpRequestMessage(httpMethod, uri);

        if (request != null)
            requestMessage.Content = request.ToHttpContent();

        return await SendWithError<TSuccessResponse, TErrorResponse>(client, requestMessage, logger, cancellationToken).NoSync();
    }

    public static async ValueTask<(TSuccessResponse? SuccessResponse, TErrorResponse? ErrorResponse)> SendWithError<TSuccessResponse, TErrorResponse>(this System.Net.Http.HttpClient client, 
        System.Net.Http.HttpRequestMessage requestMessage, ILogger? logger = null, CancellationToken cancellationToken = default)
    {
        System.Net.Http.HttpResponseMessage response = await client.SendAsync(requestMessage, cancellationToken).NoSync();

        if (!response.IsSuccessStatusCode)
        {
            TErrorResponse? errorResponse = await response.To<TErrorResponse>(logger, cancellationToken).NoSync();
            return (default, errorResponse);
        }

        TSuccessResponse? successResponse = await response.To<TSuccessResponse>(logger, cancellationToken).NoSync();
        return (successResponse, default);
    }
}