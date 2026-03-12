using Microsoft.Extensions.Logging;
using Soenneker.Dtos.ProblemDetails;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Soenneker.Extensions.HttpClient;

public static partial class HttpClientExtension
{
    [Obsolete("SendToResult should be used; removing soon")]
    public static ValueTask<(TSuccessResponse? SuccessResponse, ProblemDetailsDto? ErrorResponse)> SendWithProblemDetails<TSuccessResponse>(this System.Net.Http.HttpClient client, string uri,
        ILogger? logger = null, CancellationToken cancellationToken = default)
    {
        return client.SendWithError<TSuccessResponse, ProblemDetailsDto>(uri, logger, cancellationToken);
    }

    [Obsolete("SendToResult should be used; removing soon")]
    public static ValueTask<(TSuccessResponse? SuccessResponse, ProblemDetailsDto? ErrorResponse)> SendWithProblemDetails<TSuccessResponse>(this System.Net.Http.HttpClient client,
        HttpMethod httpMethod, string uri, object? request = null, ILogger? logger = null, CancellationToken cancellationToken = default)
    {
        return client.SendWithError<TSuccessResponse, ProblemDetailsDto>(httpMethod, uri, request, logger, cancellationToken);
    }

    [Obsolete("SendToResult should be used; removing soon")]
    public static ValueTask<(TSuccessResponse? SuccessResponse, ProblemDetailsDto? ErrorResponse)> SendWithProblemDetails<TSuccessResponse>(this System.Net.Http.HttpClient client,
        System.Net.Http.HttpRequestMessage requestMessage, ILogger? logger = null, CancellationToken cancellationToken = default)
    {
        return client.SendWithError<TSuccessResponse, ProblemDetailsDto>(requestMessage, logger, cancellationToken);
    }
}