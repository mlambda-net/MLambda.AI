// OneLawTests.cs — a CAS that knows one thing, and the reason it knows no more.
//
// THE SCOPE IS PER FILE. OneLaw.hb opens nothing, so its `Compute` fires one law. Algebra.hb sits
// beside it in the same project and the same namespace, and knows fifteen. Neither borrows from
// the other, and that is the documented rule, not an accident of this project's layout.
namespace MLambda.AI.Maths.Tests;

using MLambda.AI.Maths;

public class OneLawTests
{
    [Fact]
    public void One_law_is_enough_to_be_a_computer_algebra_system()
    {
        var simplified = OneLaw.Compute.Simplify("x · 1");

        Assert.Equal("x", simplified.Text);
        Assert.Equal(["only_unit"], simplified.Steps);
    }

    [Fact]
    public void And_it_cannot_do_the_thing_nobody_told_it()
    {
        // `x + 0 = x` is `zero`, and `zero` lives in Prelude.Arithmetic, which this file does not
        // open. The expression comes back untouched — not wrong, just not simplified.
        var simplified = OneLaw.Compute.Simplify("x + 0");

        Assert.Equal("x + 0", simplified.Text);
        Assert.Empty(simplified.Steps);
    }

    [Fact]
    public void The_file_next_door_can_do_it_and_that_is_the_point()
    {
        Assert.Equal("x", Algebra.Compute.Simplify("x + 0").Text);
    }
}
