using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Soenneker.Dtos.ProblemDetails;

namespace Soenneker.Extensions.HttpClient;

/// <summary>
/// Represents the http client extension.
/// </summary>
public static partial class HttpClientExtension
{
    /// <summary>
    /// Attempts to send an HTTP request and returns either the success payload or readable RFC 7807 problem details.
    /// </summary>
    /// <typeparam name="TSuccessResponse">The success payload type.</typeparam>
    /// <param name="client">The HTTP client used to send the request.</param>
    /// <param name="uri">The destination URI.</param>
    /// <param name="logger">An optional logger for request and conversion failures.</param>
    /// <param name="cancellationToken">Signals that the asynchronous operation should stop.</param>
    /// <returns>A tuple containing either the success value or problem details.</returns>
    [Obsolete("TrySendToResult should be used; removing soon")]
    public static ValueTask<(TSuccessResponse? SuccessResponse, ProblemDetailsDto? ErrorResponse)> TrySendWithProblemDetails<TSuccessResponse>(this System.Net.Http.HttpClient client, string uri,
        ILogger? logger = null, CancellationToken cancellationToken = default)
    {
        return client.TrySendWithError<TSuccessResponse, ProblemDetailsDto>(uri, logger, cancellationToken);
    }

    /// <summary>
    /// Attempts to send an HTTP request and returns either the success payload or readable RFC 7807 problem details.
    /// </summary>
    /// <typeparam name="TSuccessResponse">The success payload type.</typeparam>
    /// <param name="client">The HTTP client used to send the request.</param>
    /// <param name="httpMethod">The HTTP method used when constructing the request.</param>
    /// <param name="uri">The destination URI.</param>
    /// <param name="request">The request payload, or a prepared request message for the matching overload.</param>
    /// <param name="logger">An optional logger for request and conversion failures.</param>
    /// <param name="cancellationToken">Signals that the asynchronous operation should stop.</param>
    /// <returns>A tuple containing either the success value or problem details.</returns>
    [Obsolete("TrySendToResult should be used; removing soon")]
    public static ValueTask<(TSuccessResponse? SuccessResponse, ProblemDetailsDto? ErrorResponse)> TrySendWithProblemDetails<TSuccessResponse>(this System.Net.Http.HttpClient client,
        HttpMethod httpMethod, string uri, object? request = null, ILogger? logger = null, CancellationToken cancellationToken = default)
    {
        return client.TrySendWithError<TSuccessResponse, ProblemDetailsDto>(httpMethod, uri, request, logger, cancellationToken);
    }

    /// <summary>
    /// Attempts to send an HTTP request and returns either the success payload or readable RFC 7807 problem details.
    /// </summary>
    /// <typeparam name="TSuccessResponse">The success payload type.</typeparam>
    /// <param name="client">The HTTP client used to send the request.</param>
    /// <param name="requestMessage">The prepared HTTP request message; ownership follows <see cref="HttpClient.SendAsync(HttpRequestMessage, CancellationToken)"/>.</param>
    /// <param name="logger">An optional logger for request and conversion failures.</param>
    /// <param name="cancellationToken">Signals that the asynchronous operation should stop.</param>
    /// <returns>A tuple containing either the success value or problem details.</returns>
    [Obsolete("TrySendToResult should be used; removing soon")]
    public static ValueTask<(TSuccessResponse? SuccessResponse, ProblemDetailsDto? ErrorResponse)> TrySendWithProblemDetails<TSuccessResponse>(this System.Net.Http.HttpClient client,
        System.Net.Http.HttpRequestMessage requestMessage, ILogger? logger = null, CancellationToken cancellationToken = default)
    {
        return client.TrySendWithError<TSuccessResponse, ProblemDetailsDto>(requestMessage, logger, cancellationToken);
    }
}