using Microsoft.Extensions.Logging;
using Soenneker.Dtos.Results.Operation;
using Soenneker.Extensions.HttpResponseMessage;
using Soenneker.Extensions.Object;
using Soenneker.Extensions.Task;
using Soenneker.Extensions.ValueTask;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Soenneker.Constants.UserMessages;

namespace Soenneker.Extensions.HttpClient;

public static partial class HttpClientExtension
{
    public static async ValueTask<OperationResult<TResponse>> TrySendToResult<TResponse>(this System.Net.Http.HttpClient client, string uri,
        ILogger? logger = null, CancellationToken cancellationToken = default)
    {
        using var request = new System.Net.Http.HttpRequestMessage(HttpMethod.Get, uri);
        return await client.TrySendToResult<TResponse>(request, logger, cancellationToken)
                           .NoSync();
    }

    public static async ValueTask<OperationResult<TResponse>> TrySendToResult<TResponse>(this System.Net.Http.HttpClient client, HttpMethod httpMethod,
        string uri, object? request = null, ILogger? logger = null, CancellationToken cancellationToken = default)
    {
        using var requestMessage = new System.Net.Http.HttpRequestMessage(httpMethod, uri);

        if (request != null)
            requestMessage.Content = request.TryToHttpContent();

        return await client.TrySendToResult<TResponse>(requestMessage, logger, cancellationToken)
                           .NoSync();
    }

    public static async ValueTask<OperationResult<TResponse>> TrySendToResult<TResponse>(this System.Net.Http.HttpClient client,
        System.Net.Http.HttpRequestMessage request, ILogger? logger, CancellationToken cancellationToken = default)
    {
        try
        {
            using System.Net.Http.HttpResponseMessage response = await client.SendAsync(request, cancellationToken)
                                                                       .NoSync();

            if (!response.IsSuccessStatusCode)
            {
                logger?.LogWarning("HTTP request to {Uri} returned a non-success status code {StatusCode}", request.RequestUri, response.StatusCode);
            }

            return await response.ToResult<TResponse>(logger, cancellationToken)
                                 .NoSync();
        }
        catch (OperationCanceledException oce) when (cancellationToken.IsCancellationRequested)
        {
            logger?.LogWarning(oce, "HTTP request to {Uri} was canceled.", request.RequestUri);

            return OperationResult.Fail<TResponse>(UserMessages.RequestCanceledTitle, UserMessages.RequestCanceledDetail, HttpStatusCode.RequestTimeout);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Exception occurred while sending HTTP request to {Uri}.", request.RequestUri);

            return OperationResult.Fail<TResponse>(UserMessages.SomethingWentWrongTitle, UserMessages.SomethingWentWrongDetail, HttpStatusCode.InternalServerError);
        }
    }
}