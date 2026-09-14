// Desk.cs — one customer message in, one reply out, and every step of it answerable to the policy book.
//
// THE ORDER OF A REPLY.
//   1. The model reads the message and drafts a reply (Draft.tav). Nobody sees the draft.
//   2. The booking is looked up by reference, from the records — not from the model.
//   3. The agent (Assistant.ha) decides the kind of reply: answer, ask for a booking, or decline.
//   4. To answer, the policy expert (Policy.hs) derives the norms — what the airline is obliged and
//      forbidden to do for this booking — and the plan (Reply.hk) runs: review the draft's promise under
//      CHAT-1, ask the model to phrase the norms (Format.tav), judge that phrasing's promise too, fall back to
//      the plain wording (Plain.tav) if it fails, and print — with every norm the reply rests on.
//
// WHAT THE MODEL CAN GET WRONG, AND WHAT THAT COSTS. It can misread the request: the customer gets a reply
// about the wrong remedy, still correct for their booking. It can draft or phrase a promise the book does
// does not oblige the airline to give: making it is a violation of CHAT-1, so it is withheld and the plain
// wording goes out. It can be unavailable: the plain
// wording goes out. What it cannot do is commit the airline to anything, which is what went wrong in
// Moffatt v. Air Canada.
//
// AND WHAT IS STILL TRUSTED. `promise` is the model's own account of what its text commits to. A model that
// wrote "your refund is approved" and declared `"promise": "nothing"` would pass the check. That is why the
// phrasing call never sees the customer's words: the only text that could talk the model into lying is
// kept out of the call whose words are shown.
using Mind = MLambda.AI.Airline.Assistant;

namespace MLambda.AI.Airline;

public enum Outcome
{
    Answered,
    Clarified,
    Declined,
}

/// <summary>What the customer was told, on what grounds, and how the reply was made.</summary>
public sealed record Reply(
    Outcome Outcome,
    string Text,
    IReadOnlyList<Norm> Norms,
    IReadOnlyList<string> Review,
    IReadOnlyList<string> Steps,
    IReadOnlyList<string> Beliefs);

public sealed class Desk(ILlm llm, PolicyBook book, Bookings bookings, TextWriter output)
{
    private const string Self = "desk";

    private readonly Rulebook rules = new(book);

    /// <summary>A question still waiting for its booking reference.</summary>
    private string? pending;
    private int turn;

    /// <summary>Why the model's last answer could not be used, said in the review so an outage is visible.</summary>
    private string why = string.Empty;

    public async Task<Reply> HandleAsync(string message, CancellationToken cancellationToken = default)
    {
        var said = pending is null ? message : $"{pending}\n{message}";
        var booking = bookings.FindIn(said);
        var draft = await DraftAsync(said, booking, cancellationToken);

        var engine = Mind.AssistantEngineFactory.Create();
        engine.AssertAll([
            new Mind.DesireFact(Self, "Answered"),
            new Mind.AttentionFact(Self, "Listening"),
            new Mind.TopicFact(Self, draft.InScope ? "in" : "out"),
            new Mind.BookingFact(Self, booking is null ? "no" : "yes"),
        ]);

        await foreach (var abandoned in engine.Reasons(Self, cancellationToken))
        {
            pending = null;
            Print(abandoned.Reason, [], []);

            return new Reply(Outcome.Declined, abandoned.Reason, [], [], [], []);
        }

        await foreach (var intention in engine.Intentions(Self, cancellationToken))
        {
            switch (intention.Plan)
            {
                case "Clarify":
                    pending = said;
                    return await ClarifyAsync(draft, cancellationToken);
                case "Answer":
                    pending = null;
                    return await AnswerAsync($"turn-{++turn}", booking!, draft, cancellationToken);
            }
        }

        throw new InvalidOperationException("Assistant.ha licensed no reply and gave no reason; the agent is incomplete.");
    }

    private async Task<Draft> DraftAsync(string said, Booking? booking, CancellationToken cancellationToken)
    {
        var prompt = Prompts.Draft(book, booking, said);

        try
        {
            var answer = await llm.CompleteAsync(prompt.System, prompt.User, cancellationToken);

            why = "its answer held no JSON object";

            return Draft.Parse(answer) ?? Draft.Guess(said);
        }
        catch (LlmUnavailableException outage)
        {
            why = outage.Message;

            return Draft.Guess(said);
        }
    }

