// LinearTests.cs — one contraction underneath, and two different words called solve.
namespace MLambda.AI.Maths.Tests;

using MLambda.AI.Maths;
using MLambda.Hilbert.Runtime;

public class LinearTests
{
    /// <summary>A 2×2 worth checking by hand: [[1, 2], [3, 4]].</summary>
    private static Matrix Small() => new double[,] { { 1, 2 }, { 3, 4 } };

    /// <summary>The 2×2 identity.</summary>
    private static Matrix Identity() => new double[,] { { 1, 0 }, { 0, 1 } };

    [Fact]
    public void A_matrix_product_is_a_contraction_over_one_index()
    {
        var product = Linear.Product(Small(), Identity());

        Assert.Equal(1d, product[0, 0], 12);
        Assert.Equal(2d, product[0, 1], 12);
        Assert.Equal(3d, product[1, 0], 12);
        Assert.Equal(4d, product[1, 1], 12);
    }

    [Fact]
    public void And_naming_it_changed_nothing_at_all()
    {
        // `mul` IS `einsum("ij,jk->ik", …)`. The prelude defines it that way, a `def` is inlined,
        // and so these two doorways are the same arithmetic reached by two names.
        var named = Linear.Product(Small(), Small());
        var spelled = Linear.Contracted(Small(), Small());

        Assert.Equal(named[0, 0], spelled[0, 0], 12);
        Assert.Equal(named[0, 1], spelled[0, 1], 12);
        Assert.Equal(named[1, 0], spelled[1, 0], 12);
        Assert.Equal(named[1, 1], spelled[1, 1], 12);
    }

    [Fact]
    public void The_numeric_solve_answers_a_linear_system_with_numbers()
    {
        // 2x = 4, 3y = 9 — diagonal, so conjugate gradient has an easy time and the answer is
        // checkable in your head.
        Matrix a = new double[,] { { 2, 0 }, { 0, 3 } };
        Vector b = new double[] { 4, 9 };

        var x = Linear.Solved(a, b);

        Assert.Equal(2d, x[0], 6);
        Assert.Equal(3d, x[1], 6);
    }
}
