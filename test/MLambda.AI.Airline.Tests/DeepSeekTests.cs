// DeepSeekTests.cs — the one POST, checked without the network.
namespace MLambda.AI.Airline.Tests;

using System.Net;
using System.Text.Json;

public class DeepSeekTests
{
    private const string Key = "sk-test-not-a-real-key";

    private sealed class Wire(HttpStatusCode status, string body) : HttpMessageHandler
    {
        public HttpRequestMessage? Sent { get; private set; }

        public string SentBody { get; private set; } = string.Empty;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Sent = request;
            SentBody = await request.Content!.ReadAsStringAsync(cancellationToken);

            return new HttpResponseMessage(status) { Content = new StringContent(body) };
        }
    }

    private static string Answer(string content) =>
        JsonSerializer.Serialize(new { choices = new[] { new { message = new { role = "assistant", content } } } });

    [Fact]
    public async Task It_posts_to_the_hard_coded_endpoint_with_the_key_as_a_bearer_token()
    {
        var wire = new Wire(HttpStatusCode.OK, Answer("{}"));

        await new DeepSeek(new HttpClient(wire), Key).CompleteAsync("sys", "usr");

        Assert.Equal(HttpMethod.Post, wire.Sent!.Method);
        Assert.Equal(new Uri(DeepSeek.Endpoint), wire.Sent.RequestUri);
        Assert.Equal("https://api.deepseek.com/chat/completions", DeepSeek.Endpoint);
        Assert.Equal("Bearer", wire.Sent.Headers.Authorization!.Scheme);
        Assert.Equal(Key, wire.Sent.Headers.Authorization.Parameter);
    }

    [Fact]
    public async Task The_endpoint_ignores_any_base_address_the_client_was_given()
    {
        var wire = new Wire(HttpStatusCode.OK, Answer("{}"));
        var client = new HttpClient(wire) { BaseAddress = new Uri("https://example.invalid/") };

        await new DeepSeek(client, Key).CompleteAsync("sys", "usr");

        Assert.Equal(new Uri(DeepSeek.Endpoint), wire.Sent!.RequestUri);
    }

    [Fact]
    public async Task The_body_is_a_json_mode_chat_request_with_both_prompts_and_no_key()
    {
        var wire = new Wire(HttpStatusCode.OK, Answer("{}"));

        await new DeepSeek(new HttpClient(wire), Key).CompleteAsync("the system prompt", "the user prompt");

        using var body = JsonDocument.Parse(wire.SentBody);
        var root = body.RootElement;

        Assert.Equal(DeepSeek.Model, root.GetProperty("model").GetString());
        Assert.Equal("json_object", root.GetProperty("response_format").GetProperty("type").GetString());
        Assert.False(root.GetProperty("stream").GetBoolean());
        Assert.Equal(
            [("system", "the system prompt"), ("user", "the user prompt")],
            root.GetProperty("messages").EnumerateArray().Select(m => (m.GetProperty("role").GetString(), m.GetProperty("content").GetString())));
        Assert.DoesNotContain(Key, wire.SentBody);
    }

    [Fact]
    public async Task It_returns_the_first_choice_s_content()
    {
        var wire = new Wire(HttpStatusCode.OK, Answer("""{"reply": "hi"}"""));

        Assert.Equal("""{"reply": "hi"}""", await new DeepSeek(new HttpClient(wire), Key).CompleteAsync("s", "u"));
    }

    [Fact]
    public async Task A_refusal_is_unavailable_and_the_message_carries_the_status_not_the_body()
    {
        var wire = new Wire(HttpStatusCode.Unauthorized, $$"""{"error": "bad key {{Key}}"}""");

        var failure = await Assert.ThrowsAsync<LlmUnavailableException>(() => new DeepSeek(new HttpClient(wire), Key).CompleteAsync("s", "u"));

        Assert.Contains("401", failure.Message);
        Assert.DoesNotContain(Key, failure.Message);
    }

    [Fact]
    public async Task An_empty_answer_is_unavailable()
    {
        var wire = new Wire(HttpStatusCode.OK, """{"choices": []}""");

        await Assert.ThrowsAsync<LlmUnavailableException>(() => new DeepSeek(new HttpClient(wire), Key).CompleteAsync("s", "u"));
    }

    [Fact]
    public void The_key_is_read_from_LLM_API_then_LLM_UNDERSCORE_API_and_is_absent_when_neither_is_set()
    {
        Assert.Equal("a", DeepSeek.KeyFrom(name => name switch { "LLM-API" => "a", "LLM_API" => "b", _ => null }));
        Assert.Equal("b", DeepSeek.KeyFrom(name => name switch { "LLM-API" => " ", "LLM_API" => "b", _ => null }));
        Assert.Null(DeepSeek.KeyFrom(_ => null));
    }

    [Fact]
    public void A_blank_key_is_refused_before_anything_is_sent()
    {
        Assert.ThrowsAny<ArgumentException>(() => new DeepSeek(new HttpClient(), " "));
    }
}
