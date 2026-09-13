// CliffWorld.cs — the cliff walk from Sutton & Barto §6.5, as a state machine, made smaller.
//
// THIS IS C# BECAUSE IT IS NOT MATHEMATICS. A world is a set of rules about where you end up, and
// the learners -- which ARE mathematics -- live in `.hb`. Choosing an action from a learner's
// distribution is also here, because choosing is the host's job.
//
//     row 0   .  .  .  .  .  .  .
//     row 1   .  .  .  .  .  .  .
//     row 2   S  C  C  C  C  C  G
//
// Every step costs −1. Stepping onto C costs −100 and sends you back to S. Reaching G ends it.
//
// THREE ROWS BY SEVEN, NOT THE TEXTBOOK'S FOUR BY TWELVE, and the reason is measured: through the
// generated code one `Train` call costs about 9 ms and one `Act` about 3 ms, because each call
// interprets a graph (Prelude/Networks.hb says as much about host-driven fits). The textbook world
// over hundreds of episodes is minutes per learner. This one keeps what the lesson needs -- a safe
// row and a row beside the cliff -- and runs in seconds.
namespace MLambda.AI.Learning.Tests;

internal static class CliffWorld
{
    public const int Rows = 3;
    public const int Columns = 7;
    public const int States = Rows * Columns;
    public const int Actions = 4;
    public const int Start = (Rows - 1) * Columns;
    public const int Goal = (Rows * Columns) - 1;

    // up, right, down, left
    private static readonly (int Row, int Column)[] Moves = [(-1, 0), (0, 1), (1, 0), (0, -1)];

    public static int RowOf(int state) => state / Columns;

    public static int ColumnOf(int state) => state % Columns;

    public static bool IsCliff(int state) => RowOf(state) == Rows - 1 && ColumnOf(state) is > 0 and < Columns - 1;

    /// <summary>One step: where you land, what it cost, and whether the episode is over.</summary>
    public static (int Next, double Reward, bool Done) Step(int state, int action)
    {
        var row = Math.Clamp(RowOf(state) + Moves[action].Row, 0, Rows - 1);
        var column = Math.Clamp(ColumnOf(state) + Moves[action].Column, 0, Columns - 1);
        var next = (row * Columns) + column;

        if (IsCliff(next))
        {
            return (Start, -100d, false);
        }

        return (next, -1d, next == Goal);
    }

    /// <summary>Draw an action from a distribution over actions.</summary>
    public static int Draw(IReadOnlyList<double> chances, Random random)
    {
        var roll = random.NextDouble();
        var running = 0d;

        for (var action = 0; action < chances.Count; action++)
        {
            running += chances[action];

            if (roll < running)
            {
                return action;
            }
        }

        return chances.Count - 1;
    }

    /// <summary>The greedy path from Start, following the learned table, up to a step limit.</summary>
    public static List<int> GreedyPath(IReadOnlyList<double> q, int limit = 60)
    {
        var path = new List<int> { Start };
        var state = Start;

        for (var step = 0; step < limit && state != Goal; step++)
        {
            var best = 0;

            for (var action = 1; action < Actions; action++)
            {
                if (q[(state * Actions) + action] > q[(state * Actions) + best])
                {
                    best = action;
                }
            }

            (state, _, _) = Step(state, best);
            path.Add(state);
        }

        return path;
    }
}
