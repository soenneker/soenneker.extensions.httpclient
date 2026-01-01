using System;
using System.Net.Http;
using System.Text.Json;
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
    public static async ValueTask<TResponse?> TrySendToType<TResponse>(this System.Net.Http.HttpClient client, string uri, ILogger? logger = null, CancellationToken cancellationToken = default)
    {
        using var request = new System.Net.Http.HttpRequestMessage(HttpMethod.Get, uri);
        return await client.TrySendToType<TResponse>(request, logger, cancellationToken).NoSync();
    }

    public static async ValueTask<TResponse?> TrySendToType<TResponse>(this System.Net.Http.HttpClient client, HttpMethod httpMethod, string uri, object? request = null,
        ILogger? logger = null, CancellationToken cancellationToken = default)
    {
        using var requestMessage = new System.Net.Http.HttpRequestMessage(httpMethod, uri);

        if (request != null)
            requestMessage.Content = request.TryToHttpContent();

        return await client.TrySendToType<TResponse>(requestMessage, logger, cancellationToken).NoSync();
    }

    public static async ValueTask<TResponse?> TrySendToType<TResponse>(this System.Net.Http.HttpClient client, System.Net.Http.HttpRequestMessage request, ILogger? logger,
        CancellationToken cancellationToken = default)
    {
        string? responseContent = null;

        try
        {
            using System.Net.Http.HttpResponseMessage response = await client.SendAsync(request, cancellationToken).NoSync();

            if (!response.IsSuccessStatusCode)
            {
                logger?.LogError("HTTP request ({uri}) returned a non-successful status code ({statusCode})", request.RequestUri, response.StatusCode);
                return default;
            }

            return await response.ToStrict<TResponse>(cancellationToken: cancellationToken).NoSync();
        }
        catch (OperationCanceledException)
        {
            logger?.LogWarning("HTTP request to {uri} was canceled.", request.RequestUri);
            return default;
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