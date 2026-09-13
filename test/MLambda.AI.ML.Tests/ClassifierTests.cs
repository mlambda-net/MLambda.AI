// ClassifierTests.cs — a class is one more index.
namespace MLambda.AI.ML.Tests;

using MLambda.AI.ML;
using MLambda.Hilbert.Runtime;

public class ClassifierTests
{
    // THE THIRD COLUMN IS A CONSTANT ONE, and without it this classifier cannot be right.
    //
    // NOT A BUG, A DOCUMENTED CONVENTION. Prelude/Networks.hb says in its header: "A BIAS IS A
    // COLUMN OF ONES IN THE DESIGN ... so no layer here carries one, and a caller who wants a bias
    // augments `x`." This file first found that out by failing, which is worth admitting: reading
    // the module header would have said it in one line.
    //
    // So no layer has a bias term, and every unit multiplies its input by its weights and nothing
    // else. At the input (0, 0) that product is zero whatever the weights are -- so the
    // network answers zero there, and no amount of training changes it. XOR as two classes needs
    // the answer [1, 0] at the origin, which a bias-free network can never give.
    //
    // A column of ones is the fix, and it is the same fix `Line` uses to get an intercept: a model
    // that only knows how to weight columns gets a constant by being handed a column that is
    // always one. `Bias_free_networks_are_stuck_at_zero_on_the_origin` below shows the failure.
    private static Matrix Xs() => new double[,]
    {
        { 0d, 0d, 1d }, { 0d, 1d, 1d }, { 1d, 0d, 1d }, { 1d, 1d, 1d },
    };

    // SEED 1, AND THE REASON IS NOT THAT IT PASSES. Seed 7 -- used everywhere else in this project --
    // kills five of this network's eight hidden units, and the three that survive cannot tell row 0
    // (0,0,1) from row 2 (1,0,1). Those rows want opposite answers, so the network settles on the
    // compromise between them, their gradients cancel EXACTLY, and training stops dead: loss 0.25
    // after 500 steps and after 3000, weights unchanged to the last bit. Seeds 1, 2 and 3 all
    // reach every row correctly. Choosing a seed that happens to pass WITHOUT saying so would be
    // cherry-picking; so seed 7's failure is a test of its own, `A_bad_start_is_not_rescued_by_more_training`.
    private const double GoodSeed = 1d;
    private const double StuckSeed = 7d;

    private static Matrix XsWithoutBias() => new double[,]
    {
        { 0d, 0d }, { 0d, 1d }, { 1d, 0d }, { 1d, 1d },
    };

    /// <summary>XOR again, now as two classes: column 0 is "off", column 1 is "on".</summary>
    private static Matrix Ys() => new double[,]
    {
        { 1d, 0d }, { 0d, 1d }, { 0d, 1d }, { 1d, 0d },
    };

    private static Classifier Trained(double seed = GoodSeed, int steps = 500)
    {
        var net = new Classifier(3, 8, 2, seed: seed);

        for (var step = 0; step < steps; step++)
        {
            net.Train(Xs(), Ys(), 0.5);
        }

        return net;
    }

    [Fact]
    public void Every_row_of_shares_is_a_distribution()
    {
        // WHAT THE SOFTMAX IS FOR: non-negative, and each row sums to one.
        var shares = new Classifier(3, 8, 2, seed: 7).Shares(Xs()).Values;

        for (var row = 0; row < 4; row++)
        {
            var off = shares[row * 2];
            var on = shares[(row * 2) + 1];

            Assert.True(off >= 0d && on >= 0d);
            Assert.Equal(1d, off + on, 9);
        }
    }

    [Fact]
    public void The_softmax_never_changes_which_class_wins()
    {
        // MONOTONE, so the scores and the shares rank the classes identically. That is why the
        // softmax can live at the call site: taking it out of the network loses nothing.
        var net = new Classifier(3, 8, 2, seed: 7);
        var scores = net.Eval(Xs()).Values;
        var shares = net.Shares(Xs()).Values;

        for (var row = 0; row < 4; row++)
        {
            var scoreSaysOn = scores[(row * 2) + 1] > scores[row * 2];
            var shareSaysOn = shares[(row * 2) + 1] > shares[row * 2];

            Assert.Equal(scoreSaysOn, shareSaysOn);
        }
    }

    [Fact]
    public void Training_reduces_the_error()
    {
        var before = new Classifier(3, 8, 2, seed: GoodSeed).Loss(Xs(), Ys()).Values[0];
        var after = Trained().Loss(Xs(), Ys()).Values[0];

        Assert.True(after < before / 2, $"loss went from {before:F4} to {after:F4}");
    }

    [Fact]
    public void And_once_trained_it_picks_the_right_class_for_every_row()
    {
        // The winner is one-hot per row, so it should equal the targets exactly.
        Assert.Equal(Ys().Values, Trained().Winner(Xs()).Values);
    }

    [Fact]
    public void Bias_free_networks_are_stuck_at_zero_on_the_origin()
    {
        // THE FAILURE THE BIAS COLUMN FIXES, kept as a test so it stays understood. Trained on
        // inputs with no constant column, the origin scores exactly zero for BOTH classes -- a tie
        // that `winner` reports as two winners -- and training cannot move it.
        var net = new Classifier(2, 8, 2, seed: 7);

        for (var step = 0; step < 500; step++)
        {
            net.Train(XsWithoutBias(), Ys(), 0.5);
        }

        var origin = net.Eval(XsWithoutBias()).Values;

        Assert.Equal(0d, origin[0]);
        Assert.Equal(0d, origin[1]);
    }

    [Fact]
    public void A_bad_start_is_not_rescued_by_more_training()
    {
        // THE LESSON SEED 7 TEACHES, pinned so it stays true -- and it is DYING RELU, not a bug.
        //
        // A rectified unit whose input is negative for every example outputs zero and passes back a
        // zero gradient, so it never recovers. By step 500 seed 7 has five of its eight units there
        // measured after training, so they may have died on the way rather than at the draw. The three
        // survivors cannot separate the two rows that want opposite answers, so the network sits at
        // their compromise, where the gradients from those rows cancel exactly.
        //
        // NOT A PLATEAU, which is the word an earlier version of this test used. A plateau has a
        // small slope and training creeps across it. This has NO slope: the weights after 1500
        // steps are bit-for-bit the weights after 500. More training cannot help, because there is
        // nothing for it to follow.
        //
        // Real training restarts from several seeds and keeps the best, for exactly this reason.
        var net = Trained(StuckSeed, steps: 500);
        var weightsAt500 = net.First.Values.ToArray();

        for (var step = 0; step < 1000; step++)
        {
            net.Train(Xs(), Ys(), 0.5);
        }

        Assert.Equal(weightsAt500, net.First.Values);
        Assert.True(net.Loss(Xs(), Ys()).Values[0] > 0.1d, "expected seed 7 to stay stuck");
    }

    [Fact]
    public void While_a_good_start_is_still_moving_at_the_same_point()
    {
        // NON-VACUOUS: the test above would pass for a network whose Train did nothing at all.
        // Seed 1 at the same step is converged but not frozen -- its weights still change.
        var net = Trained(GoodSeed, steps: 500);
        var weightsAt500 = net.First.Values.ToArray();

        net.Train(Xs(), Ys(), 0.5);

        Assert.NotEqual(weightsAt500, net.First.Values);
    }
}
