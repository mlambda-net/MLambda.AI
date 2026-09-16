// ProofTests.cs — the laws the CAS spent, proved rather than assumed.
//
// THREE GRADES OF CERTAINTY, AND THEY ARE NOT THE SAME. `Demonstrate` searches for steps and may
// not find them. `Proof` checks steps you already have. The `.hp` kernel checks a derivation at
// build time and fails the build. This file exercises all three over the same mathematics.
namespace MLambda.AI.Maths.Tests;

using MLambda.AI.Maths;
using MLambda.Hilbert.Algebra;

public class ProofTests
{
    [Fact]
    public void The_derivative_of_x_squared_is_two_x_by_the_product_rule()
    {
        Theorem.Proved(MathsProofs.DerivOfXSquared());
    }

    [Fact]
    public void And_the_rule_it_leans_on_is_usable_on_its_own()
    {
        Theorem.Proved(MathsProofs.TheProductRuleIsUsable());
        Theorem.Proved(MathsProofs.AConstantHasNoSlope());
    }

    [Fact]
    public void Demonstrate_searches_for_steps_and_Proof_then_checks_them()
    {
        // The round trip: what the search writes back is a script the checker accepts.
        var found = Algebra.Compute.Demonstrate("theorem t : x · 1 + 0 = x");

        var demonstrated = Assert.IsType<Demonstrated>(found);
        Assert.IsType<Checked>(Algebra.Compute.Proof(demonstrated.Theorem));
    }

    [Fact]
    public void A_search_that_runs_out_of_room_says_so_instead_of_answering()
    {
        // `tan` appears in no law Algebra.hb can see, so no amount of searching closes this.
        // Bounded search means it comes back, rather than running forever.
        Assert.IsType<NotFound>(Algebra.Compute.Demonstrate("theorem t : tan(x) = x"));
    }
}
