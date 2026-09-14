// MindTests.cs — a kitchen robot's state of mind.
//
// NOW the kettle is on and the cup is dirty. The robot could not see the cup (glimpse now →
// clean_cup) and assumed it clean (guess now → clean_cup). It wants tea (wish now → tea) and has
// planned for a clean cup (plan now → clean_cup).
namespace MLambda.AI.Minds.Tests;

using MLambda.AI.Minds;
using MLambda.AI.Minds.Mind;

public class MindTests
{
    private static IMindEngine Kitchen()
    {
        var engine = MindEngineFactory.Create();
        engine.AssertAll([
            new AgentFact("robot"),
            new WorldFact("now"), new WorldFact("clean_cup"), new WorldFact("tea"),
            new PropFact("kettle_on"), new PropFact("cup_clean"), new PropFact("tea_made"),
            new HoldsFact("now", "kettle_on"),
            new HoldsFact("clean_cup", "kettle_on"), new HoldsFact("clean_cup", "cup_clean"),
            new HoldsFact("tea", "kettle_on"), new HoldsFact("tea", "cup_clean"), new HoldsFact("tea", "tea_made"),
            new GlimpseFact("robot", "now", "clean_cup"),
            new GuessFact("robot", "now", "clean_cup"),
            new WishFact("robot", "now", "tea"),
            new PlanFact("robot", "now", "clean_cup"),
        ]);

        return engine;
    }

    [Fact]
    public async Task It_knows_only_what_is_true_in_every_world_it_cannot_rule_out()
    {
        Assert.Equal(["kettle_on"], await Answers.Sorted(Kitchen().Known("robot", "now")));
    }

    [Fact]
    public async Task It_believes_more_than_it_knows_including_something_false()
    {
        var kitchen = Kitchen();

        Assert.Equal(["cup_clean", "kettle_on"], await Answers.Sorted(kitchen.Believed("robot", "now")));
        Assert.DoesNotContain("cup_clean", await Answers.Sorted(kitchen.Known("robot", "now")));
    }

    [Fact]
    public async Task It_desires_what_is_not_yet_so()
    {
        // Desire is not factive: tea is not made, and that is exactly why it is wanted.
        Assert.Equal(["cup_clean", "kettle_on", "tea_made"], await Answers.Sorted(Kitchen().Desired("robot", "now")));
    }

    [Fact]
    public async Task And_intends_only_what_holds_in_every_world_it_is_committed_to()
    {
        // Realism adds the tea world to what it intends; the plan adds the clean-cup world. What holds
        // in both is intended — so a clean cup, but not yet the tea itself.
        var kitchen = Kitchen();

        Assert.Equal(["cup_clean", "kettle_on"], await Answers.Sorted(kitchen.Intended("robot", "now")));
        Assert.DoesNotContain("tea_made", await Answers.Sorted(kitchen.Intended("robot", "now")));
    }

    // ── Mind.hp, theorem by theorem: each one proved by the kernel while the test runs ──

    [Fact]
    public void Theorem_what_is_known_is_true_of_this_world() => Theorem.Proved(MindProofs.WhatIsKnownIsTrueOfThisWorld());

    [Fact]
    public void Theorem_believing_stays_within_knowing() => Theorem.Proved(MindProofs.BelievingStaysWithinKnowing());

    [Fact]
    public void Theorem_a_wish_is_desired() => Theorem.Proved(MindProofs.AWishIsDesired());

    [Fact]
    public void Theorem_a_plan_is_intended() => Theorem.Proved(MindProofs.APlanIsIntended());

    [Fact]
    public void Theorem_what_you_wish_for_you_intend() => Theorem.Proved(MindProofs.WhatYouWishForYouIntend());

    [Fact]
    public void Every_theorem_in_Mind_hp_has_a_test_above()
    {
        // A THEOREM ADDED TO THE FILE WITHOUT A TEST HERE FAILS THIS, so the list above stays whole.
        string[] tested =
        [
            "what_is_known_is_true_of_this_world",
            "believing_stays_within_knowing",
            "a_wish_is_desired",
            "a_plan_is_intended",
            "what_you_wish_for_you_intend",
        ];

        Assert.Equal(tested.Select(Theorem.Method), Theorem.Of(typeof(MindProofs)));
    }
}
