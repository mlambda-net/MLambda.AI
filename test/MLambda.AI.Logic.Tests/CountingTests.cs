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
    [Fact]
    public void Every_certificate_checked()
    {
        Assert.Equal(7, CountingProofs.All.Count);
        Assert.All(CountingProofs.All, claim => Assert.Equal("Proved", claim.Verdict));
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
        Assert.Equal("Proved", CountingProofs.AStrictStepSurvives.Verdict);
        Assert.Contains("<", CountingProofs.AStrictStepSurvives.Claim);
    }
}
