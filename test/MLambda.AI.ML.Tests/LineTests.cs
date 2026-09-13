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
        // `errorOf` is one line of source and three ONNX nodes -- two subtractions and a multiply,
        // because `(p − a) · (p − a)` names the difference twice and nothing folded it.
        Assert.Equal("errorOf", Line.ErrorOfGraph.Name);
        Assert.Equal(3, Line.ErrorOfGraph.Nodes.Count);

        Assert.Equal("slopeOf", Line.SlopeOfGraph.Name);
        Assert.Single(Line.SlopeOfGraph.Nodes);
    }
}
