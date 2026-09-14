// SortsTests.cs — classification that individuates, and the law that is deliberately missing.
namespace MLambda.AI.Logic.Tests;

using MLambda.AI.Logic;
using MLambda.AI.Logic.Sorts;

public class SortsTests
{
    // tabby ⊑ cat ⊑ mammal, cat is disjoint from bird, and fluffy is a tabby.
    private static ISortEngine Kinds()
    {
        var engine = SortEngineFactory.Create();
        engine.AssertAll([
            new KindFact("tabby"), new KindFact("cat"),
            new KindFact("mammal"), new KindFact("bird"),
            new SubFact("tabby", "cat"),
            new SubFact("cat", "mammal"),
            new DisjointFact("cat", "bird"),
            new InstFact("fluffy", "tabby"),
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
    public async Task Classification_climbs_the_whole_order()
    {
        // Only `inst(fluffy, tabby)` was asserted. cat and mammal are concluded, two steps up.
        Assert.Equal(["cat", "mammal", "tabby"], await Sorted(Kinds().SortsOf("fluffy")));
    }

    [Fact]
    public async Task A_thing_in_two_disjoint_sorts_is_a_contradiction()
    {
        var engine = SortEngineFactory.Create();
        engine.AssertAll([
            new KindFact("cat"), new KindFact("bird"),
            new DisjointFact("cat", "bird"),
            new InstFact("chimera", "cat"),
            new InstFact("chimera", "bird"),
        ]);

        Assert.Equal(["chimera"], await Sorted(engine.Impossible()));
    }

    [Fact]
    public async Task And_disjointness_reaches_down_into_subsorts()
    {
        // tabby ⊑ cat and cat is disjoint from bird, so a tabby bird is a contradiction -- and
        // nothing asserted that tabbies and birds are disjoint.
        var engine = Kinds();
        engine.AssertAll([new InstFact("oddity", "tabby"), new InstFact("oddity", "bird")]);

        Assert.Contains("oddity", await Sorted(engine.Impossible()));
    }

    [Fact]
    public async Task A_consistent_world_reports_no_contradictions()
    {
        // NON-VACUOUS: the two tests above show this query does report contradictions when there
        // are any, so an empty answer here is a real absence rather than a query that never works.
        Assert.Empty(await Sorted(Kinds().Impossible()));
    }

    [Fact]
    public async Task Sameness_climbs_the_sort_hierarchy()
    {
        // alice and bob are the same passenger. Every passenger is a traveller, so they are the
        // same traveller too -- and nothing asserted that.
        var engine = SortEngineFactory.Create();
        engine.AssertAll([
            new KindFact("passenger"), new KindFact("traveller"),
            new SubFact("passenger", "traveller"),
            new SameFact("alice", "bob", "passenger"),
        ]);

        Assert.Equal(
            ["passenger", "traveller"],
            await Sorted(engine.SortsSharing("alice", "bob")));
    }

    [Fact]
    public async Task But_it_does_not_descend()
    {
        // THE POINT OF THE ENTIRE SUBJECT, as a running assertion rather than a comment.
        //
        // Every person is a passenger. alice and bob are the same PASSENGER. It does not follow --
        // and must not follow -- that they are the same PERSON. Two people can share a seat
        // reservation on successive days.
        //
        // NON-VACUOUS: the answer contains "passenger", so the query works and the absence of
        // "person" is a real absence.
        var engine = SortEngineFactory.Create();
        engine.AssertAll([
            new KindFact("person"), new KindFact("passenger"),
            new SubFact("person", "passenger"),
            new SameFact("alice", "bob", "passenger"),
        ]);

        var shared = await Sorted(engine.SortsSharing("alice", "bob"));

        Assert.Contains("passenger", shared);
        Assert.DoesNotContain("person", shared);
    }

    // ── Sorts.hp, theorem by theorem: each one proved by the kernel while the test runs ──

    [Fact]
    public void Theorem_a_declared_kind_falls_under_itself() => Theorem.Proved(SortsProofs.ADeclaredKindFallsUnderItself());

    [Fact]
    public void Theorem_subsumption_composes() => Theorem.Proved(SortsProofs.SubsumptionComposes());

    [Fact]
    public void Theorem_an_instance_climbs() => Theorem.Proved(SortsProofs.AnInstanceClimbs());

    [Fact]
    public void Theorem_an_instance_climbs_twice() => Theorem.Proved(SortsProofs.AnInstanceClimbsTwice());

    [Fact]
    public void Theorem_nothing_is_both() => Theorem.Proved(SortsProofs.NothingIsBoth());

    [Fact]
    public void Theorem_disjointness_inherits() => Theorem.Proved(SortsProofs.DisjointnessInherits());

    [Fact]
    public void Theorem_a_subsort_still_excludes() => Theorem.Proved(SortsProofs.ASubsortStillExcludes());

    [Fact]
    public void Theorem_sameness_climbs() => Theorem.Proved(SortsProofs.SamenessClimbs());

    [Fact]
    public void Theorem_sameness_is_symmetric() => Theorem.Proved(SortsProofs.SamenessIsSymmetric());

    [Fact]
    public void Theorem_sameness_composes() => Theorem.Proved(SortsProofs.SamenessComposes());

    [Fact]
    public void Every_theorem_in_Sorts_hp_has_a_test_above()
    {
        // A THEOREM ADDED TO THE FILE WITHOUT A TEST HERE FAILS THIS, so the list above stays whole.
        string[] tested =
        [
            "a_declared_kind_falls_under_itself",
            "subsumption_composes",
            "an_instance_climbs",
            "an_instance_climbs_twice",
            "nothing_is_both",
            "disjointness_inherits",
            "a_subsort_still_excludes",
            "sameness_climbs",
            "sameness_is_symmetric",
            "sameness_composes",
        ];

        Assert.Equal(tested.Select(Theorem.Method), Theorem.Of(typeof(SortsProofs)));
    }

    [Fact]
    public void And_nothing_here_proves_that_sameness_descends()
    {
        // THE NON-THEOREM, ASSERTED AGAINST THE CORPUS ITSELF. `same_up` goes one way only, and
        // this test fails the day somebody adds a descending law or a theorem claiming one --
        // which is the only honest way to record an absence. Not `sorry`: that means "not proved
        // yet", and this is "not provable, on purpose".
        Assert.DoesNotContain(
            Theorem.Of(typeof(SortsProofs)),
            name => name.Contains("Descend", StringComparison.Ordinal));
    }
}
