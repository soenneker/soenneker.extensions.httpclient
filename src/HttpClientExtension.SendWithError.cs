using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Soenneker.Extensions.Object;
using Soenneker.Extensions.Task;
using Soenneker.Extensions.ValueTask;
using Soenneker.Utils.Json;

namespace Soenneker.Extensions.HttpClient;

/// <summary>
/// A collection of helpful HttpClient extension methods, like retry and auto (de)serialization
/// </summary>
public static partial class HttpClientExtension
{
    public static ValueTask<(TSuccessResponse? SuccessResponse, TErrorResponse? ErrorResponse)> SendWithError<TSuccessResponse, TErrorResponse>(this System.Net.Http.HttpClient client, string uri, CancellationToken cancellationToken = default)
    {
        using var requestMessage = new System.Net.Http.HttpRequestMessage(HttpMethod.Get, uri);

        return SendWithError<TSuccessResponse, TErrorResponse>(client, requestMessage, cancellationToken);
    }

    public static async ValueTask<(TSuccessResponse? SuccessResponse, TErrorResponse? ErrorResponse)> SendWithError<TSuccessResponse, TErrorResponse>(this System.Net.Http.HttpClient client, 
        HttpMethod httpMethod, string uri, object? request = null, ILogger? logger = null, CancellationToken cancellationToken = default)
    {
        using var requestMessage = new System.Net.Http.HttpRequestMessage(httpMethod, uri);

        if (request != null)
        {
            try
            {
                requestMessage.Content = request.ToHttpContent();
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Could not build HttpRequestMessage for request type ({type})", request.GetType().Name);
                return (default, default);
            }
        }

        return await SendWithError<TSuccessResponse, TErrorResponse>(client, requestMessage, cancellationToken).NoSync();
    }

    public static async ValueTask<(TSuccessResponse? SuccessResponse, TErrorResponse? ErrorResponse)> SendWithError<TSuccessResponse, TErrorResponse>(this System.Net.Http.HttpClient client, 
        System.Net.Http.HttpRequestMessage requestMessage, CancellationToken cancellationToken = default)
    {
        System.Net.Http.HttpResponseMessage response = await client.SendAsync(requestMessage, cancellationToken).NoSync();

        string responseContent = await response.Content.ReadAsStringAsync(cancellationToken).NoSync();

        if (!response.IsSuccessStatusCode)
        {
            var errorResponse = JsonUtil.Deserialize<TErrorResponse>(responseContent);
            return (default, errorResponse);
        }

        var successResponse = JsonUtil.Deserialize<TSuccessResponse>(responseContent);
        return (successResponse, default);
    }
}