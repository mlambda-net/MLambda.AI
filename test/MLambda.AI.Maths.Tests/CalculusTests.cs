// CalculusTests.cs — the same thirteen laws, at build time and at run time.
//
// `∂ u / ∂ x` IS AN ORDINARY TERM. Thirteen laws in Prelude.Calculus know how to remove one, each
// oriented so that applying it deletes a `∂`. When the last is gone, what is left is arithmetic —
// and the compiler emits that arithmetic, so nothing below differentiates while the test runs.
namespace MLambda.AI.Maths.Tests;

using MLambda.AI.Maths;
using MLambda.Hilbert.Algebra;

public class CalculusTests
{
    [Fact]
    public void A_derivative_taken_at_build_time_is_ordinary_arithmetic()
    {
        // 2x, for every x, with no differentiation happening now.
        Assert.Equal(6d, Calculus.Slope(3), 12);
        Assert.Equal(-4d, Calculus.Slope(-2), 12);
    }

    [Fact]
    public void And_the_chain_rule_is_one_of_the_laws_not_a_special_case()
    {
        // d/dx sin(x²) = cos(x²) · 2x
        Assert.Equal(Math.Cos(9d) * 6d, Calculus.Chain(3), 12);
    }

    [Fact]
    public void The_same_laws_take_the_same_derivative_at_run_time()
    {
        var derivative = Calculus.Compute.Derivate("sin(x²)", "x");

        var resolved = Assert.IsType<Resolved>(derivative);
        Assert.Contains("sine", resolved.Steps);
    }

    // ── the failure worth understanding ────────────────────────────────────────────────────────

    [Fact]
    public void A_derivative_no_law_can_take_removes_the_method_and_says_nothing()
    {
        // Calculus.hb declares `tangent`. The build accepted it and printed nothing. It is not
        // here, because `∂ tan(x) / ∂ x` still held a derivative when reduction ran out of laws.
        Assert.Null(typeof(Calculus).GetMethod("Tangent"));

        // THE CONTROL, so this cannot pass by a typo: the two that reduce ARE here.
        Assert.NotNull(typeof(Calculus).GetMethod("Slope", [typeof(double)]));
        Assert.NotNull(typeof(Calculus).GetMethod("Chain", [typeof(double)]));
    }

    [Fact]
    public void And_the_run_time_CAS_at_least_hands_back_the_residue()
    {
        // The CAS is the kinder of the two. It cannot take the derivative either, but it says so
        // with an Unresolved carrying what was left, instead of deleting a method in silence.
        var derivative = Calculus.Compute.Derivate("tan(x)", "x");

        var unresolved = Assert.IsType<Unresolved>(derivative);
        Assert.Contains("∂", unresolved.Text);
    }
}
