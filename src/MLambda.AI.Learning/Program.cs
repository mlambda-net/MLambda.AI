// Program.cs — runs a learner and prints what it learned.
//
// IF A LINE HERE LOOKS LIKE MATHEMATICS, IT IS IN THE WRONG FILE. The learners are `.hb` and the
// algebra they rely on is proved in `Identities.hp`. What IS here is the host's work: a world to act
// in, and drawing an action from the distribution a learner hands back.
//
// THE CLIFF IS A COPY of test/MLambda.AI.Learning.Tests/CliffWorld.cs, because there is no shared
// project to hold it and forty lines of state machine are cheaper than one.
using System.Reflection;
using MLambda.AI.Learning;
using MLambda.Hilbert.Proof;
using MLambda.Hilbert.Runtime;

if (args.Length == 0)
{
    Console.WriteLine("MLambda.AI.Learning — pass a sample name:");
    Console.WriteLine();
    Console.WriteLine("  Bandit      explore versus exploit, with the machinery taken away");
    Console.WriteLine("  Cliff       Q-learning and Sarsa on the same cliff, one line apart");
    Console.WriteLine("  Identities  the algebra those learners rely on, proved while you watch");
    return 0;
}

switch (args[0].ToLowerInvariant())
{
    case "bandit": Bandit(); break;
    case "cliff": Cliff(); break;
    case "identities": Identities(); break;
    default:
        Console.Error.WriteLine($"There is no sample called '{args[0]}'. Run with no arguments for the list.");
        return 1;
}

return 0;

static Vector One(double value) => new double[] { value };

static void Bandit()
{
    Console.WriteLine("Three arms that pay 0.1, 0.5 and 0.9. Every arm starts believed to be worth 10,");
    Console.WriteLine("which is more than any of them pays — so every untried arm looks best until tried.");
    Console.WriteLine();

    double[] payouts = [0.1, 0.5, 0.9];
    var bandit = new MLambda.AI.Learning.Bandit(3, 10d);

    void Show(string moment)
    {
        var means = bandit.Means().Values;
        var chances = bandit.Choose(0.3d).Values;

        Console.WriteLine($"  {moment,-26} believed {means[0],6:F2} {means[1],6:F2} {means[2],6:F2}" +
                          $"    chance at ε=0.3  {chances[0]:F3} {chances[1]:F3} {chances[2]:F3}");
    }

    Show("nothing tried yet");

    for (var arm = 0; arm < 3; arm++)
    {
        bandit.Train((Vector)Enumerable.Repeat((double)arm, 5).ToArray(), (Vector)Enumerable.Repeat(payouts[arm], 5).ToArray());
        Show($"after five pulls of arm {arm}");
    }

    Console.WriteLine();
    Console.WriteLine("Nothing tried: the three tie, and a tie SPLITS rather than being broken arbitrarily.");
    Console.WriteLine("At the end, the best arm is chosen with 0.800 — not 0.700. ε-greedy explores with");
    Console.WriteLine("chance 0.3, and exploring picks uniformly, which includes the best arm: 1 − ε + ε/A.");
}

