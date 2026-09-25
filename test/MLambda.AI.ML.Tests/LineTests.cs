// LineTests.cs — the first .hb in this repository, running.
//
// ONE `fn` BECAME MANY METHODS. The compiler emits each formula once for reals, once for complex
// numbers, once for every tensor rank, and once more as an ONNX graph an executor can run. These
// tests spend three of those, because the fact that they are the SAME formula is the point.
namespace MLambda.AI.ML.Tests;

using MLambda.AI.ML;
using MLambda.Hilbert.Runtime;

public class LineTests
{
    [Fact]
    public void A_slope_is_rise_over_run()
    {
        Assert.Equal(2d, Line.SlopeOf(4d, 2d));
    }

    [Fact]
    public void An_exact_prediction_has_no_error()
    {
        Assert.Equal(0d, Line.ErrorOf(3d, 3d));
    }

    [Fact]
    public void And_error_is_squared_so_direction_does_not_matter()
    {
        // Missing by two is the same error whichever way you missed.
        Assert.Equal(Line.ErrorOf(5d, 3d), Line.ErrorOf(1d, 3d));
        Assert.Equal(4d, Line.ErrorOf(5d, 3d));
    }

    [Fact]
    public void The_same_formula_runs_over_a_whole_vector_at_once()
    {
        // NOT A LOOP THIS FILE WROTE. `errorOf` was declared over two names and the compiler
        // emitted a tensor overload from the same expression -- so a batch of predictions costs
        // one call and no extra source.
        Vector predicted = new double[] { 1d, 2d, 3d };
        Vector actual = new double[] { 1d, 4d, 0d };

        var errors = Line.ErrorOf(predicted, actual);

        Assert.Equal([0d, 4d, 9d], errors.Values);
    }

    [Fact]
    public void And_a_scalar_broadcasts_across_one()
    {
        Vector actual = new double[] { 1d, 2d, 3d };

        var errors = Line.ErrorOf(3d, actual);

        Assert.Equal([4d, 1d, 0d], errors.Values);
    }

    [Fact]
    public void The_formula_is_also_an_onnx_graph()
    {
        // THE FORMULA COMPILED TWICE: once to C# arithmetic, and once to a graph an executor runs.
        // `errorOf` is one line of source and two ONNX nodes -- one subtraction and a multiply that
        // reads it twice. `(p − a) · (p − a)` names the difference twice, and the graph emitter keeps
        // one node per structurally distinct term, so the second mention is the first node again.
        Assert.Equal("errorOf", Line.ErrorOfGraph.Name);
        Assert.Equal(2, Line.ErrorOfGraph.Nodes.Count);
        Assert.Equal(["sub", "sub"], Line.ErrorOfGraph.Nodes[1].Inputs);

        Assert.Equal("slopeOf", Line.SlopeOfGraph.Name);
        Assert.Single(Line.SlopeOfGraph.Nodes);
    }

    // ── the fit ────────────────────────────────────────────────────────────────────────────────
    //
    // THE DESIGN MATRIX CARRIES A COLUMN OF ONES, which is how a straight line gets an intercept
    // out of a method that only knows how to weight columns. Column one is x; column two is 1; so
    // the coefficients come back as [slope, intercept].

    /// <summary>Points from y = 3x + 1, exactly.</summary>
    private static Matrix Inputs() => new double[,]
    {
        { 1d, 1d },
        { 2d, 1d },
        { 3d, 1d },
        { 4d, 1d },
    };

    private static Vector Outputs() => new double[] { 4d, 7d, 10d, 13d };

    [Fact]
    public void A_fit_recovers_a_line_it_was_given()
    {
        // THE STRONGEST TEST A FIT CAN HAVE. Points from a line with no noise in them, fitted, and
        // the line comes back. A fit that cannot do this is not a fit, whatever else it does.
        var line = Line.LineOf(Inputs(), Outputs());

        Assert.Equal(3d, line.Values[0], 6);
        Assert.Equal(1d, line.Values[1], 6);
    }

    [Fact]
    public void And_predicts_where_the_line_goes_next()
    {
        var line = Line.LineOf(Inputs(), Outputs());
        Matrix further = new double[,] { { 10d, 1d } };

        var predicted = Line.PredictWith((Tensor)line, (Tensor)further);

        Assert.Equal(31d, predicted.Values[0], 6);
    }

    [Fact]
    public void A_line_with_nothing_left_over_has_no_spread()
    {
        // NO NOISE IN, NO SPREAD OUT. The corpus agrees with itself perfectly about these points.
        Assert.Equal(0d, Line.SpreadOf(Inputs(), Outputs()).Values[0], 6);
    }

    [Fact]
    public void And_data_that_disagrees_with_itself_has_some()
    {
        // NON-VACUOUS, and the reason spread is reported beside the fit at all. An offer two
        // thousand above fair value means one thing on a spread of thirty thousand and quite
        // another on a spread of five hundred.
        Vector noisy = new double[] { 4d, 8d, 9d, 13d };

        var spread = Line.SpreadOf(Inputs(), noisy).Values[0];

        Assert.True(spread > 0.1d, $"expected the residuals to spread, got {spread}");
    }

    [Fact]
    public void The_fitted_values_are_the_line_read_back_at_the_inputs()
    {
        var fitted = Line.FittedTo(Inputs(), Outputs());

        Assert.Equal([4d, 7d, 10d, 13d], fitted.Values.Select(v => Math.Round(v, 6)));
    }
}
