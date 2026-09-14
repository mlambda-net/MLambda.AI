// CleanerTests.cs — a plan library, shifting attention, and an honest failure.
namespace MLambda.AI.Agent.Tests;

using MLambda.AI.Agent.Cleaner;

public class CleanerTests
{
    /// <summary>A cleaner that wants a spotless house, with dirt it can reach.</summary>
    private static ICleanerEngine WithReachableDirt(string who = "c1")
    {
        var engine = CleanerEngineFactory.Create();
        engine.AssertAll([
            new DesireFact(who, "Spotless"),
            new AttentionFact(who, "Cleaning"),
            new DirtFact(who, "kitchen"),
            new ReachableFact(who, "kitchen"),
        ]);

        return engine;
    }

    /// <summary>The same cleaner, with dirt behind a locked door.</summary>
    private static ICleanerEngine WithBlockedDirt(string who = "c2")
    {
        var engine = CleanerEngineFactory.Create();
        engine.AssertAll([
            new DesireFact(who, "Spotless"),
            new AttentionFact(who, "Cleaning"),
            new DirtFact(who, "cellar"),
            new BlockedFact(who, "cellar"),
        ]);

        return engine;
    }

    private static async Task<List<string>> Plans(ICleanerEngine engine, string who)
    {
        var plans = new List<string>();

        await foreach (var row in engine.Intentions(who))
        {
            plans.Add(row.Plan);
        }

        plans.Sort(StringComparer.Ordinal);

        return plans;
    }

    private static async Task<List<string>> Rows(IAsyncEnumerable<string> source)
    {
        var all = new List<string>();

        await foreach (var row in source)
        {
            all.Add(row);
        }

        all.Sort(StringComparer.Ordinal);

        return all;
    }

    [Fact]
    public async Task A_plan_library_offers_every_route_and_chooses_none()
    {
        // BOTH PLANS REACH THE SAME GOAL, so both are licensed. The agent proposes; it does not
        // choose. Choosing is the host's job, and an engine that chose would be hiding the
        // decision somewhere nobody can read it.
        Assert.Equal(["Mop", "Sweep"], await Plans(WithReachableDirt(), "c1"));
    }

    [Fact]
    public async Task Dirt_it_cannot_reach_licenses_nothing()
    {
        // NON-VACUOUS: the test above shows this engine offers plans when it can act.
        Assert.Empty(await Plans(WithBlockedDirt(), "c2"));
    }

    [Fact]
    public async Task And_it_says_the_goal_is_out_of_reach_rather_than_trying_forever()
    {
        // `impossible Spotless when …`. THE ALTERNATIVE IS AN AGENT THAT NEVER STOPS, and every
        // other BDI framework makes this a log line somebody remembered to write.
        Assert.Equal(["Spotless"], await Rows(WithBlockedDirt().Abandoned("c2")));
    }

    [Fact]
    public async Task A_cleaner_that_can_still_act_abandons_nothing()
    {
        // NON-VACUOUS: the test above shows abandonment is reported when it is due.
        Assert.Empty(await Rows(WithReachableDirt().Abandoned("c1")));
    }

    [Fact]
    public async Task And_it_says_why_in_the_words_the_agent_was_written_with()
    {
        // `because "…"` IS NOT A COMMENT. The reason the parser insists on reaches the engine as a fact
        // beside the abandoned goal, so a host can show a person WHY, not only THAT.
        var reasons = new List<ReasonsRow>();
        await foreach (var row in WithBlockedDirt().Reasons("c2"))
        {
            reasons.Add(row);
        }

        Assert.Equal([new ReasonsRow("Spotless", "there is dirt in a room I cannot get to")], reasons);
    }

    [Fact]
    public async Task And_gives_no_reason_when_it_has_given_nothing_up()
    {
        var reasons = new List<ReasonsRow>();
        await foreach (var row in WithReachableDirt().Reasons("c1"))
        {
            reasons.Add(row);
        }

        // NON-VACUOUS: the test above shows the reason arrives when a goal is abandoned.
        Assert.Empty(reasons);
    }

    [Fact]
    public async Task Attention_moves_when_the_way_is_shut()
    {
        // `when B(self) blocked(r) attend Blocked`. THE GOAL HAS NOT CHANGED -- it still wants a
        // spotless house. What changed is what it is ABOUT, which is a different thing and the
        // reason attention is a keyword rather than a belief.
        Assert.Contains("Blocked", await Rows(WithBlockedDirt().Looking("c2")));
    }

    [Fact]
    public async Task And_stays_put_when_it_is_not()
    {
        var looking = await Rows(WithReachableDirt().Looking("c1"));

        Assert.Equal(["Cleaning"], looking);
        Assert.DoesNotContain("Blocked", looking);
    }

    [Fact]
    public async Task A_cleaned_room_meets_the_goal()
    {
        var engine = CleanerEngineFactory.Create();
        engine.AssertAll([
            new DesireFact("c3", "Spotless"),
            new CleanedFact("c3", "kitchen"),
        ]);

        Assert.Equal(["Spotless"], await Rows(engine.Met("c3")));
    }

    [Fact]
    public async Task One_cleaners_locked_door_is_not_anothers()
    {
        var engine = CleanerEngineFactory.Create();
        engine.AssertAll([
            new DesireFact("x", "Spotless"), new DirtFact("x", "hall"), new ReachableFact("x", "hall"),
            new DesireFact("y", "Spotless"), new DirtFact("y", "hall"), new BlockedFact("y", "hall"),
        ]);

        Assert.Equal(["Mop", "Sweep"], await Plans(engine, "x"));
        Assert.Empty(await Rows(engine.Abandoned("x")));

        Assert.Empty(await Plans(engine, "y"));
        Assert.Equal(["Spotless"], await Rows(engine.Abandoned("y")));
    }
}
