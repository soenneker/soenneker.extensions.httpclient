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
    /// Sends with problem details.
    /// </summary>
    /// <typeparam name="TSuccessResponse">The TSuccessResponse type.</typeparam>
    /// <param name="client">The client.</param>
    /// <param name="uri">The uri.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task containing the result of the operation.</returns>
    [Obsolete("SendToResult should be used; removing soon")]
    public static ValueTask<(TSuccessResponse? SuccessResponse, ProblemDetailsDto? ErrorResponse)> SendWithProblemDetails<TSuccessResponse>(this System.Net.Http.HttpClient client, string uri,
        ILogger? logger = null, CancellationToken cancellationToken = default)
    {
        return client.SendWithError<TSuccessResponse, ProblemDetailsDto>(uri, logger, cancellationToken);
    }

    /// <summary>
    /// Sends with problem details.
    /// </summary>
    /// <typeparam name="TSuccessResponse">The TSuccessResponse type.</typeparam>
    /// <param name="client">The client.</param>
    /// <param name="httpMethod">The http method.</param>
    /// <param name="uri">The uri.</param>
    /// <param name="request">The request.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task containing the result of the operation.</returns>
    [Obsolete("SendToResult should be used; removing soon")]
    public static ValueTask<(TSuccessResponse? SuccessResponse, ProblemDetailsDto? ErrorResponse)> SendWithProblemDetails<TSuccessResponse>(this System.Net.Http.HttpClient client,
        HttpMethod httpMethod, string uri, object? request = null, ILogger? logger = null, CancellationToken cancellationToken = default)
    {
        return client.SendWithError<TSuccessResponse, ProblemDetailsDto>(httpMethod, uri, request, logger, cancellationToken);
    }

    /// <summary>
    /// Sends with problem details.
    /// </summary>
    /// <typeparam name="TSuccessResponse">The TSuccessResponse type.</typeparam>
    /// <param name="client">The client.</param>
    /// <param name="requestMessage">The request message.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task containing the result of the operation.</returns>
    [Obsolete("SendToResult should be used; removing soon")]
    public static ValueTask<(TSuccessResponse? SuccessResponse, ProblemDetailsDto? ErrorResponse)> SendWithProblemDetails<TSuccessResponse>(this System.Net.Http.HttpClient client,
        System.Net.Http.HttpRequestMessage requestMessage, ILogger? logger = null, CancellationToken cancellationToken = default)
    {
        return client.SendWithError<TSuccessResponse, ProblemDetailsDto>(requestMessage, logger, cancellationToken);
    }
}