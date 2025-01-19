using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Soenneker.Dtos.ProblemDetails;

namespace Soenneker.Extensions.HttpClient;

public static partial class HttpClientExtension
{
    public static ValueTask<(TSuccessResponse? SuccessResponse, ProblemDetailsDto? ErrorResponse)> SendWithProblemDetails<TSuccessResponse>(this System.Net.Http.HttpClient client, string uri,
        ILogger? logger = null, CancellationToken cancellationToken = default)
    {
        return SendWithError<TSuccessResponse, ProblemDetailsDto>(client, uri, logger, cancellationToken);
    }

    public static ValueTask<(TSuccessResponse? SuccessResponse, ProblemDetailsDto? ErrorResponse)> SendWithProblemDetails<TSuccessResponse>(this System.Net.Http.HttpClient client,
        HttpMethod httpMethod, string uri, object? request = null, ILogger? logger = null, CancellationToken cancellationToken = default)
    {
        return SendWithError<TSuccessResponse, ProblemDetailsDto>(client, httpMethod, uri, request, logger, cancellationToken);
    }

    public static ValueTask<(TSuccessResponse? SuccessResponse, ProblemDetailsDto? ErrorResponse)> SendWithProblemDetails<TSuccessResponse>(this System.Net.Http.HttpClient client,
        System.Net.Http.HttpRequestMessage requestMessage, ILogger? logger = null, CancellationToken cancellationToken = default)
    {
        return SendWithError<TSuccessResponse, ProblemDetailsDto>(client, requestMessage, logger, cancellationToken);
    }
}