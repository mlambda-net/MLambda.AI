// BanditTests.cs — explore versus exploit, with the machinery taken away.
//
// THREE ARMS THAT PAY THE SAME EVERY TIME: 0.1, 0.5 and 0.9. Deterministic payouts are a fixture
// choice, not a claim about bandits -- they make every expected estimate checkable by hand, so a
// failure means the learner and not the dice.
namespace MLambda.AI.Learning.Tests;

using MLambda.AI.Learning;
using MLambda.Hilbert.Runtime;

public class BanditTests
{
    private static readonly double[] Payouts = [0.1d, 0.5d, 0.9d];

    /// <summary>Pull one arm some number of times, each paying its fixed amount.</summary>
    private static void Pull(Bandit bandit, int arm, int times)
    {
        var arms = Enumerable.Repeat((double)arm, times).ToArray();
        var rewards = Enumerable.Repeat(Payouts[arm], times).ToArray();

        bandit.Train((Vector)arms, (Vector)rewards);
    }

    [Fact]
    public void An_untried_arm_looks_best_when_optimism_is_high()
    {
        // OPTIMISM NEEDS NO ε. With `start` above anything the arms pay, every arm the learner has
        // not tried outranks every arm it has -- so it will go and try them. Exploration, and not
        // a coin flipped anywhere.
        var bandit = new Bandit(3, 10d);
        Pull(bandit, arm: 1, times: 1);

        var means = bandit.Means().Values;

        Assert.Equal(10d, means[0]);
        Assert.Equal(0.5d, means[1], 9);
        Assert.Equal(10d, means[2]);
    }

    [Fact]
    public void A_pulled_arm_is_estimated_at_what_it_pays()
    {
        // `total / counts`, visible as two weights: the estimate IS the average, and nothing else.
        var bandit = new Bandit(3, 0d);
        Pull(bandit, arm: 2, times: 20);

        Assert.Equal(0.9d, bandit.Means().Values[2], 9);
        Assert.Equal(20d, bandit.Counts.Values[2]);
    }

    [Fact]
    public void Choosing_answers_a_distribution_not_an_arm()
    {
        // EXPLORATION IS ARITHMETIC, NEVER A BRANCH. `choose` hands back how likely each arm is;
        // drawing one is the host's job.
        var bandit = new Bandit(3, 0d);
        Pull(bandit, 0, 5);
        Pull(bandit, 1, 5);
        Pull(bandit, 2, 5);

        var chances = bandit.Choose(0.3d).Values;

        Assert.All(chances, chance => Assert.InRange(chance, 0d, 1d));
        Assert.Equal(1d, chances.Sum(), 9);
    }

    [Fact]
    public void The_greedy_arm_is_chosen_with_one_minus_epsilon_plus_epsilon_over_A()
    {
        // THE TERM EVERY BRANCHING IMPLEMENTATION FORGETS. ε-greedy explores with probability ε,
        // and exploration picks uniformly -- which INCLUDES the greedy arm. So the greedy arm's real
        // chance is 1 − ε + ε/A, not 1 − ε.
        //
        // With ε = 0.3 and three arms: 0.7 + 0.1 = 0.8 for the best arm, and 0.1 for each other.
        // An implementation that wrote `if (random < ε) explore() else exploit()` and then reported
        // 1 − ε would be off by exactly ε/A, and this is the test that notices.
        var bandit = new Bandit(3, 0d);
        Pull(bandit, 0, 5);
        Pull(bandit, 1, 5);
        Pull(bandit, 2, 5);

        var chances = bandit.Choose(0.3d).Values;

        Assert.Equal(0.1d, chances[0], 9);
        Assert.Equal(0.1d, chances[1], 9);
        Assert.Equal(0.8d, chances[2], 9);
    }

    [Fact]
    public void With_no_exploration_the_best_arm_gets_everything()
    {
        // NON-VACUOUS: the test above could pass for a policy that ignored ε. At ε = 0 the whole
        // distribution sits on arm 2.
        var bandit = new Bandit(3, 0d);
        Pull(bandit, 0, 5);
        Pull(bandit, 1, 5);
        Pull(bandit, 2, 5);

        Assert.Equal([0d, 0d, 1d], bandit.Choose(0d).Values.Select(v => Math.Round(v, 9)));
    }

    [Fact]
    public void Ties_split_rather_than_being_broken_arbitrarily()
    {
        // GREEDY IS A MASK, SO TIES SPLIT. A fresh bandit believes every arm is worth `start`, so
        // three arms tie, and the greedy policy spreads a third over each. The alternative -- pick
        // one and call it the policy -- would be a decision nobody made.
        var fresh = new Bandit(3, 5d);

        Assert.All(fresh.Choose(0d).Values, chance => Assert.Equal(1d / 3d, chance, 9));
    }

    [Fact]
    public void The_estimates_rank_the_arms_correctly()
    {
        // NOT "it always pulls the best arm". An ε-greedy learner deliberately does not, and a test
        // asserting it would be asserting that exploration is broken. The honest claim is about
        // what the learner BELIEVES.
        var bandit = new Bandit(3, 0d);
        Pull(bandit, 0, 10);
        Pull(bandit, 1, 10);
        Pull(bandit, 2, 10);

        var means = bandit.Means().Values;

        Assert.True(means[2] > means[1] && means[1] > means[0], $"means were {string.Join(", ", means)}");
    }
}
