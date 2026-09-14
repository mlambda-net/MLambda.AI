// Records.cs — the policy book's wording and the booking records, read from data/.
//
// TWO KINDS OF TRUTH THE MODEL NEVER SUPPLIES. What a policy says is policies.json; what happened to a
// booking is bookings.json. The model may read both in a prompt, and may repeat them, but a fact about a
// booking reaches Policy.hs only from here.
using System.Text.Json;
using System.Text.RegularExpressions;

namespace MLambda.AI.Airline;

public sealed record PolicyText(string Id, string Title, string Text);

public sealed record Booking(string Reference, string Route, string Fare, bool Travelled, bool CancelledByAirline);

public sealed class PolicyBook(string source, IReadOnlyList<PolicyText> policies)
{
    /// <summary>Where the wording comes from, and that it is modelled rather than quoted.</summary>
    public string Source => source;

    public IReadOnlyList<PolicyText> All => policies;

    public PolicyText this[string id] =>
        policies.FirstOrDefault(p => p.Id == id) ?? throw new KeyNotFoundException($"No policy '{id}' in the book.");

    public static PolicyBook Load(string path)
    {
        var shelf = Data.Read<Shelf>(path);

        return new PolicyBook(shelf.Source, shelf.Policies);
    }

    private sealed record Shelf(string Source, IReadOnlyList<PolicyText> Policies);
}

public sealed partial class Bookings(IReadOnlyList<Booking> bookings)
{
    public static Bookings Load(string path) => new(Data.Read<Shelf>(path).Bookings);

    /// <summary>The first word in the message that is a reference this airline issued.</summary>
    ///
    /// <remarks>A LOOKUP, NOT A READING. Six letters or digits that match a record, found by the host.
    /// Asking the model for the reference would let a message talk its way into somebody else's
    /// booking; a regular expression cannot be talked to.</remarks>
    public Booking? FindIn(string message) =>
        Reference().Matches(message)
            .Select(m => bookings.FirstOrDefault(b => string.Equals(b.Reference, m.Value, StringComparison.OrdinalIgnoreCase)))
            .FirstOrDefault(b => b is not null);

    [GeneratedRegex(@"\b[A-Za-z0-9]{6}\b")]
    private static partial Regex Reference();

    private sealed record Shelf(string Source, IReadOnlyList<Booking> Bookings);
}

internal static class Data
{
    private static readonly JsonSerializerOptions Json = new() { PropertyNameCaseInsensitive = true };

    /// <summary>A data file, from the working directory or, failing that, beside the executable.</summary>
    ///
    /// <remarks>BOTH, BECAUSE `dotnet run` AND AN XUNIT HOST DISAGREE about where they run from; the
    /// csproj copies data/ to the output so the second always succeeds.</remarks>
    public static T Read<T>(string path)
    {
        var resolved = File.Exists(path) ? path : Path.Combine(AppContext.BaseDirectory, path);

        return JsonSerializer.Deserialize<T>(File.ReadAllText(resolved), Json)
            ?? throw new InvalidDataException($"{path} is empty.");
    }
}
