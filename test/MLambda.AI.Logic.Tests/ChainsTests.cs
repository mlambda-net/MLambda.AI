// ChainsTests.cs — the oldest rule in logic, chained.
//
// THE CLAIMS: raining → wet_ground → slippery → be_careful. Only "raining" is asserted true.
namespace MLambda.AI.Logic.Tests;

using MLambda.AI.Logic;
using MLambda.AI.Logic.Chains;

public class ChainsTests
{
    private static IChainEngine Weather()
    {
        var engine = ChainEngineFactory.Create();
        engine.AssertAll([
            new HoldsFact("raining"),
            new SaysFact("raining", "wet_ground"),
            new SaysFact("wet_ground", "slippery"),
            new SaysFact("slippery", "be_careful"),
        ]);

        return engine;
    }

    private static async Task<List<string>> Sorted(IAsyncEnumerable<string> rows)
    {
        var all = new List<string>();

        await foreach (var row in rows)
        {
            all.Add(row);
        }

        all.Sort(StringComparer.Ordinal);

        return all;
    }

    [Fact]
    public async Task What_a_claim_leads_to_directly()
    {
        Assert.Equal(["wet_ground"], await Sorted(Weather().Follows("raining")));
    }

    [Fact]
    public async Task One_assertion_and_the_rule_reaches_the_end_of_the_chain()
    {
        // THE POINT OF THE SAMPLE. Exactly one claim was asserted true. The other three are
        // concluded, and the rule that concluded them never mentions how long a chain may be.
        Assert.Equal(
            ["be_careful", "raining", "slippery", "wet_ground"],
            await Sorted(Weather().Truths()));
    }

    [Fact]
    public async Task A_chain_with_no_true_start_concludes_nothing()
    {
        // THE ONE EMPTY ANSWER, asserted alone. The implications are all there; nothing is true,
        // so nothing follows. An implication is not an assertion, which is the confusion this
        // sample exists to prevent.
        var engine = ChainEngineFactory.Create();
        engine.AssertAll([
            new SaysFact("raining", "wet_ground"),
            new SaysFact("wet_ground", "slippery"),
        ]);

        Assert.Empty(await Sorted(engine.Truths()));
    }

    [Fact]
    public async Task And_the_chain_does_not_run_backwards()
    {
        // Being careful does not make it rain. NON-VACUOUS: the chain concludes plenty, and
        // "be_careful" leads nowhere in particular.
        var truths = await Sorted(Weather().Truths());

        Assert.NotEmpty(truths);
        Assert.Empty(await Sorted(Weather().Follows("be_careful")));
    }

    [Fact]
    public async Task A_claim_added_later_is_chained_from()
    {
        var engine = Weather();

        Assert.DoesNotContain("stay_home", await Sorted(engine.Truths()));

        engine.AssertAll([new SaysFact("be_careful", "stay_home")]);

        Assert.Contains("stay_home", await Sorted(engine.Truths()));
    }

    [Fact]
    public void Every_theorem_was_proved()
    {
        Assert.Equal(4, ChainsProofs.All.Count);
        Assert.All(ChainsProofs.All, claim => Assert.Equal("Proved", claim.Verdict));
    }

    [Fact]
    public void The_chain_is_proved_for_any_links_at_all()
    {
        // The engine chained one particular set of weather claims; the proof chains any p, q, r, s.
        // That is the difference between the two dialects, in one assertion.
        Assert.Contains("∀", ChainsProofs.ThreeSteps.Claim);
        Assert.Equal("Proved", ChainsProofs.ThreeSteps.Verdict);
    }
}
