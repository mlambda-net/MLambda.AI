// AssistantTests.cs — the kind of reply Assistant.ha licenses, before any words are written.
namespace MLambda.AI.Airline.Tests;

using MLambda.AI.Airline.Assistant;

public class AssistantTests
{
    private static IAssistantEngine Believing(string topic, string booking)
    {
        var engine = AssistantEngineFactory.Create();
        engine.AssertAll([
            new DesireFact("a", "Answered"),
            new AttentionFact("a", "Listening"),
            new TopicFact("a", topic),
            new BookingFact("a", booking),
        ]);

        return engine;
    }

    private static async Task<List<string>> Rows(IAsyncEnumerable<string> source)
    {
        var all = new List<string>();

        await foreach (var row in source)
        {
            all.Add(row);
        }

        return all;
    }

    private static async Task<List<string>> Plans(IAssistantEngine engine)
    {
        var plans = new List<string>();

        await foreach (var row in engine.Intentions("a"))
        {
            plans.Add(row.Plan);
        }

        return plans;
    }

    [Fact]
    public async Task A_covered_question_with_a_booking_is_answered()
    {
        var engine = Believing("in", "yes");

        Assert.Equal(["Answer"], await Plans(engine));
        Assert.Empty(await Rows(engine.Abandoned("a")));
    }

    [Fact]
    public async Task A_covered_question_without_a_booking_asks_for_one()
    {
        var engine = Believing("in", "no");

        Assert.Equal(["Clarify"], await Plans(engine));
        Assert.Empty(await Rows(engine.Abandoned("a")));
    }

    [Fact]
    public async Task A_question_no_policy_covers_is_abandoned_and_says_why()
    {
        var engine = Believing("out", "yes");

        Assert.Empty(await Plans(engine));
        Assert.Equal(["Answered"], await Rows(engine.Abandoned("a")));

        await foreach (var reason in engine.Reasons("a"))
        {
            Assert.Contains("no written policy here covers it", reason.Reason);
        }
    }

    [Fact]
    public async Task Once_replied_the_desire_is_met()
    {
        var engine = Believing("in", "yes");
        engine.Assert(new RepliedFact("a", 1));

        Assert.Equal(["Answered"], await Rows(engine.Met("a")));
    }
}
