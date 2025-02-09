using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Soenneker.Extensions.Task;

namespace Soenneker.Extensions.HttpClient;

public static partial class HttpClientExtension
{
    /// <summary>
    /// Tries to send an HTTP request and returns a tuple indicating success or failure and the response message if successful.
    /// </summary>
    /// <param name="client">The <see cref="HttpClient"/> instance.</param>
    /// <param name="request">The <see cref="HttpRequestMessage"/> to send.</param>
    /// <param name="logger"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>A tuple containing a boolean for success and the <see cref="HttpResponseMessage"/> or null if the request failed.</returns>
    public static async ValueTask<(bool successful, System.Net.Http.HttpResponseMessage? response)> TrySend(this System.Net.Http.HttpClient client, System.Net.Http.HttpRequestMessage request,
        ILogger? logger = null, CancellationToken cancellationToken = default)
    {
        try
        {
            System.Net.Http.HttpResponseMessage response = await client.SendAsync(request, cancellationToken).NoSync();

            if (!response.IsSuccessStatusCode)
                logger?.LogError("HTTP request ({uri}) returned a non-successful status code ({statusCode})", request.RequestUri, response.StatusCode);

            return (response.IsSuccessStatusCode, response);
        }
        catch (OperationCanceledException)
        {
            logger?.LogWarning("HTTP request to {uri} was canceled.", request.RequestUri);
            return (false, null);
        }
        catch (Exception e)
        {
            logger?.LogError(e, "Exception sending request to {uri}", request.RequestUri);

            return (false, null);
        }
    }
}