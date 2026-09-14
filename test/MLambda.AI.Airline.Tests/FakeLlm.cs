// FakeLlm.cs — a language model that says what the test tells it to, and remembers what it was asked.
//
// TWO SCRIPTED ANSWERS, ONE PER CALL. The desk makes at most two calls per reply — the draft and the
// phrasing — and each prompt says which it is in its first sentence, so the fake answers by that rather
// than by counting. A null answer is an outage.
namespace MLambda.AI.Airline.Tests;

internal sealed class FakeLlm(string? draft, string? format) : ILlm
{
    public List<Prompt> Asked { get; } = [];

    public Prompt DraftPrompt => Asked.Single(p => p.System.StartsWith("You draft replies", StringComparison.Ordinal));

    public Prompt? FormatPrompt => Asked.SingleOrDefault(p => p.System.StartsWith("You phrase decisions", StringComparison.Ordinal));

    public Task<string> CompleteAsync(string system, string user, CancellationToken cancellationToken = default)
    {
        Asked.Add(new Prompt(system, user));

        var answer = system.StartsWith("You draft replies", StringComparison.Ordinal) ? draft : format;

        return answer is null
            ? throw new LlmUnavailableException("the fake model is down")
            : Task.FromResult(answer);
    }

    /// <summary>A draft answer as the Draft.tav contract asks for it.</summary>
    public static string Drafted(string request, string bereavement, string promise, string text) =>
        $$"""{"request": "{{request}}", "bereavement": "{{bereavement}}", "promise": "{{promise}}", "draft": "{{text}}"}""";

    /// <summary>A phrasing answer as the Format.tav contract asks for it.</summary>
    public static string Phrased(string reply, string promise) =>
        $$"""{"reply": "{{reply}}", "promise": "{{promise}}"}""";
}

/// <summary>The policy book and bookings the program ships with.</summary>
internal static class Shipped
{
    public static PolicyWording Wording { get; } = PolicyWording.Load("data/policies.json");

    public static Bookings Bookings { get; } = Bookings.Load("data/bookings.json");

    public static Booking Booking(string reference) =>
        Bookings.FindIn(reference) ?? throw new KeyNotFoundException(reference);
}
