using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using Soenneker.Dtos.Results.Operation;
using Soenneker.Extensions.HttpRequestMessage;
using Soenneker.Extensions.HttpResponseMessage;
using Soenneker.Extensions.Object;
using Soenneker.Extensions.Task;
using Soenneker.Extensions.ValueTask;
using Soenneker.Utils.Random;
using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Soenneker.Constants.UserMessages;

namespace Soenneker.Extensions.HttpClient;

public static partial class HttpClientExtension
{
    /// <summary>
    /// Sends an HTTP GET request to the specified URI with retry logic, using exponential backoff and optional jitter for delays between retries.
    /// Returns an OperationResult containing either the success response or problem details.
    /// </summary>
    /// <typeparam name="TResponse">The type into which the HTTP response content will be deserialized. It is expected that the response can be deserialized into this type.</typeparam>
    /// <param name="client">The <see cref="System.Net.Http.HttpClient"/> instance used to send the HTTP GET request.</param>
    /// <param name="uri">The URI the request is sent to.</param>
    /// <param name="numberOfRetries">Optional. The number of times to retry the request in case of failure. Defaults to 2.</param>
    /// <param name="logger">Optional. The logger used to log warnings and errors during the request and retry process. Can be null if logging is not required.</param>
    /// <param name="baseDelay">Optional. The initial delay for the exponential backoff calculation between retries. If not provided, defaults to a system-defined value. Subsequent retries exponentially increase the delay based on this initial value.</param>
    /// <param name="log"></param>
    /// <param name="cancellationToken"></param>
    public static async ValueTask<OperationResult<TResponse>> SendToResultWithRetry<TResponse>(this System.Net.Http.HttpClient client, string uri,
        int numberOfRetries = 2, ILogger? logger = null, TimeSpan? baseDelay = null, bool log = true, CancellationToken cancellationToken = default)
    {
        using var request = new System.Net.Http.HttpRequestMessage(HttpMethod.Get, uri);
        return await SendToResultWithRetry<TResponse>(client, request, numberOfRetries, logger, baseDelay, log, cancellationToken)
            .NoSync();
    }

    /// <summary>
    /// Sends an HTTP request with the specified method, URI, and request body, incorporating retry logic with exponential backoff and optional jitter for delays between retries.
    /// Returns an OperationResult containing either the success response or problem details.
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
    public static async ValueTask<OperationResult<TResponse>> SendToResultWithRetry<TResponse>(this System.Net.Http.HttpClient client, HttpMethod httpMethod,
        string uri, object? request = null, int numberOfRetries = 2, ILogger? logger = null, TimeSpan? baseDelay = null, bool log = true,
        CancellationToken cancellationToken = default)
    {
        using var requestMessage = new System.Net.Http.HttpRequestMessage(httpMethod, uri);

        if (request != null)
            requestMessage.Content = request.ToHttpContent();

        return await SendToResultWithRetry<TResponse>(client, requestMessage, numberOfRetries, logger, baseDelay, log, cancellationToken)
            .NoSync();
    }

    /// <summary>
    /// Sends an HTTP request with retry logic, using exponential backoff and optional jitter for delays between retries.
    /// This method retries the request upon encountering specific exceptions or non-success HTTP response codes.
    /// Returns an OperationResult containing either the success response or problem details.
    /// </summary>
    /// <typeparam name="TResponse">The type into which the HTTP response content will be deserialized.</typeparam>
    /// <param name="client">The <see cref="System.Net.Http.HttpClient"/> instance used to send the request.</param>
    /// <param name="request">The <see cref="HttpRequestMessage"/> representing the HTTP request to be sent.</param>
    /// <param name="numberOfRetries">Optional. The number of times to retry the request in case of failure. Defaults to 2.</param>
    /// <param name="logger">Optional. The logger used to log warnings and errors. Can be null if logging is not required.</param>
    /// <param name="baseDelay">Optional. The initial delay for the exponential backoff calculation. If not provided, defaults to 2 seconds. Subsequent retries double the delay time.</param>
    /// <param name="log"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>A <see cref="ValueTask{T}"/> that represents the asynchronous operation, containing the OperationResult with either success response or problem details.</returns>
    /// <remarks>
    /// This method retries requests upon encountering an <see cref="HttpRequestException"/>, <see cref="JsonException"/>, or <see cref="InvalidOperationException"/> (the latter representing non-success status codes).
    /// Each retry delay is calculated based on exponential backoff strategy with optional jitter to prevent retry storms in distributed systems.
    /// </remarks>
    public static async ValueTask<OperationResult<TResponse>> SendToResultWithRetry<TResponse>(this System.Net.Http.HttpClient client,
        System.Net.Http.HttpRequestMessage request, int numberOfRetries = 2, ILogger? logger = null, TimeSpan? baseDelay = null, bool log = true,
        CancellationToken cancellationToken = default)
    {
        TimeSpan initialDelay = baseDelay ?? TimeSpan.FromSeconds(2);

        AsyncRetryPolicy retryPolicy = Policy.Handle<HttpRequestException>()
                                             .Or<InvalidOperationException>() // used to represent non-success status codes we want to retry
                                             .WaitAndRetryAsync(numberOfRetries,
                                                 retryAttempt => initialDelay * Math.Pow(2, retryAttempt - 1) +
                                                                 TimeSpan.FromMilliseconds(RandomUtil.Next(0, 1000)), (exception, timespan, retryAttempt, _) =>
                                                 {
                                                     if (!log)
                                                         return;

                                                     logger?.LogWarning(exception,
                                                         "HTTP attempt {RetryAttempt}: retrying after {DelaySeconds} seconds due to error.", retryAttempt,
                                                         timespan.TotalSeconds);
                                                 });

        try
        {
            OperationResult<TResponse> result = await retryPolicy.ExecuteAsync(async () =>
                                                                 {
                                                                     // You can only send a request once, so we have to clone it
                                                                     System.Net.Http.HttpRequestMessage clonedRequest = await request
                                                                         .Clone(cancellationToken: cancellationToken)
                                                                         .NoSync();

                                                                     System.Net.Http.HttpResponseMessage response = await client
                                                                         .SendAsync(clonedRequest, cancellationToken)
                                                                         .NoSync();

                                                                     if (!response.IsSuccessStatusCode)
                                                                     {
                                                                         // Use exception to drive Polly retry behavior
                                                                         throw new InvalidOperationException(
                                                                             $"HTTP request failed with status code: {response.StatusCode}");
                                                                     }

                                                                     // Let ToResult handle JSON / ProblemDetails / logging, etc.
                                                                     OperationResult<TResponse> operationResult = await response
                                                                         .ToResult<TResponse>(logger, cancellationToken)
                                                                         .NoSync();

                                                                     // We do NOT throw based on operationResult.Failed here;
                                                                     // the server responded successfully, so we surface the problem to the caller
                                                                     // instead of retrying for app-level errors.
                                                                     return operationResult;
                                                                 })
                                                                 .NoSync();

            return result;
        }
        catch (OperationCanceledException oce) when (cancellationToken.IsCancellationRequested)
        {
            logger?.LogWarning(oce, "HTTP request with retry to {Uri} was canceled.", request.RequestUri);

            return OperationResult.Fail<TResponse>(UserMessages.RequestCanceledTitle, UserMessages.RequestCanceledDetail, HttpStatusCode.RequestTimeout);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Unhandled exception during HTTP request with retry to {Uri}.", request.RequestUri);

            return OperationResult.Fail<TResponse>(UserMessages.SomethingWentWrongTitle, UserMessages.SomethingWentWrongDetail, HttpStatusCode.InternalServerError);
        }
    }
}