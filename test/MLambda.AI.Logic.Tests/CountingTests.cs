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
    public void Theorem_adding_then_taking_away() => Theorem.Proved(CountingProofs.Prove("adding_then_taking_away"));

    [Fact]
    public void Theorem_doubling_is_adding_twice() => Theorem.Proved(CountingProofs.Prove("doubling_is_adding_twice"));

    [Fact]
    public void Theorem_the_square_of_a_sum() => Theorem.Proved(CountingProofs.Prove("the_square_of_a_sum"));

    [Fact]
    public void Theorem_halfway_between() => Theorem.Proved(CountingProofs.Prove("halfway_between"));

    [Fact]
    public void Theorem_at_most_composes() => Theorem.Proved(CountingProofs.Prove("at_most_composes"));

    [Fact]
    public void Theorem_a_strict_step_survives() => Theorem.Proved(CountingProofs.Prove("a_strict_step_survives"));

    [Fact]
    public void Theorem_four_steps_compose() => Theorem.Proved(CountingProofs.Prove("four_steps_compose"));

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

        Assert.Equal(tested, CountingProofs.All.Select(claim => claim.Name));
    }

    [Fact]
    public void And_none_of_them_leaned_on_an_axiom()
    {
        // `axioms []`. The work is in the certificate and the kernel checked it, so there is
        // nothing in this corpus a reader has to take on trust -- which is the difference between
        // a decided claim and an assumed one.
        Assert.All(CountingProofs.All, claim => Assert.Equal(string.Empty, claim.Axioms));
    }

    [Fact]
    public void A_ring_identity_is_stated_without_quantifiers()
    {
        // `ring` normalises both sides to the same polynomial, so the variables are free and the
        // claim is an identity rather than an implication.
        Assert.Equal("a + b − b = a", CountingProofs.AddingThenTakingAway.Claim);
        Assert.DoesNotContain("⇒", CountingProofs.AddingThenTakingAway.Claim);
    }

    [Fact]
    public void An_order_claim_does_carry_its_hypotheses()
    {
        // `linarith` refutes the negation using the hypotheses in scope, so those hypotheses have
        // to be there -- which is why these claims are implications and the ring ones are not.
        var composes = CountingProofs.AtMostComposes.Claim;

        Assert.Contains("∀", composes);
        Assert.Contains("⇒", composes);
    }

    [Fact]
    public void Strictness_survives_a_slack_step()
    {
        // The certificate reaches exactly zero here, and only the strict premise makes zero a
        // contradiction. It is the case that needs the strictness bookkeeping to be real.
        Theorem.Proved(CountingProofs.Prove("a_strict_step_survives"));
        Assert.Contains("<", CountingProofs.AStrictStepSurvives.Claim);
    }
}
