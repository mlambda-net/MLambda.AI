// AdjudicatorTests.cs — a veto, not a vote.
namespace MLambda.AI.Actuarial.Tests;

using MLambda.AI.Actuarial;
using MLambda.AI.Actuarial.Adjudicator;

public class AdjudicatorTests
{
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

    private static async Task<string> VerdictFor(IAdjudicatorEngine engine, string purchase)
    {
        var walk = await Rows(engine.WalkVerdicts());
        var negotiate = await Rows(engine.NegotiateVerdicts());
        var buy = await Rows(engine.BuyVerdicts());

        var verdicts = new List<string>();
        if (walk.Contains(purchase)) verdicts.Add("Walk");
        if (negotiate.Contains(purchase)) verdicts.Add("Negotiate");
        if (buy.Contains(purchase)) verdicts.Add("Buy");

        // EXACTLY ONE, ALWAYS. Asserted here rather than trusted, because the whole point of stratifying
        // the negations is that the three verdicts cannot both fire for the same purchase.
        return Assert.Single(verdicts);
    }

    [Fact]
    public async Task All_three_agents_buying_is_a_buy()
    {
        var engine = AdjudicatorEngineFactory.Create();
        engine.AssertAll([
            new BuysFact("valuation", "h1"), new BuysFact("condition", "h1"), new BuysFact("credit", "h1"),
        ]);

        Assert.Equal("Buy", await VerdictFor(engine, "h1"));
    }

    [Fact]
    public async Task One_negotiation_among_buys_is_a_negotiation()
    {
        var engine = AdjudicatorEngineFactory.Create();
        engine.AssertAll([
            new BuysFact("valuation", "h1"), new HagglesFact("condition", "h1"), new BuysFact("credit", "h1"),
        ]);

        Assert.Equal("Negotiate", await VerdictFor(engine, "h1"));
    }

    [Fact]
    public async Task One_agent_seeing_ruin_outranks_two_seeing_a_bargain()
    {
        // A VETO, NOT A VOTE. Two buys and one walk is a walk -- deliberately. The cost of a wrong buy is
        // unbounded and the cost of a wrong walk is a missed house.
        var engine = AdjudicatorEngineFactory.Create();
        engine.AssertAll([
            new BuysFact("valuation", "h1"), new BuysFact("condition", "h1"), new WalksFact("credit", "h1"),
        ]);

        Assert.Equal("Walk", await VerdictFor(engine, "h1"));
    }

    [Fact]
    public async Task And_a_walk_outranks_a_negotiation_too()
    {
        var engine = AdjudicatorEngineFactory.Create();
        engine.AssertAll([
            new BuysFact("valuation", "h1"), new HagglesFact("condition", "h1"), new WalksFact("credit", "h1"),
        ]);

        Assert.Equal("Walk", await VerdictFor(engine, "h1"));
    }

    [Fact]
    public async Task A_purchase_nobody_voted_on_has_no_verdict()
    {
        // NOT A DEFAULT BUY. Silence is not agreement, which is why `verdict_buy` needs somebody to have
        // voted. Without that clause an empty panel would buy everything.
        var engine = AdjudicatorEngineFactory.Create();
        engine.AssertAll([new BuysFact("valuation", "h1")]);

        Assert.DoesNotContain("h2", await Rows(engine.BuyVerdicts()));
        Assert.Contains("h1", await Rows(engine.BuyVerdicts()));
    }

    [Fact]
    public async Task Two_purchases_are_judged_separately()
    {
        var engine = AdjudicatorEngineFactory.Create();
        engine.AssertAll([
            new BuysFact("valuation", "h1"), new BuysFact("credit", "h1"),
            new BuysFact("valuation", "h2"), new WalksFact("credit", "h2"),
        ]);

        Assert.Equal("Buy", await VerdictFor(engine, "h1"));
        Assert.Equal("Walk", await VerdictFor(engine, "h2"));
    }

    // ── Adjudicator.hp, theorem by theorem: each one proved by the kernel while the test runs ──

    [Fact]
    public void Theorem_one_walk_is_enough() => Theorem.Proved(AdjudicatorProofs.OneWalkIsEnough());

    [Fact]
    public void Theorem_a_haggle_is_a_vote() => Theorem.Proved(AdjudicatorProofs.AHaggleIsAVote());

    [Fact]
    public void Theorem_a_haggle_contests_the_purchase() => Theorem.Proved(AdjudicatorProofs.AHaggleContestsThePurchase());

    [Fact]
    public void Theorem_a_buy_is_a_vote() => Theorem.Proved(AdjudicatorProofs.ABuyIsAVote());

    [Fact]
    public void Every_theorem_in_Adjudicator_hp_has_a_test_above()
    {
        string[] tested = ["one_walk_is_enough", "a_haggle_is_a_vote", "a_haggle_contests_the_purchase", "a_buy_is_a_vote"];

        Assert.Equal(tested.Select(Theorem.Method), Theorem.Of(typeof(AdjudicatorProofs)));
    }
}
