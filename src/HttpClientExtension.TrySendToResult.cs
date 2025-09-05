using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Soenneker.Dtos.Results.Operation;
using Soenneker.Extensions.HttpResponseMessage;
using Soenneker.Extensions.Object;
using Soenneker.Extensions.Task;
using Soenneker.Extensions.ValueTask;

namespace Soenneker.Extensions.HttpClient;

public static partial class HttpClientExtension
{
    public static async ValueTask<OperationResult<TResponse>?> TrySendToResult<TResponse>(this System.Net.Http.HttpClient client, string uri, ILogger? logger = null, CancellationToken cancellationToken = default)
    {
        using var request = new System.Net.Http.HttpRequestMessage(HttpMethod.Get, uri);
        return await TrySendToResult<TResponse>(client, request, logger, cancellationToken).NoSync();
    }

    public static async ValueTask<OperationResult<TResponse>?> TrySendToResult<TResponse>(this System.Net.Http.HttpClient client, HttpMethod httpMethod, string uri, object? request = null,
        ILogger? logger = null, CancellationToken cancellationToken = default)
    {
        using var requestMessage = new System.Net.Http.HttpRequestMessage(httpMethod, uri);

        if (request != null)
            requestMessage.Content = request.TryToHttpContent();

        return await TrySendToResult<TResponse>(client, requestMessage, logger, cancellationToken).NoSync();
    }

    public static async ValueTask<OperationResult<TResponse>?> TrySendToResult<TResponse>(this System.Net.Http.HttpClient client, System.Net.Http.HttpRequestMessage request, ILogger? logger,
        CancellationToken cancellationToken = default)
    {
        try
        {
            System.Net.Http.HttpResponseMessage response = await client.SendAsync(request, cancellationToken).NoSync();

            if (!response.IsSuccessStatusCode)
            {
                logger?.LogError("HTTP request ({uri}) returned a non-successful status code ({statusCode})", request.RequestUri, response.StatusCode);
            }

            return await response.ToResult<TResponse>(logger, cancellationToken).NoSync();
        }
        catch (OperationCanceledException)
        {
            logger?.LogWarning("HTTP request to {uri} was canceled.", request.RequestUri);
            return null;
        }
        catch (JsonException jsonEx)
        {
            logger?.LogError(jsonEx, "Deserialization exception for type ({type})", typeof(TResponse).Name);
            return null;
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Exception occurred while sending HTTP request or reading response.");
            return null;
        }
    }
}
