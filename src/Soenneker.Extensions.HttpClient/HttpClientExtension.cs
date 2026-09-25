using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization.Metadata;
using Soenneker.Json.OptionsCollection;

namespace Soenneker.Extensions.HttpClient;

/// <summary>
/// A collection of helpful HttpClient extension methods, like retry and auto (de)serialization
/// </summary>
public static partial class HttpClientExtension
{
    [RequiresUnreferencedCode("JSON serialization uses reflection and may require types removed by trimming.")]
    [RequiresDynamicCode("JSON serialization may require runtime code generation.")]
    private static JsonTypeInfo<T> GetJsonTypeInfo<T>() => (JsonTypeInfo<T>)JsonOptionsCollection.WebOptions.GetTypeInfo(typeof(T));
}