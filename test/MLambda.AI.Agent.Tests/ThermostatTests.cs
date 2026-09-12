// ThermostatTests.cs — the smallest agent there is, deliberating.
namespace MLambda.AI.Agent.Tests;

using MLambda.AI.Agent.Thermostat;

public class ThermostatTests
{
    // THE DESIRE AND THE FOCUS ARE THE HOST'S TO ASSERT, not the theory's to derive. An agent wants
    // a goal because it says so, and which subject wants it is known only once an instance exists.
    private static IThermostatEngine At(int degrees, string who = "t1")
    {
        var engine = ThermostatEngineFactory.Create();
        engine.AssertAll([
            new DesireFact(who, "Comfortable"),
            new AttentionFact(who, "Watching"),
            new TemperatureFact(who, degrees),
        ]);

        return engine;
    }

    private static async Task<List<string>> Plans(IThermostatEngine engine, string who = "t1")
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
    public async Task Cold_means_it_intends_to_warm()
    {
        Assert.Equal(["Warm"], await Plans(At(10)));
    }

    [Fact]
    public async Task Hot_means_it_intends_to_cool()
    {
        Assert.Equal(["Cool"], await Plans(At(30)));
    }

    [Fact]
    public async Task And_the_intention_names_the_goal_it_serves()
    {
        // AN INTENTION IS NOT AN ACTION. It is a plan adopted TOWARD a goal, and the pair is what
        // the engine reports -- which is what lets a host ask why, not just what.
        var rows = new List<IntentionsRow>();

        await foreach (var row in At(10).Intentions("t1"))
        {
            rows.Add(row);
        }

        var only = Assert.Single(rows);

        Assert.Equal("Warm", only.Plan);
        Assert.Equal("Comfortable", only.Goal);
    }

    [Fact]
    public async Task Comfortable_means_it_intends_nothing()
    {
        // THE GUARD IS REAL ARITHMETIC, not a record match. At 20 neither condition holds, so no
        // intention is licensed and the agent stops proposing.
        //
        // NON-VACUOUS: the two tests above show this engine does produce intentions.
        Assert.Empty(await Plans(At(20)));
    }

    [Fact]
    public async Task And_at_twenty_the_goal_is_met()
    {
        // ACHIEVEMENT IS A DEFINITION. Nothing performed it; the agent states what being achieved
        // IS, and the engine concludes it.
        Assert.Equal(["Comfortable"], await Rows(At(20).Met("t1")));
        Assert.Empty(await Rows(At(10).Met("t1")));
    }

    [Fact]
    public async Task The_band_is_closed_at_both_ends()
    {
        // `t ≥ 18 ∧ t ≤ 24`. Eighteen is comfortable and seventeen is not, and an off-by-one here
        // would be invisible in every other test in this file.
        Assert.Equal(["Comfortable"], await Rows(At(18).Met("t1")));
        Assert.Equal(["Comfortable"], await Rows(At(24).Met("t1")));
        Assert.Empty(await Rows(At(17).Met("t1")));
        Assert.Empty(await Rows(At(25).Met("t1")));
    }

    [Fact]
    public async Task It_is_minding_what_the_host_said_it_was_minding()
    {
        Assert.Equal(["Watching"], await Rows(At(10).Looking("t1")));
    }

    [Fact]
    public async Task One_thermostats_beliefs_never_become_anothers()
    {
        // THE SUBJECT IS THREADED THROUGH EVERY PREDICATE for exactly this reason: one engine holds
        // every subject's facts at once, and a rule about t1 must not fire on t2's beliefs.
        var engine = ThermostatEngineFactory.Create();
        engine.AssertAll([
            new DesireFact("t1", "Comfortable"), new TemperatureFact("t1", 10),
            new DesireFact("t2", "Comfortable"), new TemperatureFact("t2", 20),
        ]);

        Assert.Equal(["Warm"], await Plans(engine, "t1"));
        Assert.Empty(await Plans(engine, "t2"));
    }

    [Fact]
    public async Task A_belief_that_changes_changes_what_it_intends()
    {
        // WHY THIS AGENT IS OPEN-MINDED. Its world will not hold still: somebody opens a window and
        // the right thing to do changes. An agent that held its intention regardless would be a
        // worse thermostat than a bimetallic strip.
        var engine = ThermostatEngineFactory.Create();
        engine.AssertAll([new DesireFact("t3", "Comfortable"), new TemperatureFact("t3", 10)]);

        Assert.Equal(["Warm"], await Plans(engine, "t3"));

        engine.AssertAll([new TemperatureFact("t3", 30)]);

        Assert.Contains("Cool", await Plans(engine, "t3"));
    }
}
