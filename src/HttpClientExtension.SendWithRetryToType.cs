using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using Soenneker.Extensions.HttpRequestMessage;
using Soenneker.Extensions.Object;
using Soenneker.Extensions.Task;
using Soenneker.Extensions.ValueTask;
using Soenneker.Utils.Json;
using Soenneker.Utils.Random;

namespace Soenneker.Extensions.HttpClient;

public static partial class HttpClientExtension
{
    /// <summary>
    /// Sends an HTTP GET request to the specified URI with retry logic, using exponential backoff and optional jitter for delays between retries.
    /// </summary>
    /// <typeparam name="TResponse">The type into which the HTTP response content will be deserialized. It is expected that the response can be deserialized into this type.</typeparam>
    /// <param name="client">The <see cref="System.Net.Http.HttpClient"/> instance used to send the HTTP GET request.</param>
    /// <param name="uri">The URI the request is sent to.</param>
    /// <param name="numberOfRetries">Optional. The number of times to retry the request in case of failure. Defaults to 2.</param>
    /// <param name="logger">Optional. The logger used to log warnings and errors during the request and retry process. Can be null if logging is not required.</param>
    /// <param name="baseDelay">Optional. The initial delay for the exponential backoff calculation between retries. If not provided, defaults to a system-defined value. Subsequent retries exponentially increase the delay based on this initial value.</param>
    /// <param name="log"></param>
    /// <param name="cancellationToken"></param>
    public static ValueTask<TResponse?> SendWithRetryToType<TResponse>(this System.Net.Http.HttpClient client, string uri, int numberOfRetries = 2,
        ILogger? logger = null, TimeSpan? baseDelay = null, bool log = true, CancellationToken cancellationToken = default)
    {
        using var request = new System.Net.Http.HttpRequestMessage(HttpMethod.Get, uri);

        return SendWithRetryToType<TResponse>(client, request, numberOfRetries, logger, baseDelay, log, cancellationToken);
    }

    /// <summary>
    /// Sends an HTTP request with the specified method, URI, and request body, incorporating retry logic with exponential backoff and optional jitter for delays between retries.
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
    public static async ValueTask<TResponse?> SendWithRetryToType<TResponse>(this System.Net.Http.HttpClient client, HttpMethod httpMethod, string uri, object request, int numberOfRetries = 2,
        ILogger? logger = null, TimeSpan? baseDelay = null, bool log = true, CancellationToken cancellationToken = default)
    {
        using var requestMessage = new System.Net.Http.HttpRequestMessage(httpMethod, uri);

        try
        {
            requestMessage.Content = request.ToHttpContent();
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Could not build HttpRequestMessage for request type ({type})", request.GetType().Name);
            return default;
        }

        return await SendWithRetryToType<TResponse>(client, requestMessage, numberOfRetries, logger, baseDelay, log, cancellationToken).NoSync();
    }

    /// <summary>
    /// Sends an HTTP request with retry logic, using exponential backoff and optional jitter for delays between retries.
    /// This method retries the request upon encountering specific exceptions or non-success HTTP response codes.
    /// </summary>
    /// <typeparam name="TResponse">The type into which the HTTP response content will be deserialized.</typeparam>
    /// <param name="client">The <see cref="System.Net.Http.HttpClient"/> instance used to send the request.</param>
    /// <param name="request">The <see cref="HttpRequestMessage"/> representing the HTTP request to be sent.</param>
    /// <param name="numberOfRetries">Optional. The number of times to retry the request in case of failure. Defaults to 2.</param>
    /// <param name="logger">Optional. The logger used to log warnings and errors. Can be null if logging is not required.</param>
    /// <param name="baseDelay">Optional. The initial delay for the exponential backoff calculation. If not provided, defaults to 2 seconds. Subsequent retries double the delay time.</param>
    /// <param name="log"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>A <see cref="ValueTask{TResponse}"/> that represents the asynchronous operation, containing the deserialized response if successful, or null if the request ultimately fails after all retries.</returns>
    /// <remarks>
    /// This method retries requests upon encountering an <see cref="HttpRequestException"/>, <see cref="JsonException"/>, or <see cref="InvalidOperationException"/> (the latter representing non-success status codes).
    /// Each retry delay is calculated based on exponential backoff strategy with optional jitter to prevent retry storms in distributed systems.
    /// </remarks>
    public static async ValueTask<TResponse?> SendWithRetryToType<TResponse>(this System.Net.Http.HttpClient client, System.Net.Http.HttpRequestMessage request,
        int numberOfRetries = 2, ILogger? logger = null, TimeSpan? baseDelay = null, bool log = true, CancellationToken cancellationToken = default)
    {
        TResponse? response = default;

        try
        {
            response = await SendWithRetryToTypeInternal<TResponse>(client, request, numberOfRetries, logger, baseDelay, log, cancellationToken).NoSync();
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Aborting SendWithRetry, exhausted max retry attempts, returning null {type} response", typeof(TResponse).Name);
        }

        return response;
    }

    private static async ValueTask<TResponse?> SendWithRetryToTypeInternal<TResponse>(this System.Net.Http.HttpClient client, System.Net.Http.HttpRequestMessage request,
        int numberOfRetries, ILogger? logger, TimeSpan? baseDelay, bool log, CancellationToken cancellationToken)
    {
        TimeSpan initialDelay = baseDelay ?? TimeSpan.FromSeconds(2);

        AsyncRetryPolicy? retryPolicy = Policy
            .Handle<HttpRequestException>()
            .Or<JsonException>()
            .Or<InvalidOperationException>()
            .WaitAndRetryAsync(numberOfRetries,
                retryAttempt => initialDelay * Math.Pow(2, retryAttempt - 1) + TimeSpan.FromMilliseconds(RandomUtil.Next(0, 1000)),
                (exception, timespan, retryAttempt, _) =>
                {
                    if (log)
                        logger?.LogWarning("HTTP Attempt {retryAttempt}: Retrying after {timespan} seconds due to error: {message}", retryAttempt, timespan.TotalSeconds, exception.Message);
                });

        TResponse result = await retryPolicy.ExecuteAsync(async () =>
        {
            System.Net.Http.HttpRequestMessage
                clonedRequest = await request.Clone().NoSync(); // Unfortunately we need to clone the original request and send that one because you can only send a request once

            System.Net.Http.HttpResponseMessage response = await client.SendAsync(clonedRequest, cancellationToken).NoSync();

            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException($"HTTP request failed with status code: {response.StatusCode}");

            string? responseContent = null;

            try
            {
                responseContent = await response.Content.ReadAsStringAsync(cancellationToken).NoSync();

                var result = JsonUtil.Deserialize<TResponse>(responseContent);

                if (result != null) 
                    return result;

                if (log)
                    throw new JsonException($"Deserialization of type ({typeof(TResponse).Name}) resulted in null, content: {responseContent}");

                throw new JsonException($"Deserialization of type ({typeof(TResponse).Name}) resulted in null");
            }
            catch (JsonException jsonEx)
            {
                logger?.LogError(jsonEx, "Deserialization exception for type ({type})", typeof(TResponse).Name);

                if (log)
                    throw new JsonException($"Deserialization of type ({typeof(TResponse).Name}) failed, content: {responseContent}");

                throw new JsonException($"Deserialization of type ({typeof(TResponse).Name}) failed");
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "General exception reading content");
                throw;
            }
        }).NoSync();

        return result;
    }
}