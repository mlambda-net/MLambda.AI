// PolicyExpert.cs — the host's side of the policy expert: asks Policy.hs what each party ought to do, and
// whether a promise violates it.
//
// NO RULE LIVES HERE. Which policy applies, whether it obliges or forbids, and what the assistant may promise
// are all laws of Policy.hs. This class asserts one situation's facts — the request and the booking record —
// runs the engine the build generated from the theory, and turns its answers into `Norm`s with their wording.
//
// A FRESH ENGINE PER QUESTION. The generated engine has no retract, and a promise checked against one
// situation must not linger into the next, so each question asserts the situation's facts into a new
// engine. Those are a handful of facts and a dozen rules; building the engine costs less than reasoning
// about reuse.
using Rules = MLambda.AI.Airline.Policy;
using MLambda.AI.Airline.Records;

namespace MLambda.AI.Airline.Expert;

/// <summary>A norm Policy.hs derived: O(party) act or F(party) act, and the policy it comes from.</summary>
public sealed record Norm(char Modality, string Party, string Act, PolicyText Policy)
{
    public const char Obligatory = 'O';
    public const char Forbidden = 'F';

    /// <summary>`F(airline) bereavement_fare` — the norm as deontic logic writes it.</summary>
    public string Written => $"{Modality}({Party}) {Act}";
}

/// <summary>Everything the book concluded about what the airline owes for one request on one booking.</summary>
public sealed record Decision(
    string Situation,
    Booking Booking,
    string Request,
    bool Bereavement,
    IReadOnlyList<Norm> Obliged,
    IReadOnlyList<Norm> Forbidden)
{
    /// <summary>No norm spoke. The reply promises nothing and sends the request to a person.</summary>
    public bool Uncovered => Obliged.Count == 0 && Forbidden.Count == 0;
}

public sealed class PolicyExpert(PolicyWording wording)
{
    public const string Airline = "airline";
    public const string Assistant = "assistant";
    public const string Nothing = "nothing";

    /// <summary>Every remedy the book has a word for.</summary>
    public static readonly IReadOnlyList<string> Remedies = ["refund", "bereavement_fare", "travel_credit"];

    public PolicyWording Wording => wording;

    public async Task<Decision> DecideAsync(
        string situation, Booking booking, string request, bool bereavement, CancellationToken cancellationToken = default)
    {
        var engine = Facts(situation, booking, request, bereavement);

        var obliged = new List<Norm>();

        await foreach (var row in engine.Obligations(situation, Airline, cancellationToken))
        {
            obliged.Add(new Norm(Norm.Obligatory, Airline, row.What, wording[row.Source]));
        }

        var forbidden = new List<Norm>();

        await foreach (var row in engine.Prohibitions(situation, Airline, cancellationToken))
        {
            forbidden.Add(new Norm(Norm.Forbidden, Airline, row.What, wording[row.Source]));
        }

        return new Decision(situation, booking, request, bereavement, Ordered(obliged), Ordered(forbidden));
    }

    /// <summary>The CHAT-1 norm the promise breaks, or null when the assistant was permitted to make it.</summary>
    ///
    /// <remarks>"nothing" PROMISES NOTHING and breaks nothing. Any other word — including one the model
    /// invented — is asserted as an act the assistant did, and the expert says whether that act was
    /// forbidden: it is, unless the airline is obliged to give exactly that remedy.</remarks>
    public async Task<Norm?> ViolatedByAsync(Decision decision, string promise, CancellationToken cancellationToken = default)
    {
        if (promise == Nothing)
        {
            return null;
        }

        var engine = Facts(decision.Situation, decision.Booking, decision.Request, decision.Bereavement);
        engine.Assert(new Rules.ActFact(promise));
        engine.Assert(new Rules.DidFact(decision.Situation, Assistant, promise));

        await foreach (var row in engine.Violations(decision.Situation, cancellationToken))
        {
            if (row.Who == Assistant && row.What == promise)
            {
                await foreach (var norm in engine.Prohibitions(decision.Situation, Assistant, cancellationToken))
                {
                    if (norm.What == promise)
                    {
                        return new Norm(Norm.Forbidden, Assistant, promise, wording[norm.Source]);
                    }
                }
            }
        }

        return null;
    }

    /// <summary>What <paramref name="party"/> is permitted to do in the decision's situation: everything not forbidden.</summary>
    public async Task<IReadOnlyList<string>> PermittedAsync(Decision decision, string party, CancellationToken cancellationToken = default)
    {
        var permitted = new List<string>();

        await foreach (var act in Facts(decision.Situation, decision.Booking, decision.Request, decision.Bereavement)
                           .Permissions(decision.Situation, party, cancellationToken))
        {
            permitted.Add(act);
        }

        permitted.Sort(StringComparer.Ordinal);

        return permitted;
    }

    /// <summary>What holds at the situation where <paramref name="party"/> does all it ought — O(party), read at an ideal.</summary>
    public async Task<IReadOnlyList<string>> IdeallyAsync(Decision decision, string party, CancellationToken cancellationToken = default)
    {
        var ideal = $"{decision.Situation}/ideal-for-{party}";
        var engine = Facts(decision.Situation, decision.Booking, decision.Request, decision.Bereavement);
        engine.Assert(new Rules.IdealSeedFact(party, decision.Situation, ideal));

        var holds = new List<string>();

        await foreach (var act in engine.Ideally(ideal, cancellationToken))
        {
            holds.Add(act);
        }

        holds.Sort(StringComparer.Ordinal);

        return holds;
    }

    /// <summary>Acts the book both obliges and forbids a party in this situation — a defect in the book.</summary>
    public async Task<IReadOnlyList<string>> ConflictsAsync(
        string situation, Booking booking, string request, bool bereavement, CancellationToken cancellationToken = default)
    {
        var conflicts = new List<string>();

        await foreach (var row in Facts(situation, booking, request, bereavement).Conflicts(cancellationToken))
        {
            conflicts.Add($"{row.Who} {row.What}");
        }

        return conflicts;
    }

    /// <summary>"a refund", "a bereavement fare", "a travel credit" — a remedy as a sentence says it.</summary>
    public static string Say(string remedy) => remedy switch
    {
        "refund" => "a refund",
        "bereavement_fare" => "a bereavement fare",
        "travel_credit" => "a travel credit",
        Nothing => "nothing",
        _ => $"\"{remedy}\"",
    };

    private static Rules.IPolicyEngine Facts(string situation, Booking booking, string request, bool bereavement)
    {
        var engine = Rules.PolicyEngineFactory.Create();
        engine.Assert(new Rules.SituationFact(situation));
        engine.Assert(new Rules.PartyFact(Airline));
        engine.Assert(new Rules.PartyFact(Assistant));

        foreach (var remedy in Remedies)
        {
            engine.Assert(new Rules.ActFact(remedy));
        }

        engine.Assert(new Rules.RequestFact(situation, request));
        engine.Assert(new Rules.FareFact(situation, booking.Fare));

        if (bereavement)
        {
            engine.Assert(new Rules.BereavementFact(situation));
        }

        if (booking.Travelled)
        {
            engine.Assert(new Rules.TravelledFact(situation));
        }

        if (booking.CancelledByAirline)
        {
            engine.Assert(new Rules.CancelledByAirlineFact(situation));
        }

        return engine;
    }

    private static IReadOnlyList<Norm> Ordered(List<Norm> norms) =>
        [.. norms.DistinctBy(n => (n.Act, n.Policy.Id)).OrderBy(n => n.Policy.Id, StringComparer.Ordinal)];
}
