// PlanTests.cs — Reply.hk alone: the order of its steps, whatever the host answers.
namespace MLambda.AI.Airline.Tests;

public class PlanTests
{
    private static readonly TimeSpan Patience = TimeSpan.FromSeconds(10);

    private static async Task<(IReadOnlyList<string> Steps, Journal Beliefs)> Run(string plan, IReadOnlyDictionary<string, string> answers)
    {
        var journal = new Journal();
        var steps = await Steps.RunAsync(
            plan,
            journal,
            (command, _) => Task.FromResult(answers.GetValueOrDefault(command, "done"))).WaitAsync(Patience);

        return (steps, journal);
    }

    [Fact]
    public async Task An_upheld_draft_well_phrased_goes_review_format_print()
    {
        var (steps, journal) = await Run("AnswerSteps", new Dictionary<string, string> { ["review"] = "upheld", ["format"] = "formatted" });

        Assert.Equal(["review", "format", "print"], steps);
        Assert.Equal(["reviewed(upheld)", "replied(1)"], journal.Beliefs);
    }

    [Fact]
    public async Task An_overruled_draft_is_still_formatted_and_a_rejected_phrasing_falls_back()
    {
        var (steps, journal) = await Run("AnswerSteps", new Dictionary<string, string> { ["review"] = "overruled", ["format"] = "overruled" });

        Assert.Equal(["review", "format", "fallback", "print"], steps);
        Assert.Equal(["reviewed(overruled)", "replied(1)"], journal.Beliefs);
    }

    [Fact]
    public async Task Any_answer_but_formatted_falls_back_including_one_the_plan_has_never_heard()
    {
        var (steps, _) = await Run("AnswerSteps", new Dictionary<string, string> { ["review"] = "upheld", ["format"] = "banana" });

        Assert.Equal(["review", "format", "fallback", "print"], steps);
    }

    [Fact]
    public async Task Clarifying_asks_then_prints_and_reviews_nothing()
    {
        var (steps, journal) = await Run("ClarifySteps", new Dictionary<string, string>());

        Assert.Equal(["ask", "print"], steps);
        Assert.Equal(["replied(1)"], journal.Beliefs);
    }

    [Fact]
    public async Task A_host_that_fails_aborts_the_plan_and_the_failure_surfaces()
    {
        var failure = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            Steps.RunAsync("AnswerSteps", new Journal(), (_, _) => throw new InvalidOperationException("host down")).WaitAsync(Patience));

        Assert.Equal("host down", failure.Message);
    }
}
