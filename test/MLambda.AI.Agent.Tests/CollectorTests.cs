// CollectorTests.cs — the whole BDI cycle: believe, desire, intend, achieve, learn.
namespace MLambda.AI.Agent.Tests;

using MLambda.AI.Agent.Collector;

public class CollectorTests
{
    private static ICollectorEngine Holding(int count, string who = "c1")
    {
        var engine = CollectorEngineFactory.Create();
        engine.AssertAll([
            new DesireFact(who, "Enough"),
            new AttentionFact(who, "Gathering"),
            new GatheredFact(who, count),
        ]);

        return engine;
    }

    private static async Task<List<string>> Plans(ICollectorEngine engine, string who = "c1")
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
    public async Task Below_five_it_intends_to_collect()
    {
        Assert.Equal(["Collect"], await Plans(Holding(1)));
    }

    [Fact]
    public async Task And_it_keeps_intending_all_the_way_up()
    {
        // SINGLE-MINDED. The intention survives every step short of the goal -- it is not
        // reconsidered because the count changed, only because the goal was reached.
        Assert.Equal(["Collect"], await Plans(Holding(0)));
        Assert.Equal(["Collect"], await Plans(Holding(4)));
    }

    [Fact]
    public async Task At_five_it_intends_nothing_further()
    {
        // NON-VACUOUS: the two tests above show this engine does produce intentions.
        Assert.Empty(await Plans(Holding(5)));
    }

    [Fact]
    public async Task And_the_goal_is_met_at_five_and_not_before()
    {
        Assert.Equal(["Enough"], await Rows(Holding(5).Met("c1")));
        Assert.Empty(await Rows(Holding(4).Met("c1")));
    }

    [Fact]
    public async Task And_the_plan_that_got_there_earns_credit()
    {
        // `reward Collect positive when B(self) gathered(5)`. THIS IS THE SEAM to reinforcement
        // learning: the same feedback that marks a plan good here is what a learner maximises in
        // MLambda.AI.Learning.
        var rows = new List<CreditRow>();

        await foreach (var row in Holding(5).Credit("c1"))
        {
            rows.Add(row);
        }

        var only = Assert.Single(rows);

        Assert.Equal("Collect", only.Plan);
        Assert.Equal("positive", only.Sign);
    }

    [Fact]
    public async Task And_credit_is_only_earned_once_the_goal_is_reached()
    {
        // NON-VACUOUS: the test above shows credit is reported when it is due.
        var rows = new List<CreditRow>();

        await foreach (var row in Holding(3).Credit("c1"))
        {
            rows.Add(row);
        }

        Assert.Empty(rows);
    }

    [Fact]
    public async Task The_cycle_runs_end_to_end_as_beliefs_arrive()
    {
        // BELIEVE, DESIRE, INTEND, ACHIEVE, LEARN -- in that order, driven by nothing but the
        // count going up. Every line below is the engine re-deriving from changed beliefs.
        var engine = CollectorEngineFactory.Create();
        engine.AssertAll([new DesireFact("c9", "Enough"), new GatheredFact("c9", 2)]);

        Assert.Equal(["Collect"], await Plans(engine, "c9"));
        Assert.Empty(await Rows(engine.Met("c9")));

        engine.AssertAll([new GatheredFact("c9", 5)]);

        Assert.Equal(["Enough"], await Rows(engine.Met("c9")));
    }

    [Fact]
    public async Task One_collectors_facts_never_become_anothers()
    {
        var engine = CollectorEngineFactory.Create();
        engine.AssertAll([
            new DesireFact("a", "Enough"), new GatheredFact("a", 5),
            new DesireFact("b", "Enough"), new GatheredFact("b", 1),
        ]);

        Assert.Equal(["Enough"], await Rows(engine.Met("a")));
        Assert.Empty(await Rows(engine.Met("b")));
        Assert.Equal(["Collect"], await Plans(engine, "b"));
    }
}
