// AlgebraTests.cs — the laws did it, and the arithmetic can prove they did.
//
// NOTHING HERE ASSERTS ARITHMETIC THE COMPILER KNOWS. The compiler knows no identities at all;
// every number below is the consequence of a `law` that Algebra.hb opened.
//
// AND THE ASSERTION IS EXACT ON PURPOSE. `sin(x)² + cos(x)²` computed in doubles is not 1 — it is
// 0.9999999999999999 at x = 0.009 and 1.0000000000000002 at x = 0.017. Folded by `pythagorean` it
// is the literal 1, bit for bit, because no sine is ever taken. An approximate assertion would
// pass either way and would therefore test nothing.
namespace MLambda.AI.Maths.Tests;

using MLambda.AI.Maths;

public class AlgebraTests
{
    [Fact]
    public void A_pythagorean_identity_folds_to_one_before_the_program_runs()
    {
        Assert.Equal(1d, Algebra.Energy(0.009));
        Assert.Equal(1d, Algebra.Energy(0.017));
    }

    [Fact]
    public void And_the_arithmetic_it_replaced_would_not_have_given_one()
    {
        // THE CONTROL. If this ever equals 1 the compiler changed, not the mathematics — and the
        // test above stopped meaning anything.
        Assert.NotEqual(1d, (Math.Sin(0.009) * Math.Sin(0.009)) + (Math.Cos(0.009) * Math.Cos(0.009)));
    }
}
