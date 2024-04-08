using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Soenneker.Extensions.Object;
using Soenneker.Extensions.Task;
using Soenneker.Extensions.ValueTask;
using Soenneker.Utils.Json;

namespace Soenneker.Extensions.HttpClient;

public static partial class HttpClientExtension
{
    public static ValueTask<TResponse?> SendToType<TResponse>(this System.Net.Http.HttpClient client, string uri,
        ILogger? logger = null, CancellationToken cancellationToken = default)
    {
        using var request = new System.Net.Http.HttpRequestMessage(HttpMethod.Get, uri);

        return SendToType<TResponse>(client, request, logger, cancellationToken);
    }

    public static async ValueTask<TResponse?> SendToType<TRequest, TResponse>(this System.Net.Http.HttpClient client, HttpMethod httpMethod, string uri, TRequest request,
        ILogger? logger = null, CancellationToken cancellationToken = default)
    {
        using var requestMessage = new System.Net.Http.HttpRequestMessage(httpMethod, uri);

        try
        {
            requestMessage.Content = request.ToHttpContent();
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Could not build HttpRequestMessage for request type ({type})", typeof(TRequest).Name);
            return default;
        }

        return await SendToType<TResponse>(client, requestMessage, logger, cancellationToken).NoSync();
    }

    public static async ValueTask<TResponse?> SendToType<TResponse>(this System.Net.Http.HttpClient client, System.Net.Http.HttpRequestMessage request,
        ILogger? logger = null, CancellationToken cancellationToken = default)
    {
        TResponse? response = default;

        try
        {
            response = await SendToTypeInternal<TResponse>(client, request, logger, cancellationToken).NoSync();
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Aborting SendWithRetry, exhausted max retry attempts, returning null {type} response", typeof(TResponse).Name);
        }

        return response;
    }

    private static async ValueTask<TResponse?> SendToTypeInternal<TResponse>(this System.Net.Http.HttpClient client, System.Net.Http.HttpRequestMessage request,
        ILogger? logger, CancellationToken cancellationToken)
    {
        string? responseContent = null;

        try
        {
            System.Net.Http.HttpResponseMessage response = await client.SendAsync(request, cancellationToken).NoSync();

            if (!response.IsSuccessStatusCode)
            {
                logger?.LogError("HTTP request failed with status code: {statusCode}", response.StatusCode);
                return default;
            }

            responseContent = await response.Content.ReadAsStringAsync(cancellationToken).NoSync();
            var result = JsonUtil.Deserialize<TResponse>(responseContent);

            if (result == null)
            {
                logger?.LogWarning("Deserialization of type ({type}) resulted in null, content: {responseContent}",  typeof(TResponse).Name,responseContent );
                return default;
            }

            return result;
        }
        catch (JsonException jsonEx)
        {
            logger?.LogError(jsonEx, "Deserialization exception for type ({type}), content: {responseContent}", typeof(TResponse).Name, responseContent);
            return default;
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Exception occurred while sending HTTP request or reading response.");
            return default;
        }
    }
}