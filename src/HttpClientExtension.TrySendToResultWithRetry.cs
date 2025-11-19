using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Soenneker.Dtos.Results.Operation;
using Soenneker.Extensions.Object;
using Soenneker.Extensions.ValueTask;

namespace Soenneker.Extensions.HttpClient;

public static partial class HttpClientExtension
{
    /// <summary>
    /// Sends an HTTP GET request to the specified URI with retry logic, using exponential backoff and optional jitter for delays between retries.
    /// Returns an OperationResult containing either the success response or problem details, or null if all retries are exhausted.
    /// </summary>
    /// <typeparam name="TResponse">The type into which the HTTP response content will be deserialized. It is expected that the response can be deserialized into this type.</typeparam>
    /// <param name="client">The <see cref="System.Net.Http.HttpClient"/> instance used to send the HTTP GET request.</param>
    /// <param name="uri">The URI the request is sent to.</param>
    /// <param name="numberOfRetries">Optional. The number of times to retry the request in case of failure. Defaults to 2.</param>
    /// <param name="logger">Optional. The logger used to log warnings and errors during the request and retry process. Can be null if logging is not required.</param>
    /// <param name="baseDelay">Optional. The initial delay for the exponential backoff calculation between retries. If not provided, defaults to a system-defined value. Subsequent retries exponentially increase the delay based on this initial value.</param>
    /// <param name="log"></param>
    /// <param name="cancellationToken"></param>
    public static async ValueTask<OperationResult<TResponse>> TrySendToResultWithRetry<TResponse>(this System.Net.Http.HttpClient client, string uri, int numberOfRetries = 2,
        ILogger? logger = null, TimeSpan? baseDelay = null, bool log = true, CancellationToken cancellationToken = default)
    {
        using var request = new System.Net.Http.HttpRequestMessage(HttpMethod.Get, uri);
        return await TrySendToResultWithRetry<TResponse>(client, request, numberOfRetries, logger, baseDelay, log, cancellationToken).NoSync();
    }

    /// <summary>
    /// Sends an HTTP request with the specified method, URI, and request body, incorporating retry logic with exponential backoff and optional jitter for delays between retries.
    /// Returns an OperationResult containing either the success response or problem details, or null if all retries are exhausted.
    /// </summary>
    /// <typeparam name="TResponse">The type into which the HTTP response content will be deserialized. It is expected that the response can be deserialized into this type.</typeparam>
    /// <param name="client">The <see cref="System.Net.Http.HttpClient"/> instance used to send the HTTP request.</param>
    /// <param name="httpMethod">The HTTP method to be used for the request.</param>
    /// <param name="uri">The URI the request is sent to.</param>
    /// <param name="request">The request body to be serialized and sent with the request.</param>
    /// <param name="numberOfRetries">Optional. The number of times to retry the request in case of failure. Defaults to 2.</param>
    /// <param name="logger">Optional. The logger used to log warnings, errors, and retry operations during the process. Can be null if logging is not desired.</param>
    /// <param name="baseDelay">Optional. The initial delay for the exponential backoff calculation between retries. If not provided, defaults to a system-defined value. Each subsequent retry exponentially increases the delay based on this initial value.</param>
    /// <param name="log"></param>
    /// <param name="cancellationToken"></param>
    public static async ValueTask<OperationResult<TResponse>> TrySendToResultWithRetry<TResponse>(this System.Net.Http.HttpClient client, HttpMethod httpMethod, string uri, object? request = null, int numberOfRetries = 2,
        ILogger? logger = null, TimeSpan? baseDelay = null, bool log = true, CancellationToken cancellationToken = default)
    {
        using var requestMessage = new System.Net.Http.HttpRequestMessage(httpMethod, uri);

        if (request != null)
            requestMessage.Content = request.TryToHttpContent();

        return await TrySendToResultWithRetry<TResponse>(client, requestMessage, numberOfRetries, logger, baseDelay, log, cancellationToken).NoSync();
    }

    /// <summary>
    /// Sends an HTTP request with retry logic, using exponential backoff and optional jitter for delays between retries.
    /// This method retries the request upon encountering specific exceptions or non-success HTTP response codes.
    /// Returns an OperationResult containing either the success response or problem details, or null if all retries are exhausted.
    /// </summary>
    /// <typeparam name="TResponse">The type into which the HTTP response content will be deserialized.</typeparam>
    /// <param name="client">The <see cref="System.Net.Http.HttpClient"/> instance used to send the request.</param>
    /// <param name="request">The <see cref="HttpRequestMessage"/> representing the HTTP request to be sent.</param>
    /// <param name="numberOfRetries">Optional. The number of times to retry the request in case of failure. Defaults to 2.</param>
    /// <param name="logger">Optional. The logger used to log warnings and errors. Can be null if logging is not required.</param>
    /// <param name="baseDelay">Optional. The initial delay for the exponential backoff calculation. If not provided, defaults to 2 seconds. Subsequent retries double the delay time.</param>
    /// <param name="log"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>A <see cref="ValueTask{T}"/> that represents the asynchronous operation, containing the OperationResult with either success response or problem details, or null if all retries are exhausted.</returns>
    /// <remarks>
    /// This method retries requests upon encountering an <see cref="HttpRequestException"/>, <see cref="JsonException"/>, or <see cref="InvalidOperationException"/> (the latter representing non-success status codes).
    /// Each retry delay is calculated based on exponential backoff strategy with optional jitter to prevent retry storms in distributed systems.
    /// </remarks>
    public static async ValueTask<OperationResult<TResponse>> TrySendToResultWithRetry<TResponse>(this System.Net.Http.HttpClient client, System.Net.Http.HttpRequestMessage request,
        int numberOfRetries = 2, ILogger? logger = null, TimeSpan? baseDelay = null, bool log = true, CancellationToken cancellationToken = default)
    {
        try
        {
            return await SendToResultWithRetry<TResponse>(client, request, numberOfRetries, logger, baseDelay, log, cancellationToken).NoSync();
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Exhausted all retry attempts for the HTTP request, returning null");
            return null;
        }
    }
}
