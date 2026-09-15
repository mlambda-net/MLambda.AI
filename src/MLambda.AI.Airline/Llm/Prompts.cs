// Prompts.cs — fills the TAV templates. What each prompt says is in Templates/; this file only supplies values.
//
// EVERY KEY, EVERY TIME. A template that reads a key the map does not carry throws TAV0040 while rendering
// — even inside an `if` — so absent values are passed as empty strings, and flags as "yes" or "".
//
// THE FENCE IS KEPT HERE. The customer's message is the only untrusted text in any prompt, and it goes into
// exactly one of them, Draft. Before it does, every run of three or more angle brackets is removed, so the
// message cannot close `<<<CUSTOMER … CUSTOMER>>>` early and write something that reads like the system's.
using System.Text.RegularExpressions;
using MLambda.AI.Airline.Templates;
using MLambda.Grammar.Tav.Render;
using MLambda.AI.Airline.Expert;
using MLambda.AI.Airline.Records;

namespace MLambda.AI.Airline.Llm;

public sealed record Prompt(string System, string User);

public static partial class Prompts
{
    /// <summary>Longer than any honest question about a fare; a message past it is cut, not refused.</summary>
    public const int Longest = 2000;

    public static Prompt Draft(PolicyWording wording, Booking? booking, string message)
    {
        var policies = wording.All
            .Select(TavValue (p) => Map(("id", p.Id), ("title", p.Title), ("text", p.Text)))
            .ToList();

        var system = DraftTemplate.Render(new TavMap(new Dictionary<string, TavValue>
        {
            ["part"] = new TavString("system"),
            ["policies"] = new TavList(policies),
        }));

        var user = DraftTemplate.Render(new TavMap(new Dictionary<string, TavValue>
        {
            ["part"] = new TavString("user"),
            ["found"] = Flag(booking is not null),
            ["reference"] = new TavString(booking?.Reference ?? string.Empty),
            ["route"] = new TavString(booking?.Route ?? string.Empty),
            ["fare"] = new TavString(booking?.Fare ?? string.Empty),
            ["travelled"] = new TavString(booking is null ? string.Empty : YesNo(booking.Travelled)),
            ["cancelled"] = new TavString(booking is null ? string.Empty : YesNo(booking.CancelledByAirline)),
            ["message"] = new TavString(Fence(message)),
        }));

        return new Prompt(system.Trim(), user.Trim());
    }

    /// <summary>The decision to phrase. No customer text and no draft: see Format.tav.</summary>
    public static Prompt Format(Decision decision)
    {
        var system = FormatTemplate.Render(Decided(decision, "system"));
        var user = FormatTemplate.Render(Decided(decision, "user"));

        return new Prompt(system.Trim(), user.Trim());
    }

    /// <summary>The reply without a model, from the decision and the policies' own words.</summary>
    public static string Plain(Decision decision) =>
        PlainTemplate.Render(Decided(decision, "plain")).Trim();

    public static string Clarify(string request) =>
        ClarifyTemplate.Render(new TavMap(new Dictionary<string, TavValue>
        {
            ["asked"] = new TavString(PolicyExpert.Say(request)),
        })).Trim();

    /// <summary>The message as it may appear inside the fence: no marker-like brackets, no control characters, bounded.</summary>
    public static string Fence(string message)
    {
        var bounded = message.Length > Longest ? message[..Longest] : message;
        var printable = new string([.. bounded.Where(c => !char.IsControl(c) || c == '\n')]);

        return Brackets().Replace(printable, string.Empty).Trim();
    }

    private static TavMap Decided(Decision decision, string part) =>
        new(new Dictionary<string, TavValue>
        {
            ["part"] = new TavString(part),
            ["reference"] = new TavString(decision.Booking.Reference),
            ["route"] = new TavString(decision.Booking.Route),
            ["asked"] = new TavString(PolicyExpert.Say(decision.Request)),
            ["obliged"] = Norms(decision.Obliged),
            ["forbidden"] = Norms(decision.Forbidden),
            ["uncovered"] = Flag(decision.Uncovered),
            ["planned"] = Flag(decision.Planned),
            ["plan"] = new TavString(string.Join(", then ", decision.Before)),
            ["after"] = new TavString(decision.Planned ? $", once {PolicyExpert.Said(decision.Before)}," : string.Empty),
            ["doing"] = new TavString(string.Join(" and ", decision.Before.Select(PolicyExpert.Doing))),
            ["lapses"] = new TavList([.. decision.Lapses.Select(TavValue (l) => Map(
                ("what", PolicyExpert.Say(l.Owed.Act)), ("id", l.Owed.Policy.Id), ("text", l.Owed.Policy.Text),
                ("event", PolicyExpert.Say(l.Event)), ("step", l.Event)))]),
            ["never"] = Flag(decision.Planned && decision.NeverAgain),
        });

    private static TavList Norms(IReadOnlyList<Norm> norms) =>
        new([.. norms.Select(TavValue (n) => Map(("what", PolicyExpert.Say(n.Act)), ("id", n.Policy.Id), ("text", n.Policy.Text)))]);

    private static TavMap Map(params (string Key, string Value)[] entries) =>
        new(entries.ToDictionary(e => e.Key, TavValue (e) => new TavString(e.Value)));

    private static TavString Flag(bool on) => new(on ? "yes" : string.Empty);

    private static string YesNo(bool value) => value ? "yes" : "no";

    [GeneratedRegex("<{3,}|>{3,}")]
    private static partial Regex Brackets();
}
