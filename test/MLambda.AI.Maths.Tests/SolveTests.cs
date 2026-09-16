// SolveTests.cs — the CAS refuses rather than guesses, and then you teach it.
//
// A TERM LAW MOVES A SUB-TERM; AN EQUATION LAW MOVES AN EQUATION. `x · 1 = x` rewrites part of an
// expression wherever it matches. None of that turns `2x + 3 = 7` into `x = 2`, because that means
// moving the equation itself — and only a law written with `⇔` can.
namespace MLambda.AI.Maths.Tests;

using MLambda.AI.Maths;
using MLambda.Hilbert.Algebra;

public class SolveTests
{
    [Fact]
    public void Without_an_equation_law_nothing_can_move_a_whole_equation()
    {
        // Algebra.hb opens Arithmetic and Trigonometry — fifteen laws, all of them term laws.
        Assert.IsType<Unsolved>(Algebra.Compute.Solve("2 · x + 3 = 7", "x"));
    }

    [Fact]
    public void Two_transposition_laws_are_enough_to_isolate_a_variable()
    {
        var solved = Solve.Compute.Solve("2 · x + 3 = 7", "x");

        Assert.Equal("x = 2", solved.ToString());
    }
}
