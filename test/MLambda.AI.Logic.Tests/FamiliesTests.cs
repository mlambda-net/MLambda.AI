// FamiliesTests.cs — recursion reaching a fixpoint, and a guard filtering.
//
// THE FAMILY:
//
//     alice ─┬─ bob ── dave
//            └─ carol
namespace MLambda.AI.Logic.Tests;

using MLambda.AI.Logic;
using MLambda.AI.Logic.Families;

public class FamiliesTests
{
    private static IFamilyEngine Family()
    {
        var engine = FamilyEngineFactory.Create();
        engine.AssertAll([
            new PersonFact("alice"), new PersonFact("bob"),
            new PersonFact("carol"), new PersonFact("dave"),
            new ParentFact("alice", "bob"),
            new ParentFact("alice", "carol"),
            new ParentFact("bob", "dave"),
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
    public async Task A_parent_is_an_ancestor()
    {
        Assert.Contains("bob", await Sorted(Family().Ancestors("alice")));
    }

    [Fact]
    public async Task And_recursion_reaches_the_grandchild()
    {
        // `indirect` composes parent with ancestor, and the theory never says how far to go.
        // dave is alice's grandchild, and nothing asserted that.
        Assert.Equal(["bob", "carol", "dave"], await Sorted(Family().Ancestors("alice")));
    }

    [Fact]
    public async Task Ancestry_does_not_run_backwards()
    {
        // THE ONE EMPTY ANSWER IN THIS FILE, asserted on its own so every other negative here can
        // assume a non-empty one and mean something by it.
        Assert.Empty(await Sorted(Family().Ancestors("dave")));
    }

    [Fact]
    public async Task Two_children_of_one_parent_are_siblings()
    {
        Assert.Equal(["carol"], await Sorted(Family().SiblingsOf("bob")));
        Assert.Equal(["bob"], await Sorted(Family().SiblingsOf("carol")));
    }

    [Fact]
    public async Task And_the_guard_stops_a_child_being_its_own_sibling()
    {
        // `x ≠ y` IS THE WHOLE OF IT. Without the guard bob is his own sibling, because the rule
        // matches him twice as a child of alice.
        //
        // NON-VACUOUS: bob has a sibling, so the list is not empty -- he simply is not in it.
        var siblings = await Sorted(Family().SiblingsOf("bob"));

        Assert.NotEmpty(siblings);
        Assert.DoesNotContain("bob", siblings);
    }

    [Fact]
    public async Task An_only_child_has_no_siblings()
    {
        // dave is alice's only grandchild and bob's only child.
        Assert.Empty(await Sorted(Family().SiblingsOf("dave")));
    }

    // ── Families.hp, theorem by theorem: each one proved by the kernel while the test runs ──

    [Fact]
    public void Theorem_a_parent_is_an_ancestor() => Theorem.Proved(FamiliesProofs.AParentIsAnAncestor());

    [Fact]
    public void Theorem_a_grandparent_is_an_ancestor() => Theorem.Proved(FamiliesProofs.AGrandparentIsAnAncestor());

    [Fact]
    public void Theorem_three_generations_compose() => Theorem.Proved(FamiliesProofs.ThreeGenerationsCompose());

    [Fact]
    public void Theorem_a_parent_of_an_ancestor_is_an_ancestor() => Theorem.Proved(FamiliesProofs.AParentOfAnAncestorIsAnAncestor());

    [Fact]
    public void Every_theorem_in_Families_hp_has_a_test_above()
    {
        // A THEOREM ADDED TO THE FILE WITHOUT A TEST HERE FAILS THIS, so the list above stays whole.
        string[] tested =
        [
            "a_parent_is_an_ancestor",
            "a_grandparent_is_an_ancestor",
            "three_generations_compose",
            "a_parent_of_an_ancestor_is_an_ancestor",
        ];

        Assert.Equal(tested.Select(Theorem.Method), Theorem.Of(typeof(FamiliesProofs)));
    }
}
