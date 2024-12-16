[![](https://img.shields.io/nuget/v/soenneker.extensions.httpclient.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.extensions.httpclient/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.extensions.httpclient/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.extensions.httpclient/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/soenneker.extensions.httpclient.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.extensions.httpclient/)

# ![](https://user-images.githubusercontent.com/4441470/224455560-91ed3ee7-f510-4041-a8d2-3fc093025112.png) Soenneker.Extensions.HttpClient
### A collection of helpful HttpClient extension methods, like retry and auto (de)serialization

## Installation

```
dotnet add package Soenneker.Extensions.HttpClient
```

### SendToString()

```csharp
var request = new HttpRequestMessage(HttpMethod.Get, "https://api.example.com/data");
var responseContent = await _httpClient.SendToString(request);
```

### SendWithError()

```csharp
var request = new HttpRequestMessage(HttpMethod.Get, "https://api.example.com/data");
MySuccessResponse? successResponse, MyErrorResponse? errorResponse = await _httpClient.SendWithError<MySuccessResponse, MyErrorResponse>(request);
```

### TrySend()

```csharp
var request = new HttpRequestMessage(HttpMethod.Get, "https://api.example.com/data");
bool successful, HttpResponseMessage? response = await _httpClient.TrySend(request);
```

### SendWithRetryToType<ResponseType>()

```csharp
var requestData = new { Name = "John Doe" };
var response = await _httpClient.SendWithRetryToType<MyResponseType>(
    HttpMethod.Post,                 // HTTP Method
    "https://api.example.com/data",   // URI
    requestData,                     // Request body
    numberOfRetries: 3,              // Retry 3 times
    logger: _logger,                 // Optional logger
    baseDelay: TimeSpan.FromSeconds(2), // Exponential backoff
    log: true,                       // Enable logging
    cancellationToken: cancellationToken // Cancellation token
);
```

... and more