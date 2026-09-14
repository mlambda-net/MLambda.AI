// Bookings.cs — what happened to a booking, read from data/bookings.json.
//
// THE SYSTEM OF RECORD. Whether a trip was flown, what fare it was and whether the airline cancelled it reach
// Policy.hs only from here. The model may read a booking in a prompt, and may repeat it, but it never
// supplies one.
using System.Text.RegularExpressions;

namespace MLambda.AI.Airline.Records;

public sealed record Booking(string Reference, string Route, string Fare, bool Travelled, bool CancelledByAirline);

public sealed partial class Bookings(IReadOnlyList<Booking> bookings)
{
    public static Bookings Load(string path) => new(DataFile.Read<Shelf>(path).Bookings);

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
