// PerceptronTests.cs — a model with weights, learning something a line cannot.
//
// XOR IS THE FIXTURE because it is the smallest problem a single layer cannot solve and two layers
// can. A perceptron that learned AND would prove nothing a straight line had not already proved.
//
// ONE OF ITS FOUR ANSWERS IS NOT LEARNED, and it is worth being exact about which. This network has
// no bias term, so at the input (0, 0) every unit multiplies zero by its weights and the network
// answers zero whatever it has learned. XOR's target there happens to BE zero, so that row comes
// out right for free. The other three rows are genuinely learned.
//
// `ClassifierTests` shows the same architecture failing at the origin, because XOR written as two
// classes needs [1, 0] there -- and fixes it with a constant column of ones, which is the same way
// `Line` gets an intercept.
namespace MLambda.AI.ML.Tests;

using MLambda.AI.ML;
using MLambda.Hilbert.Runtime;

public class PerceptronTests
{
    private static Matrix Xs() => new double[,]
    {
        { 0d, 0d },
        { 0d, 1d },
        { 1d, 0d },
        { 1d, 1d },
    };

    private static Matrix Ys() => new double[,] { { 0d }, { 1d }, { 1d }, { 0d } };

    private static double LossOf(Perceptron net) => net.Loss(Xs(), Ys()).Values[0];

    [Fact]
    public void A_fresh_network_is_not_dead_on_arrival()
    {
        // THE SCALE ON THE DRAW. Initialised badly, every hidden unit dies and the network answers
        // exactly zero forever -- which looks like "training did nothing" and is really "the
        // network was dead before it started". So: the weights are drawn, and the answers vary.
        var net = new Perceptron(2, 8, seed: 7);

        Assert.Contains(net.First.Values, weight => weight != 0d);
        Assert.Contains(net.Eval(Xs()).Values, answer => answer != 0d);
    }

    [Fact]
    public void Training_reduces_the_error()
    {
        // NOT "THE ERROR IS SMALL". That would be a claim about this architecture on this data and
        // it would pin a number nobody can defend. "The error FELL" is what training means, and it
        // is the honest assertion.
        var net = new Perceptron(2, 8, seed: 7);
        var before = LossOf(net);

        for (var step = 0; step < 500; step++)
        {
            net.Train(Xs(), Ys(), 0.5);
        }

        var after = LossOf(net);

        Assert.True(after < before / 2, $"loss went from {before:F4} to {after:F4}");
    }

    [Fact]
    public void One_call_is_one_step_and_it_moves_the_weights()
    {
        // A MODEL HAS STATE, and a `fn` does not. Train once and the weights are different objects
        // with different values -- which is what lets a caller watch a fit happen.
        var net = new Perceptron(2, 8, seed: 7);
        var firstBefore = net.First.Values.ToArray();

        net.Train(Xs(), Ys(), 0.5);

        Assert.NotEqual(firstBefore, net.First.Values);
    }

    [Fact]
    public void The_same_seed_draws_the_same_network()
    {
        // REPRODUCIBLE BY CONSTRUCTION. Without this, "the error fell" could not be tested at all,
        // because the run that passed would not be the run anybody could repeat.
        var a = new Perceptron(2, 8, seed: 7);
        var b = new Perceptron(2, 8, seed: 7);

        Assert.Equal(a.First.Values, b.First.Values);
    }

    [Fact]
    public void And_a_different_seed_draws_a_different_one()
    {
        // NON-VACUOUS: the test above would pass for a network that ignored its seed entirely.
        var a = new Perceptron(2, 8, seed: 7);
        var b = new Perceptron(2, 8, seed: 8);

        Assert.NotEqual(a.First.Values, b.First.Values);
    }
}
