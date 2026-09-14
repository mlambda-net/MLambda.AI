// ModelAnswers.cs — reading what a model sent back, on the assumption that it may be anything.
//
// A CLOSED VOCABULARY, READ CONSERVATIVELY. A `request` the program does not know is "other", which the
// agent declines. A `promise` is kept exactly as written unless it is "nothing": an invented remedy such as
// "full_refund_and_voucher" is not tidied away into "nothing", it is checked like any other promise, and no
// policy grants it. Reading a model generously is how a program ends up promising what the model made up.
using System.Text.Json;
using MLambda.AI.Airline.Expert;

namespace MLambda.AI.Airline.Llm;

/// <summary>What the first call read from the message, and the draft it wrote.</summary>
public sealed record Draft(string Request, bool Bereavement, string Promise, string Text, bool FromModel)
{
    public static readonly IReadOnlySet<string> Requests = new HashSet<string>(StringComparer.Ordinal) { "refund", "bereavement_fare" };

    public const string Other = "other";

    public bool InScope => Requests.Contains(Request);

    /// <summary>The model's JSON, or null when there is no object in it.</summary>
    public static Draft? Parse(string answer)
    {
        if (Json.Object(answer) is not { } root)
        {
            return null;
        }

        var request = Json.Word(root, "request");

        return new Draft(
            Requests.Contains(request) ? request : Other,
            Json.Word(root, "bereavement") == "yes",
            Json.Promise(root),
            Json.Text(root, "draft"),
            FromModel: true);
    }

    /// <summary>A reading without a model, for when there is none: a few words, and no draft.</summary>
    ///
    /// <remarks>CRUDE ON PURPOSE. It exists so an outage degrades the assistant to a plainer one rather
    /// than to a silent one; every decision it leads to is still Policy.hs's.</remarks>
    public static Draft Guess(string message)
    {
        var said = message.ToLowerInvariant();
        var bereavement = new[] { "bereave", "passed away", "died", "death", "funeral" }.Any(said.Contains);
        var refund = new[] { "refund", "money back", "reimburse", "cancel" }.Any(said.Contains);
        var request = bereavement ? "bereavement_fare" : refund ? "refund" : Other;

        return new Draft(request, bereavement, PolicyExpert.Nothing, string.Empty, FromModel: false);
    }
}

/// <summary>What the second call made of the decision.</summary>
public sealed record Phrasing(string Reply, string Promise)
{
    /// <summary>The model's JSON, or null when it holds no reply.</summary>
    public static Phrasing? Parse(string answer) =>
        Json.Object(answer) is { } root && Json.Text(root, "reply") is { Length: > 0 } reply
            ? new Phrasing(reply, Json.Promise(root))
            : null;
}

internal static class Json
{
    /// <summary>The outermost object in the answer, tolerating a code fence or prose around it.</summary>
    public static JsonElement? Object(string answer)
    {
        var start = answer.IndexOf('{');
        var end = answer.LastIndexOf('}');

        if (start < 0 || end <= start)
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(answer[start..(end + 1)]);

            return document.RootElement.ValueKind == JsonValueKind.Object ? document.RootElement.Clone() : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public static string Text(JsonElement root, string name) =>
        root.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()!.Trim()
            : string.Empty;

    public static string Word(JsonElement root, string name) => Text(root, name).ToLowerInvariant();

    /// <summary>The promise as written; a missing one is "unstated", which no policy grants.</summary>
    public static string Promise(JsonElement root) => Word(root, "promise") is { Length: > 0 } word ? word : "unstated";
}
