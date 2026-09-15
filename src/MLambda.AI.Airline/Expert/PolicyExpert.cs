// PolicyExpert.cs — the host's side of the policy expert: asks Policy.hs what each party ought to do, when,
// and whether a promise violates it.
//
// NO RULE LIVES HERE. Which policy applies, whether it obliges or forbids, how flying changes that, and what
// the assistant may promise are all laws of Policy.hs. This class seeds one TRACE — `now`, then a moment after
// each step the customer says they will take before asking — asserts the booking record at its first moment
// and the request at every moment, runs the engine the build generated from the theory, and turns its
// answers into `Norm`s with their wording.
//
// A FRESH ENGINE PER QUESTION. The generated engine has no retract, and a promise checked against one trace
// must not linger into the next, so each question asserts its facts into a new engine. Those are a handful
// of facts and a few dozen rules; building the engine costs less than reasoning about reuse.
using Rules = MLambda.AI.Airline.Policy;
using MLambda.AI.Airline.Records;

namespace MLambda.AI.Airline.Expert;

/// <summary>A norm Policy.hs derived: O(party) act or F(party) act, the policy it comes from, and when it holds.</summary>
public sealed record Norm(char Modality, string Party, string Act, PolicyText Policy, string When)
{
    public const char Obligatory = 'O';
    public const char Forbidden = 'F';

    /// <summary>`F(airline) bereavement_fare` — the norm as deontic logic writes it.</summary>
    public string Written => $"{Modality}({Party}) {Act}";
}

/// <summary>An obligation that held before one of the customer's own steps, and that step ended it.</summary>
public sealed record Lapse(Norm Owed, string Event);

/// <summary>What the book concluded about one request, asked at the end of the customer's plan.</summary>
public sealed record Decision(
    string Situation,
    Booking Booking,
    string Request,
    bool Bereavement,
    IReadOnlyList<string> Before,
    IReadOnlyList<Norm> Obliged,
    IReadOnlyList<Norm> Forbidden,
    IReadOnlyList<Lapse> Lapses,
    bool NeverAgain)
{
    /// <summary>No norm spoke. The reply promises nothing and sends the request to a person.</summary>
    public bool Uncovered => Obliged.Count == 0 && Forbidden.Count == 0;

    /// <summary>The customer described steps they will take before asking.</summary>
    public bool Planned => Before.Count > 0;

    /// <summary>The moment the customer would ask: after every step of the plan.</summary>
    public string Asked => PolicyExpert.Moment(Situation, Before.Count);
}

public sealed class PolicyExpert(PolicyWording wording)
{
    public const string Airline = "airline";
    public const string Assistant = "assistant";
    public const string Nothing = "nothing";
    public const string Fly = "fly";

    /// <summary>Every remedy the book has a word for.</summary>
    public static readonly IReadOnlyList<string> Remedies = ["refund", "bereavement_fare", "travel_credit"];

    /// <summary>Every step a customer can take before asking that the book knows changes something.</summary>
    public static readonly IReadOnlyList<string> Events = [Fly];

    public PolicyWording Wording => wording;

    public static string Moment(string situation, int index) => $"{situation}@{index}";

    public async Task<Decision> DecideAsync(
        string situation,
        Booking booking,
        string request,
        bool bereavement,
        IReadOnlyList<string>? before = null,
        CancellationToken cancellationToken = default)
    {
        var steps = before ?? [];
        var engine = Trace(situation, booking, request, bereavement, steps);
        var asked = Moment(situation, steps.Count);
        var when = steps.Count == 0 ? "now" : $"after {Said(steps)}";

        var obliged = new List<Norm>();

        await foreach (var row in engine.Obligations(asked, Airline, cancellationToken))
        {
            obliged.Add(new Norm(Norm.Obligatory, Airline, row.What, wording[row.Source], when));
        }

        var forbidden = new List<Norm>();

        await foreach (var row in engine.Prohibitions(asked, Airline, cancellationToken))
        {
            forbidden.Add(new Norm(Norm.Forbidden, Airline, row.What, wording[row.Source], when));
        }

        var lapses = new List<Lapse>();

        await foreach (var row in engine.Lapses(cancellationToken))
        {
            await foreach (var owed in engine.Obligations(row.At, Airline, cancellationToken))
            {
                if (owed.What == row.What)
                {
                    var norm = new Norm(Norm.Obligatory, Airline, row.What, wording[owed.Source], $"until {Say(row.By)}");
                    lapses.Add(new Lapse(norm, row.By));
                }
            }
        }

        var neverAgain = false;

        await foreach (var act in engine.Never(asked, cancellationToken))
        {
            neverAgain |= act == request;
        }

        return new Decision(
            situation, booking, request, bereavement, steps,
            Ordered(obliged), Ordered(forbidden), [.. lapses.DistinctBy(l => (l.Owed.Act, l.Owed.Policy.Id, l.Event))], neverAgain);
    }

