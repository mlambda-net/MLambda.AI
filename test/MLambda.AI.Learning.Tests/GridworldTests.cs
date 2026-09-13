// GridworldTests.cs — Q-learning and Sarsa: the same world, one line apart.
//
// WHAT IS ASSERTED HERE WAS MEASURED FIRST, ACROSS FOUR SEEDS, and only what held on all four is a
// test. Over 120 episodes of the 3 × 7 cliff:
//
//   seed   Q-learning greedy path     Sarsa greedy path            return, last 20 (Q / Sarsa)
//   1      8 steps, beside the cliff  10 steps, via the far row    −29.5 / −11.2
//   2      8 steps, beside the cliff  10 steps, via the far row    −30.4 / −16.4
//   3      8 steps, beside the cliff  LOOPS, never reaches goal    −13.9 / −33.6
//   4      8 steps, beside the cliff  10 steps, via the far row    −29.9 / −26.4
//
// HOLDS ON ALL FOUR, AND IS TESTED: both learners improve; Q-learning's greedy path never leaves the
// row beside the cliff; Sarsa's greedy policy goes to the far row.
//
// DOES NOT HOLD ON ALL FOUR, AND IS NOT TESTED: that Sarsa earns more reward while learning. That is
// the textbook's claim, and it is an average over many runs -- on seed 3, after 120 episodes, it
// reverses. Asserting it from one lucky seed would be teaching something this project cannot show.
namespace MLambda.AI.Learning.Tests;

using MLambda.AI.Learning;
using MLambda.Hilbert.Runtime;

public class GridworldTests
{
    private const int Episodes = 120;
    private const int StepLimit = 60;
    private const double Epsilon = 0.1d;
    private const double Rate = 0.5d;

    // SEED 1 FOR THE EPISODE TESTS, and no claim below depends on it being seed 1: every episode-level
    // assertion in this file held on seeds 1 to 4. The seed is fixed so a failure is repeatable.
    private const int Seed = 1;

    private static Vector One(double value) => new double[] { value };

    /// <summary>Q-learning over the cliff, trained once and shared: about five seconds of steps.</summary>
    private static readonly Lazy<(QTable Learner, List<double> Returns)> QLearning = new(() =>
    {
        var learner = new QTable(CliffWorld.States, CliffWorld.Actions);
        var random = new System.Random(Seed);
        var returns = new List<double>();

        for (var episode = 0; episode < Episodes; episode++)
        {
            var state = CliffWorld.Start;
            var total = 0d;

            for (var step = 0; step < StepLimit; step++)
            {
                var action = CliffWorld.Draw(learner.Act(One(state), Epsilon).Values, random);
                var (next, reward, done) = CliffWorld.Step(state, action);

                learner.Train(One(state), One(action), One(reward), One(next), One(done ? 1 : 0), Rate, 1d);

                total += reward;
                state = next;

                if (done)
                {
                    break;
                }
            }

            returns.Add(total);
        }

        return (learner, returns);
    });

    /// <summary>Sarsa over the same cliff. The next action is DRAWN before the update, and used in it.</summary>
    private static readonly Lazy<(Sarsa Learner, List<double> Returns)> SarsaLearning = new(() =>
    {
        var learner = new Sarsa(CliffWorld.States, CliffWorld.Actions);
        var random = new System.Random(Seed);
        var returns = new List<double>();

        for (var episode = 0; episode < Episodes; episode++)
        {
            var state = CliffWorld.Start;
            var action = CliffWorld.Draw(learner.Act(One(state), Epsilon).Values, random);
            var total = 0d;

            for (var step = 0; step < StepLimit; step++)
            {
                var (next, reward, done) = CliffWorld.Step(state, action);
                var nextAction = CliffWorld.Draw(learner.Act(One(next), Epsilon).Values, random);

                learner.Train(One(state), One(action), One(reward), One(next), One(nextAction), One(done ? 1 : 0), Rate, 1d);

                total += reward;
                state = next;
                action = nextAction;

                if (done)
                {
                    break;
                }
            }

            returns.Add(total);
        }

        return (learner, returns);
    });

    // ── the one line, tested without a single episode ─────────────────────────────────────────

