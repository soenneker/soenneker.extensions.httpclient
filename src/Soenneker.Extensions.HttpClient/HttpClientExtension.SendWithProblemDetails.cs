using Microsoft.Extensions.Logging;
using Soenneker.Dtos.ProblemDetails;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Soenneker.Extensions.HttpClient;

/// <summary>
/// Represents the http client extension.
/// </summary>
public static partial class HttpClientExtension
{
    /// <summary>
    /// Sends an HTTP request and deserializes either the success payload or an RFC 7807 problem-details payload.
    /// </summary>
    /// <typeparam name="TSuccessResponse">The success payload type.</typeparam>
    /// <param name="client">The HTTP client used to send the request.</param>
    /// <param name="uri">The destination URI.</param>
    /// <param name="logger">An optional logger for request and conversion failures.</param>
    /// <param name="cancellationToken">Signals that the asynchronous operation should stop.</param>
    /// <returns>A tuple containing either the success value or problem details.</returns>
    [Obsolete("SendToResult should be used; removing soon")]
    public static ValueTask<(TSuccessResponse? SuccessResponse, ProblemDetailsDto? ErrorResponse)> SendWithProblemDetails<TSuccessResponse>(this System.Net.Http.HttpClient client, string uri,
        ILogger? logger = null, CancellationToken cancellationToken = default)
    {
        return client.SendWithError<TSuccessResponse, ProblemDetailsDto>(uri, logger, cancellationToken);
    }

    /// <summary>
    /// Sends an HTTP request and deserializes either the success payload or an RFC 7807 problem-details payload.
    /// </summary>
    /// <typeparam name="TSuccessResponse">The success payload type.</typeparam>
    /// <param name="client">The HTTP client used to send the request.</param>
    /// <param name="httpMethod">The HTTP method used when constructing the request.</param>
    /// <param name="uri">The destination URI.</param>
    /// <param name="request">The request payload, or a prepared request message for the matching overload.</param>
    /// <param name="logger">An optional logger for request and conversion failures.</param>
    /// <param name="cancellationToken">Signals that the asynchronous operation should stop.</param>
    /// <returns>A tuple containing either the success value or problem details.</returns>
    [Obsolete("SendToResult should be used; removing soon")]
    public static ValueTask<(TSuccessResponse? SuccessResponse, ProblemDetailsDto? ErrorResponse)> SendWithProblemDetails<TSuccessResponse>(this System.Net.Http.HttpClient client,
        HttpMethod httpMethod, string uri, object? request = null, ILogger? logger = null, CancellationToken cancellationToken = default)
    {
        return client.SendWithError<TSuccessResponse, ProblemDetailsDto>(httpMethod, uri, request, logger, cancellationToken);
    }

    /// <summary>
    /// Sends an HTTP request and deserializes either the success payload or an RFC 7807 problem-details payload.
    /// </summary>
    /// <typeparam name="TSuccessResponse">The success payload type.</typeparam>
    /// <param name="client">The HTTP client used to send the request.</param>
    /// <param name="requestMessage">The prepared HTTP request message; ownership follows <see cref="HttpClient.SendAsync(HttpRequestMessage, CancellationToken)"/>.</param>
    /// <param name="logger">An optional logger for request and conversion failures.</param>
    /// <param name="cancellationToken">Signals that the asynchronous operation should stop.</param>
    /// <returns>A tuple containing either the success value or problem details.</returns>
    [Obsolete("SendToResult should be used; removing soon")]
    public static ValueTask<(TSuccessResponse? SuccessResponse, ProblemDetailsDto? ErrorResponse)> SendWithProblemDetails<TSuccessResponse>(this System.Net.Http.HttpClient client,
        System.Net.Http.HttpRequestMessage requestMessage, ILogger? logger = null, CancellationToken cancellationToken = default)
    {
        return client.SendWithError<TSuccessResponse, ProblemDetailsDto>(requestMessage, logger, cancellationToken);
    }
}