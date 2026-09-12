// WorldsTests.cs — the frame conditions, running.
//
// A "WORLD" IS A WAY THINGS COULD BE, and `sees(w, u)` says that from w, u is possible. Which
// frame conditions hold decides which modal logic you are in -- see docs/L2-practitioner/logic.md.
namespace MLambda.AI.Logic.Tests;

using MLambda.AI.Logic;
using MLambda.AI.Logic.Worlds;

public class WorldsTests
{
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
    public async Task A_declared_world_sees_itself()
    {
        // T, as this theory can state it. `place` is what binds the variable -- see Worlds.hs.
        var engine = WorldEngineFactory.Create();
        engine.AssertAll([new PlaceFact("here")]);

        Assert.Equal(["here"], await Sorted(engine.SeenFrom("here")));
    }

    [Fact]
    public async Task A_world_nobody_declared_sees_nothing()
    {
        // THE ONE EMPTY ANSWER, asserted alone -- and it is the honest consequence of binding
        // reflexivity to `place`. Reflexivity is not a fact about all worlds; it is a fact about
        // the worlds somebody said exist.
        var engine = WorldEngineFactory.Create();
        engine.AssertAll([new PlaceFact("here")]);

        Assert.Empty(await Sorted(engine.SeenFrom("elsewhere")));
    }

    [Fact]
    public async Task Transitivity_composes_a_chain()
    {
        var engine = WorldEngineFactory.Create();
        engine.AssertAll([new SeesFact("a", "b"), new SeesFact("b", "c")]);

        // c is reachable from a, and nothing asserted that.
        Assert.Contains("c", await Sorted(engine.SeenFrom("a")));
    }

    [Fact]
    public async Task Symmetry_runs_the_other_way()
    {
        var engine = WorldEngineFactory.Create();
        engine.AssertAll([new SeesFact("a", "b")]);

        Assert.Contains("a", await Sorted(engine.SeenFrom("b")));
    }

    [Fact]
    public async Task Seriality_holds_only_where_something_seeded_it()
    {
        // AXIOM D IS A PRECONDITION ON THE RUNTIME, not an inference. A seeded successor becomes
        // accessible; a world nobody seeded a successor for gets none, and nothing complains.
        var engine = WorldEngineFactory.Create();
        engine.AssertAll([new SeedFact("w1", "w2")]);

        var seen = await Sorted(engine.SeenFrom("w1"));

        Assert.NotEmpty(seen);
        Assert.Contains("w2", seen);
    }

    [Fact]
    public void S5_gets_B_and_4_without_being_handed_either()
    {
        // THE THEOREMS WORTH HAVING ARE THE ONES NOBODY GIVES YOU. T and 5 alone yield both.
        Assert.Equal("Proved", WorldsProofs.BFromTAnd5.Verdict);
        Assert.Equal("Proved", WorldsProofs.FourFromTAnd5.Verdict);
    }

    [Fact]
    public void And_every_theorem_records_which_axioms_it_leaned_on()
    {
        // "Proved under T and 5" is a different statement from "proved".
        Assert.Equal(7, WorldsProofs.All.Count);
        Assert.All(WorldsProofs.All, claim => Assert.Equal("Proved", claim.Verdict));
        Assert.Contains("refl", WorldsProofs.BFromTAnd5.Axioms);
        Assert.Contains("euclid", WorldsProofs.BFromTAnd5.Axioms);
    }

    [Fact]
    public void Reflexivity_is_stated_over_declared_worlds_only()
    {
        // THE CLAIM ITSELF CARRIES THE RESTRICTION, so a reader of the verdicts cannot mistake
        // this for the unrestricted T they were expecting.
        Assert.Contains("place(w)", WorldsProofs.TGivesEveryDeclaredWorldItself.Claim);
    }
}