    /// <summary>The CHAT-1 norm the promise breaks at the moment the customer would ask, or null when it breaks none.</summary>
    ///
    /// <remarks>"nothing" PROMISES NOTHING and breaks nothing. Any other word — including one the model
    /// invented — is asserted as an act the assistant did AT THE ASKED MOMENT, and the expert says whether that
    /// act was forbidden then: it is, unless the airline is obliged to give exactly that remedy then. A refund
    /// owed today does not license "fly, and we will refund you".</remarks>
    public async Task<Norm?> ViolatedByAsync(Decision decision, string promise, CancellationToken cancellationToken = default)
    {
        if (promise == Nothing)
        {
            return null;
        }

        var engine = Trace(decision);
        engine.Assert(new Rules.ActFact(promise));
        engine.Assert(new Rules.DidFact(decision.Asked, Assistant, promise));

        await foreach (var row in engine.Violations(decision.Asked, cancellationToken))
        {
            if (row.Who != Assistant || row.What != promise)
            {
                continue;
            }

            await foreach (var norm in engine.Prohibitions(decision.Asked, Assistant, cancellationToken))
            {
                if (norm.What == promise)
                {
                    var when = decision.Planned ? $"after {Said(decision.Before)}" : "now";
                    return new Norm(Norm.Forbidden, Assistant, promise, wording[norm.Source], when);
                }
            }
        }

        return null;
    }

    /// <summary>What <paramref name="party"/> is permitted to do at the asked moment: everything not forbidden.</summary>
    public async Task<IReadOnlyList<string>> PermittedAsync(Decision decision, string party, CancellationToken cancellationToken = default)
    {
        var permitted = new List<string>();

        await foreach (var act in Trace(decision).Permissions(decision.Asked, party, cancellationToken))
        {
            permitted.Add(act);
        }

        permitted.Sort(StringComparer.Ordinal);

        return permitted;
    }

    /// <summary>What holds where <paramref name="party"/> does all it ought at the asked moment — O(party), read at an ideal.</summary>
    public async Task<IReadOnlyList<string>> IdeallyAsync(Decision decision, string party, CancellationToken cancellationToken = default)
    {
        var ideal = $"{decision.Asked}/ideal-for-{party}";
        var engine = Trace(decision);
        engine.Assert(new Rules.IdealSeedFact(party, decision.Asked, ideal));

        var holds = new List<string>();

        await foreach (var act in engine.Ideally(ideal, cancellationToken))
        {
            holds.Add(act);
        }

        holds.Sort(StringComparer.Ordinal);

        return holds;
    }

    /// <summary>Acts the book both obliges and forbids a party at some moment of the trace — a defect in the book.</summary>
    public async Task<IReadOnlyList<string>> ConflictsAsync(
        string situation,
        Booking booking,
        string request,
        bool bereavement,
        IReadOnlyList<string>? before = null,
        CancellationToken cancellationToken = default)
    {
        var conflicts = new List<string>();

        await foreach (var row in Trace(situation, booking, request, bereavement, before ?? []).Conflicts(cancellationToken))
        {
            conflicts.Add($"{row.At} {row.Who} {row.What}");
        }

        return conflicts;
    }

    /// <summary>"a refund", "a bereavement fare", "a travel credit" — a remedy as a sentence says it.</summary>
    public static string Say(string word) => word switch
    {
        "refund" => "a refund",
        "bereavement_fare" => "a bereavement fare",
        "travel_credit" => "a travel credit",
        Fly => "you fly",
        Nothing => "nothing",
        _ => $"\"{word}\"",
    };

    /// <summary>"flying" — a step as the subject of a sentence.</summary>
    public static string Doing(string step) => step switch
    {
        Fly => "flying",
        _ => $"\"{step}\"",
    };

    /// <summary>"you fly", or "you fly, then …" — the customer's steps as a sentence says them.</summary>
    public static string Said(IReadOnlyList<string> steps) => string.Join(", then ", steps.Select(Say));

    private static Rules.IPolicyEngine Trace(Decision decision) =>
        Trace(decision.Situation, decision.Booking, decision.Request, decision.Bereavement, decision.Before);

    private static Rules.IPolicyEngine Trace(string situation, Booking booking, string request, bool bereavement, IReadOnlyList<string> before)
    {
        var engine = Rules.PolicyEngineFactory.Create();
        engine.Assert(new Rules.PartyFact(Airline));
        engine.Assert(new Rules.PartyFact(Assistant));

        foreach (var remedy in Remedies)
        {
            engine.Assert(new Rules.ActFact(remedy));
        }

        // THE TRACE: one moment now, one after each step. The request is asked at every moment, so the book
        // says what the answer would be at each; the booking record is what is so at the first.
        for (var i = 0; i <= before.Count; i++)
        {
            var moment = Moment(situation, i);
            engine.Assert(new Rules.MomentFact(moment));
            engine.Assert(new Rules.RequestFact(moment, request));

            if (i < before.Count)
            {
                engine.Assert(new Rules.HappensFact(moment, before[i]));
                engine.Assert(new Rules.NextFact(moment, Moment(situation, i + 1)));
            }
        }

        var now = Moment(situation, 0);
        engine.Assert(new Rules.FareFact(now, booking.Fare));

        if (bereavement)
        {
            engine.Assert(new Rules.BereavementFact(now));
        }

        if (booking.Travelled)
        {
            engine.Assert(new Rules.TravelledFact(now));
        }

        if (booking.CancelledByAirline)
        {
            engine.Assert(new Rules.CancelledByAirlineFact(now));
        }

        return engine;
    }

    private static IReadOnlyList<Norm> Ordered(List<Norm> norms) =>
        [.. norms.DistinctBy(n => (n.Act, n.Policy.Id)).OrderBy(n => n.Policy.Id, StringComparer.Ordinal)];
}