    [Fact]
    public void A_single_step_moves_the_entry_it_should_and_no_other()
    {
        // THE TIGHTEST TEST OF A TD UPDATE. From an all-zero table, one step from state 14 by
        // action 0, costing −1, into state 7. The target is −1 + 1 · max(q[7]) = −1, and at rate
        // 0.5 the entry moves halfway: to −0.5. Every other entry of the 84 stays exactly zero.
        var learner = new QTable(CliffWorld.States, CliffWorld.Actions);

        learner.Train(One(14), One(0), One(-1), One(7), One(0), Rate, 1d);

        var q = learner.Q.Values;

        Assert.Equal(-0.5d, q[(14 * CliffWorld.Actions) + 0], 9);
        Assert.Equal(1, q.Count(value => value != 0d));
    }

    [Fact]
    public void Off_policy_aims_at_the_best_next_action_and_on_policy_at_the_one_taken()
    {
        // THE WHOLE DIFFERENCE BETWEEN THE TWO FILES, as one number each.
        //
        // First teach both tables that state 5, action 1 is worth 10 (rate 1, terminal, so the entry
        // becomes exactly 10). Then step from state 0 into state 5 -- and tell Sarsa the next action
        // taken there was 0, which is worth nothing.
        //
        //   Q-learning aims at  0 + max(q[5]) = 10   → q[0,0] moves halfway, to 5
        //   Sarsa aims at       0 + q[5, 0]   = 0    → q[0,0] stays at 0
        var offPolicy = new QTable(CliffWorld.States, CliffWorld.Actions);
        var onPolicy = new Sarsa(CliffWorld.States, CliffWorld.Actions);

        offPolicy.Train(One(5), One(1), One(10), One(5), One(1), 1d, 1d);
        onPolicy.Train(One(5), One(1), One(10), One(5), One(1), One(1), 1d, 1d);

        offPolicy.Train(One(0), One(0), One(0), One(5), One(0), Rate, 1d);
        onPolicy.Train(One(0), One(0), One(0), One(5), One(0), One(0), Rate, 1d);

        Assert.Equal(5d, offPolicy.Q.Values[0], 9);
        Assert.Equal(0d, onPolicy.Q.Values[0], 9);
    }

    // ── over the whole cliff: only what held on all four seeds ─────────────────────────────────

    [Fact]
    public void Q_learning_collects_more_reward_as_it_learns()
    {
        // NOT "it reaches a number" -- that pins the fixture. The last 20 episodes beat the first 20.
        var returns = QLearning.Value.Returns;

        Assert.True(
            returns.TakeLast(20).Average() > returns.Take(20).Average(),
            $"first 20 averaged {returns.Take(20).Average():F1}, last 20 {returns.TakeLast(20).Average():F1}");
    }

    [Fact]
    public void Sarsa_collects_more_reward_as_it_learns()
    {
        var returns = SarsaLearning.Value.Returns;

        Assert.True(
            returns.TakeLast(20).Average() > returns.Take(20).Average(),
            $"first 20 averaged {returns.Take(20).Average():F1}, last 20 {returns.TakeLast(20).Average():F1}");
    }

    [Fact]
    public void Q_learning_walks_the_optimal_path_along_the_edge()
    {
        // OFF-POLICY LEARNS THE GREEDY POLICY'S VALUE, so its greedy path is the shortest one: up, six
        // steps right along the row beside the cliff, and down -- eight moves. It never goes to the far
        // row, because that is two steps longer and Q-learning does not count the cost of its own
        // exploration.
        var path = CliffWorld.GreedyPath(QLearning.Value.Learner.Q.Values);

        Assert.Equal(CliffWorld.Goal, path[^1]);
        Assert.Equal(8, path.Count - 1);
        Assert.DoesNotContain(path, state => CliffWorld.RowOf(state) == 0);
    }

    [Fact]
    public void Sarsa_steps_back_from_the_edge()
    {
        // ON-POLICY LEARNS THE VALUE OF WHAT IT ACTUALLY DOES -- exploration included. Walking beside
        // the cliff with ε = 0.1 means an occasional step into it at −100, and Sarsa's table has
        // counted that. So its greedy policy goes to the far row, which Q-learning's never does.
        //
        // NON-VACUOUS: the test above shows the far row is NOT where a greedy path goes by default.
        var path = CliffWorld.GreedyPath(SarsaLearning.Value.Learner.Q.Values);

        Assert.Contains(path, state => CliffWorld.RowOf(state) == 0);
    }
}
