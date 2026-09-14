// CountingTests.cs — arithmetic that was checked at build time, by nothing you have to trust.
//
// THERE IS NO ENGINE HERE AND THAT IS THE POINT. `Counting.hs` declares a sort and one predicate so
// the proofs have something to quantify over; every claim is carried by its own certificate, and
// none of them is a rule anything could run. The project file removes this theory from the Shin
// inputs for exactly that reason.
namespace MLambda.AI.Logic.Tests;

using MLambda.AI.Logic;

public class CountingTests
{
    // ── Counting.hp, theorem by theorem: each one proved by the kernel while the test runs ──

    [Fact]
    public void Theorem_adding_then_taking_away() => Theorem.Proved(CountingProofs.AddingThenTakingAway());

    [Fact]
    public void Theorem_doubling_is_adding_twice() => Theorem.Proved(CountingProofs.DoublingIsAddingTwice());

    [Fact]
    public void Theorem_the_square_of_a_sum() => Theorem.Proved(CountingProofs.TheSquareOfASum());

    [Fact]
    public void Theorem_halfway_between() => Theorem.Proved(CountingProofs.HalfwayBetween());

    [Fact]
    public void Theorem_at_most_composes() => Theorem.Proved(CountingProofs.AtMostComposes());

    [Fact]
    public void Theorem_a_strict_step_survives() => Theorem.Proved(CountingProofs.AStrictStepSurvives());

    [Fact]
    public void Theorem_four_steps_compose() => Theorem.Proved(CountingProofs.FourStepsCompose());

    [Fact]
    public void Every_theorem_in_Counting_hp_has_a_test_above()
    {
        // A THEOREM ADDED TO THE FILE WITHOUT A TEST HERE FAILS THIS, so the list above stays whole.
        string[] tested =
        [
            "adding_then_taking_away",
            "doubling_is_adding_twice",
            "the_square_of_a_sum",
            "halfway_between",
            "at_most_composes",
            "a_strict_step_survives",
            "four_steps_compose",
        ];

        Assert.Equal(tested.Select(Theorem.Method), Theorem.Of(typeof(CountingProofs)));
    }

    [Fact]
    public void Strictness_survives_a_slack_step()
    {
        // The certificate reaches exactly zero here, and only the strict premise makes zero a
        // contradiction. It is the case that needs the strictness bookkeeping to be real.
        Theorem.Proved(CountingProofs.AStrictStepSurvives());
    }
}
