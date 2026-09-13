// NearestTests.cs — the models that do not train, on two clusters nobody could confuse.
//
// CLASS A sits near the origin and CLASS B near (10, 10). The gap is deliberately absurd, so every
// expected answer below is one a person can check by looking, and a failure means the model and
// not the fixture.
namespace MLambda.AI.ML.Tests;

using MLambda.AI.ML;
using MLambda.Hilbert.Runtime;

public class NearestTests
{
    private static Tensor Known() => (Matrix)new double[,]
    {
        { 0d, 0d }, { 0d, 1d }, { 1d, 0d },
        { 10d, 10d }, { 10d, 11d }, { 11d, 10d },
    };

    /// <summary>One row per known point, one column per class, 1 where it belongs.</summary>
    private static Tensor Classes() => (Matrix)new double[,]
    {
        { 1d, 0d }, { 1d, 0d }, { 1d, 0d },
        { 0d, 1d }, { 0d, 1d }, { 0d, 1d },
    };

    private static Tensor Asked() => (Matrix)new double[,] { { 0.5d, 0.5d }, { 10.5d, 10.5d } };

    [Fact]
    public void A_point_near_the_origin_is_class_A_and_one_near_ten_is_class_B()
    {
        // ONE-HOT PER ROW: the first question lands in column 0, the second in column 1.
        var answer = Nearest.ClassifyNear(Asked(), Known(), Classes(), 2d);

        Assert.Equal([1d, 0d, 0d, 1d], answer.Values);
    }

    [Fact]
    public void The_vote_shares_are_unanimous_when_the_clusters_are_this_far_apart()
    {
        // The 3 nearest to (0.5, 0.5) are all class A, so the share is 1 and 0 -- not a
        // probability anybody estimated, a count of who turned up.
        var shares = Nearest.SharesNear(Asked(), Known(), Classes(), 3d);

        Assert.Equal([1d, 0d, 0d, 1d], shares.Values.Select(v => Math.Round(v, 6)));
    }

    [Fact]
    public void Nearest_centroid_agrees_while_keeping_one_point_per_class()
    {
        // THE TRADE, stated as a result. k-NN keeps all six points; nearest-centroid keeps two
        // averages. On clusters this clean they give the same answer, which is exactly when the
        // cheaper model is the right one.
        var answer = Nearest.NearestCentre(Asked(), Known(), Classes());

        Assert.Equal([1d, 0d, 0d, 1d], answer.Values);
    }

    [Fact]
    public void Regression_averages_the_nearest_known_values()
    {
        // No training at all: the answer is the mean of the k nearest targets, computed now.
        Tensor known = (Matrix)new double[,] { { 0d }, { 1d }, { 2d }, { 10d } };
        Tensor values = (Vector)new double[] { 0d, 10d, 20d, 100d };
        Tensor asked = (Matrix)new double[,] { { 0.9d } };

        var answer = Nearest.RegressNear(asked, known, values, 3d);

        // The three nearest to 0.9 are 0, 1 and 2 -- whose values average to 10. The outlier at
        // 10 is not one of them, which is the whole point of asking only the neighbours.
        Assert.Equal(10d, answer.Values[0], 6);
    }
}
