// DeepSeek.cs — the one real model this sample talks to, over one POST.
//
// THE ADDRESS IS CODE, THE KEY IS NOT. `Endpoint` is a constant, so where a prompt goes is reviewed like
// any other line of this program and cannot be redirected by configuration. The key is read from the
// environment variable `LLM-API` and never written anywhere: not to a file, not to a log, not to output.
//
// OPENAI-SHAPED. DeepSeek's chat API takes `model`, `messages` and `response_format`, and answers with
// `choices[0].message.content`. JSON mode is asked for because both prompts want one JSON object back;
// the parsing in ModelAnswers.cs still assumes the model may ignore that.
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MLambda.AI.Airline.Llm;

public sealed class DeepSeek : ILlm
{
    /// <summary>Where every prompt goes. Hard-coded on purpose.</summary>
    public const string Endpoint = "https://api.deepseek.com/chat/completions";

    /// <summary>The environment variable the key is read from.</summary>
    public const string KeyVariable = "LLM-API";

    /// <summary>The same name for shells that cannot export a hyphen, such as bash.</summary>
    public const string KeyVariableAlias = "LLM_API";

    public const string Model = "deepseek-chat";

    private readonly HttpClient http;
    private readonly string key;

    public DeepSeek(HttpClient http, string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        this.http = http;
        this.key = key;
    }

    /// <summary>The key from the environment, or null when neither variable is set.</summary>
    ///
    /// <remarks>ON WINDOWS THE USER SCOPE IS READ TOO. A variable set with `setx` reaches only processes
    /// started after it, and an editor's terminal is usually older than that.</remarks>
    public static string? KeyFrom(Func<string, string?> read) =>
        new[] { KeyVariable, KeyVariableAlias }.Select(read).FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));

    public static string? ReadEnvironment(string name) =>
        Environment.GetEnvironmentVariable(name)
        ?? (OperatingSystem.IsWindows() ? Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.User) : null);

    public async Task<string> CompleteAsync(string system, string user, CancellationToken cancellationToken = default)
    {
        var body = new Request(
            Model,
            [new Message("system", system), new Message("user", user)],
            new Format("json_object"),
            Temperature: 0.2,
            Stream: false);

        // BUFFERED, WITH A LENGTH. `JsonContent` streams the body chunked, with no Content-Length, and DeepSeek's
        // gateway waits on such a request until the client gives up; a string body is sent whole.
        using var request = new HttpRequestMessage(HttpMethod.Post, Endpoint)
        {
            Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json"),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", key);

        try
        {
            using var response = await http.SendAsync(request, cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                // THE STATUS, NOT THE BODY. An error body can echo the request, and the request carries the key.
                throw new LlmUnavailableException($"DeepSeek answered {(int)response.StatusCode} {response.ReasonPhrase}.");
            }

            var answer = await response.Content.ReadFromJsonAsync<Response>(cancellationToken).ConfigureAwait(false);
            var content = answer?.Choices?.FirstOrDefault()?.Message?.Content;

            return string.IsNullOrWhiteSpace(content)
                ? throw new LlmUnavailableException("DeepSeek answered with no content.")
                : content;
        }
        catch (Exception fault) when (fault is HttpRequestException or TaskCanceledException or JsonException)
        {
            throw new LlmUnavailableException(fault is TaskCanceledException ? "DeepSeek did not answer in time." : $"DeepSeek could not be reached: {fault.Message}", fault);
        }
    }

    private sealed record Request(
        [property: JsonPropertyName("model")] string Model,
        [property: JsonPropertyName("messages")] IReadOnlyList<Message> Messages,
        [property: JsonPropertyName("response_format")] Format ResponseFormat,
        [property: JsonPropertyName("temperature")] double Temperature,
        [property: JsonPropertyName("stream")] bool Stream);

    private sealed record Message(
        [property: JsonPropertyName("role")] string Role,
        [property: JsonPropertyName("content")] string? Content);

    private sealed record Format([property: JsonPropertyName("type")] string Type);

    private sealed record Response([property: JsonPropertyName("choices")] IReadOnlyList<Choice>? Choices);

    private sealed record Choice([property: JsonPropertyName("message")] Message? Message);
}
