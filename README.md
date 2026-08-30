[![](https://img.shields.io/nuget/v/soenneker.extensions.httpclient.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.extensions.httpclient/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.extensions.httpclient/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.extensions.httpclient/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/soenneker.extensions.httpclient.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.extensions.httpclient/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.extensions.httpclient/codeql.yml?label=CodeQL&style=for-the-badge)](https://github.com/soenneker/soenneker.extensions.httpclient/actions/workflows/codeql.yml)

# ![](https://user-images.githubusercontent.com/4441470/224455560-91ed3ee7-f510-4041-a8d2-3fc093025112.png) Soenneker.Extensions.HttpClient

HTTP send helpers for raw strings, JSON payloads, typed success/error bodies, `OperationResult<T>`, exception-suppressing calls, and retry with cloned requests.

## Installation

```bash
dotnet add package Soenneker.Extensions.HttpClient
```

Use these extensions with a long-lived or factory-managed `HttpClient`; do not create and dispose a new client for every call.

## Read a response body

```csharp
using Soenneker.Extensions.HttpClient;

string body = await client.SendToString(
    "https://api.example.com/health",
    logger,
    cancellationToken);
```

`SendToString()` returns the body for both successful and non-successful HTTP statuses. A non-success status is logged when a logger is supplied, but does not itself throw. Transport, content-read, timeout, and cancellation failures propagate.

## Deserialize a successful JSON response

```csharp
Customer customer = await client.SendToType<Customer>(
    "https://api.example.com/customers/42",
    logger,
    cancellationToken);
```

`SendToType<T>()` requires a successful HTTP status and a non-null JSON value of `T`. Non-success status, transport, cancellation, empty-body, and deserialization failures throw. Overloads accept a URI for GET, an HTTP method plus an optional object body, or a prepared `HttpRequestMessage`. Object bodies are serialized as HTTP content.

## Keep HTTP failures as data

```csharp
OperationResult<Customer> result = await client.SendToResult<Customer>(
    "https://api.example.com/customers/42",
    logger,
    cancellationToken);

(Customer? success, ApiError? error) = await client.SendWithError<Customer, ApiError>(
    "https://api.example.com/customers/42",
    logger,
    cancellationToken);
```

`SendToResult<T>()` converts successful JSON to `Value`, 204 responses to an empty result, and non-success JSON problem details to `Problem`. Conversion failures become failed operation results, while request-send failures still propagate.

`SendWithError<TSuccess,TError>()` chooses the payload type from `IsSuccessStatusCode`; exactly one tuple side is normally populated. `SendWithProblemDetails<TSuccess>()` is the same pattern with `ProblemDetailsDto` as the error type. Transport and conversion failures propagate from the non-`Try` variants.

## Suppress request exceptions deliberately

The `Try*` methods log failures when a logger is supplied and return a sentinel instead of throwing:

- `TrySend()` returns `(false, null)` for an exception or cancellation. For an HTTP non-success response it returns `(false, response)`; the caller must dispose that response.
- `TrySendToString()` returns `(false, body)` for an HTTP error response and `(false, null)` for an exception or cancellation.
- `TrySendToType<T>()` returns `default(T)` for HTTP errors, cancellation, request failures, or conversion failures.
- `TrySendWithError()` and `TrySendWithProblemDetails()` return default tuples when sending or conversion fails.
- `TrySendToResult<T>()` returns a failed result: cancellation maps to HTTP 408 and other caught failures map to HTTP 500.

Because several `Try*` methods collapse cancellation and failure into null/default values, use the strict variants when callers must distinguish those outcomes.

## Retry

```csharp
Customer customer = await client.SendToTypeWithRetry<Customer>(
    HttpMethod.Get,
    "https://api.example.com/customers/42",
    numberOfRetries: 2,
    logger: logger,
    baseDelay: TimeSpan.FromSeconds(1),
    cancellationToken: cancellationToken);
```

Retry variants clone the request for each attempt, allowing buffered request content to be sent again. `numberOfRetries` counts retries after the initial attempt, so `2` allows up to three sends. Delays use exponential backoff from `baseDelay` (two seconds by default) plus 0–999 ms of jitter.

Retries occur for `HttpRequestException` and every non-success HTTP status. That includes non-transient 4xx responses. The helper does not inspect `Retry-After`, and it does not know whether a POST or other operation is idempotent; choose retry counts only when repeating the request is safe. Cancellation stops strict retry methods. `Try*WithRetry` catches the exhausted failure—including cancellation—and returns its normal null/default sentinel.

## Ownership

URI and method/body overloads create and dispose their own request messages. Prepared `HttpRequestMessage` overloads leave the original request owned by the caller. Methods that deserialize or read content dispose their `HttpResponseMessage` internally.

`SendWithRetry()` and `TrySendWithRetry()` return a live `HttpResponseMessage`; the caller must dispose it. Failed responses encountered during retry are disposed before the next attempt.