static void Cliff()
{
    const int Rows = 3, Columns = 7, States = Rows * Columns, Actions = 4, Start = 14, Goal = 20;
    (int, int)[] moves = [(-1, 0), (0, 1), (1, 0), (0, -1)];

    (int Next, double Reward, bool Done) Step(int state, int action)
    {
        var row = Math.Clamp((state / Columns) + moves[action].Item1, 0, Rows - 1);
        var column = Math.Clamp((state % Columns) + moves[action].Item2, 0, Columns - 1);
        var next = (row * Columns) + column;

        return row == Rows - 1 && column is > 0 and < Columns - 1 ? (Start, -100d, false) : (next, -1d, next == Goal);
    }

    int Draw(IReadOnlyList<double> chances, System.Random random)
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

    HashSet<int> GreedyPath(IReadOnlyList<double> q)
    {
        var path = new HashSet<int> { Start };
        var state = Start;

        for (var step = 0; step < 40 && state != Goal; step++)
        {
            var best = Enumerable.Range(0, Actions).MaxBy(action => q[(state * Actions) + action]);
            (state, _, _) = Step(state, best);
            path.Add(state);
        }

        return path;
    }

    Console.WriteLine("Training Q-learning and Sarsa for 120 episodes each on the same cliff (about ten seconds —");
    Console.WriteLine("every step goes through the generated code, and each call interprets a graph)...");
    Console.WriteLine();

    var offPolicy = new QTable(States, Actions);
    var onPolicy = new Sarsa(States, Actions);
    var qRandom = new System.Random(1);
    var sRandom = new System.Random(1);

    for (var episode = 0; episode < 120; episode++)
    {
        var state = Start;

        for (var step = 0; step < 60; step++)
        {
            var action = Draw(offPolicy.Act(One(state), 0.1).Values, qRandom);
            var (next, reward, done) = Step(state, action);
            offPolicy.Train(One(state), One(action), One(reward), One(next), One(done ? 1 : 0), 0.5, 1d);
            state = next;

            if (done)
            {
                break;
            }
        }

        state = Start;
        var chosen = Draw(onPolicy.Act(One(state), 0.1).Values, sRandom);

        for (var step = 0; step < 60; step++)
        {
            var (next, reward, done) = Step(state, chosen);
            var then = Draw(onPolicy.Act(One(next), 0.1).Values, sRandom);
            onPolicy.Train(One(state), One(chosen), One(reward), One(next), One(then), One(done ? 1 : 0), 0.5, 1d);
            state = next;
            chosen = then;

            if (done)
            {
                break;
            }
        }
    }

    var qPath = GreedyPath(offPolicy.Q.Values);
    var sPath = GreedyPath(onPolicy.Q.Values);

    Console.WriteLine("  Q-learning                      Sarsa");

    for (var row = 0; row < Rows; row++)
    {
        string Draw1(HashSet<int> path) => string.Join(" ", Enumerable.Range(0, Columns).Select(column =>
        {
            var state = (row * Columns) + column;
            return state == Start ? "S" : state == Goal ? "G" : row == Rows - 1 ? "C" : path.Contains(state) ? "*" : ".";
        }));

        Console.WriteLine($"  {Draw1(qPath)}                   {Draw1(sPath)}");
    }

    Console.WriteLine();
    Console.WriteLine("Q-learning walks the row beside the cliff: the shortest path, and the value of a");
    Console.WriteLine("greedy policy it is not actually following. Sarsa learned the value of what it DOES,");
    Console.WriteLine("exploration included — an occasional slip off that row costs 100 — so it goes round.");
    Console.WriteLine();
    Console.WriteLine("One line of source separates them: qTarget takes the best next action, sarsaTarget");
    Console.WriteLine("the one actually taken. (Seed 1. On seed 3 Sarsa has not settled after 120 episodes —");
    Console.WriteLine("see the header of GridworldTests.cs for what holds on every seed and what does not.)");
}

// EACH THEOREM IS A METHOD, AND CALLING IT IS THE PROOF. The generated `<File>Proofs` class carries
// no verdict: `AnimalsProofs.ACatIsAnAnimal()` reads the `.hp` embedded in this assembly, runs its
// proof script, and has the kernel replay the term -- here and now, not read back from the build.
// The methods are found rather than listed, so a theorem added to the file is printed without anyone
// remembering to add it here.
static IEnumerable<Judged> Proved(Type proofs) =>
    proofs.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
        .Where(m => m.ReturnType == typeof(Judged) && m.GetParameters().Length == 0)
        .OrderBy(m => m.MetadataToken)
        .Select(m => (Judged)m.Invoke(null, null)!);

static void Identities()
{
    Console.WriteLine("The algebra the learners rely on, each theorem proved by the kernel NOW rather than");
    Console.WriteLine("read back from the build — with `axioms []`, so nothing below is assumed.");
    Console.WriteLine();

    foreach (var verdict in Proved(typeof(IdentitiesProofs)))
    {
        Console.WriteLine($"  {verdict.Status,-8} {verdict.Name}");
    }

    Console.WriteLine();
    Console.WriteLine("Not here, on purpose: value iteration's convergence (a limit), Robbins–Monro, the");
    Console.WriteLine("geometric sum at every n, and a step never overshooting at a SYMBOLIC step size —");
    Console.WriteLine("that one needs a product of hypotheses, which a linear certificate cannot supply.");
}