    private async Task<Reply> ClarifyAsync(Draft draft, CancellationToken cancellationToken)
    {
        var journal = new Journal();
        var text = string.Empty;

        var steps = await Steps.RunAsync("ClarifySteps", journal, (command, _) =>
        {
            switch (command)
            {
                case "ask":
                    text = Prompts.Clarify(draft.Request);
                    break;
                case "print":
                    Print(text, [], []);
                    break;
                default:
                    throw Unknown(command);
            }

            return Task.FromResult("done");
        }, cancellationToken);

        return new Reply(Outcome.Clarified, text, [], [], steps, journal.Beliefs);
    }

    private async Task<Reply> AnswerAsync(string claim, Booking booking, Draft draft, CancellationToken cancellationToken)
    {
        var decision = await rules.DecideAsync(claim, booking, draft.Request, draft.Bereavement, cancellationToken);
        var cited = decision.Forbidden.Concat(decision.Obliged).ToList();
        var review = new List<string>();
        var journal = new Journal();
        var text = Prompts.Plain(decision);

        void Withheld(Norm broken, string reason)
        {
            review.Add(reason);

            if (!cited.Contains(broken))
            {
                cited.Add(broken);
            }
        }

        async Task<string> Serve(string command, CancellationToken token)
        {
            switch (command)
            {
                case "review":
                    if (!draft.FromModel)
                    {
                        review.Add($"The model gave no usable draft ({why.TrimEnd('.')}), so the request was read from its words and nothing was drafted.");
                        return "unavailable";
                    }

                    if (await rules.ViolatedByAsync(decision, draft.Promise, token) is { } broken)
                    {
                        Withheld(broken, $"The model's draft promised {Rulebook.Say(draft.Promise)}, which the airline is not obliged to give for booking {booking.Reference}; promising it violates {broken.Policy.Id}. The draft was withheld.");
                        return "overruled";
                    }

                    review.Add(draft.Promise == Rulebook.Nothing
                        ? "The model's draft promised nothing, so it violated no norm."
                        : $"The model's draft promised {Rulebook.Say(draft.Promise)}, which the airline is obliged to give.");
                    return "upheld";

                case "format":
                    if (await FormatAsync(decision, token) is not { } phrasing)
                    {
                        review.Add($"The model could not phrase the reply ({why.TrimEnd('.')}), so the plain wording was used.");
                        return "unavailable";
                    }

                    if (await rules.ViolatedByAsync(decision, phrasing.Promise, token) is { } breach)
                    {
                        Withheld(breach, $"The model's wording promised {Rulebook.Say(phrasing.Promise)}, which violates {breach.Policy.Id}, so the plain wording was used.");
                        return "overruled";
                    }

                    text = phrasing.Reply;
                    return "formatted";

                case "fallback":
                    text = Prompts.Plain(decision);
                    return "done";

                case "print":
                    Print(text, cited, review);
                    return "done";

                default:
                    throw Unknown(command);
            }
        }

        var steps = await Steps.RunAsync("AnswerSteps", journal, Serve, cancellationToken);

        return new Reply(Outcome.Answered, text, cited, review, steps, journal.Beliefs);
    }

    private async Task<Phrasing?> FormatAsync(Decision decision, CancellationToken cancellationToken)
    {
        var prompt = Prompts.Format(decision);

        try
        {
            why = "its answer held no reply";

            return Phrasing.Parse(await llm.CompleteAsync(prompt.System, prompt.User, cancellationToken));
        }
        catch (LlmUnavailableException outage)
        {
            why = outage.Message;

            return null;
        }
    }

    private void Print(string text, IReadOnlyList<Norm> norms, IReadOnlyList<string> review)
    {
        output.WriteLine($"Assistant: {text}");

        foreach (var norm in norms)
        {
            output.WriteLine($"  policy  {norm.Policy.Id,-6}  {norm.Written,-30}  {norm.Policy.Title}");
        }

        foreach (var line in review)
        {
            output.WriteLine($"  review  {line}");
        }

        output.WriteLine();
    }

    private static InvalidOperationException Unknown(string command) =>
        new($"Reply.hk sent '{command}', which this desk does not know how to do.");
}
