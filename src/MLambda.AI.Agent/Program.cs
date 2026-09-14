// Program.cs — runs one agent at several belief states and prints what it decided.
//
// THE PRINTING IS THE LESSON. Each sample shows the SAME agent at three or four different belief
// states, side by side, so a reader watches deliberation change with belief rather than being told
// it does.
//
// AND NOTHING HERE ACTS. `plan Warm ↦ Room.Heat` names a label; there is no `Room.Heat` in this
// repository and there does not need to be. The agent says "I intend Warm toward Comfortable", and
// deciding what that means is this file's job -- which is exactly the division the dialect draws.
using System.Reflection;
using MLambda.AI.Agent;
using MLambda.Hilbert.Proof;

using Therm = MLambda.AI.Agent.Thermostat;
using Coll = MLambda.AI.Agent.Collector;
using Clean = MLambda.AI.Agent.Cleaner;

if (args.Length == 0)
{
    Console.WriteLine("MLambda.AI.Agent — pass an agent name:");
    Console.WriteLine();
    Console.WriteLine("  Thermostat  one belief, one goal — the smallest agent there is");
    Console.WriteLine("  Collector   the whole cycle, and a commitment that finishes things");
    Console.WriteLine("  Cleaner     a plan library, shifting attention, and an honest failure");
    Console.WriteLine("  Agency      the theory underneath all three, and what is proved of it");
    return 0;
}

switch (args[0].ToLowerInvariant())
{
    case "thermostat": await Thermostat(); break;
    case "collector": await Collector(); break;
    case "cleaner": await Cleaner(); break;
    case "agency": Agency(); break;
    default:
        Console.Error.WriteLine($"There is no agent called '{args[0]}'. Run with no arguments for the list.");
        return 1;
}

return 0;

static async Task<string> Listed(IAsyncEnumerable<string> rows)
{
    var all = new List<string>();

    await foreach (var row in rows)
    {
        all.Add(row);
    }

    all.Sort(StringComparer.Ordinal);

    return all.Count == 0 ? "—" : string.Join(", ", all);
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

static void Verdicts(string title, Type proofs)
{
    Console.WriteLine();
    Console.WriteLine($"What the kernel proves of {title} as this runs, whatever any agent believes:");

    foreach (var verdict in Proved(proofs))
    {
        Console.WriteLine($"  {verdict.Status,-8} {verdict.Name}");
    }
}

static async Task Thermostat()
{
    Console.WriteLine("One thermostat, four temperatures. Its goal never changes; what it intends does.");
    Console.WriteLine();
    Console.WriteLine("  degrees   intends      goal met");
    Console.WriteLine("  ───────   ──────────   ────────");

    foreach (var degrees in new[] { 10, 18, 24, 30 })
    {
        var engine = Therm.ThermostatEngineFactory.Create();
        engine.AssertAll([
            new Therm.DesireFact("t", "Comfortable"),
            new Therm.AttentionFact("t", "Watching"),
            new Therm.TemperatureFact("t", degrees),
        ]);

        var plans = new List<string>();

        await foreach (var row in engine.Intentions("t"))
        {
            plans.Add(row.Plan);
        }

        var intends = plans.Count == 0 ? "—" : string.Join(", ", plans);

        Console.WriteLine($"  {degrees,7}   {intends,-10}   {await Listed(engine.Met("t"))}");
    }

    Console.WriteLine();
    Console.WriteLine("`commit open_minded` — its world will not hold still, so it reconsiders as");
    Console.WriteLine("beliefs change. An agent that blindly finished heating after somebody opened");
    Console.WriteLine("a window would be worse than a bimetallic strip.");
}

static async Task Collector()
{
    Console.WriteLine("One collector, counting to five. Watch the intention survive every step");
    Console.WriteLine("short of the goal — that is `commit single_minded`.");
    Console.WriteLine();
    Console.WriteLine("  gathered   intends      goal met     credit");
    Console.WriteLine("  ────────   ──────────   ──────────   ──────");

    foreach (var count in new[] { 0, 3, 4, 5 })
    {
        var engine = Coll.CollectorEngineFactory.Create();
        engine.AssertAll([
            new Coll.DesireFact("c", "Enough"),
            new Coll.AttentionFact("c", "Gathering"),
            new Coll.GatheredFact("c", count),
        ]);

        var plans = new List<string>();

        await foreach (var row in engine.Intentions("c"))
        {
            plans.Add(row.Plan);
        }

        var credit = new List<string>();

        await foreach (var row in engine.Credit("c"))
        {
            credit.Add($"{row.Plan} {row.Sign}");
        }

        var intends = plans.Count == 0 ? "—" : string.Join(", ", plans);
        var earned = credit.Count == 0 ? "—" : string.Join(", ", credit);

        Console.WriteLine($"  {count,8}   {intends,-10}   {await Listed(engine.Met("c")),-10}   {earned}");
    }

    Console.WriteLine();
    Console.WriteLine("`reward` is the seam to reinforcement learning: the same feedback that marks");
    Console.WriteLine("a plan good here is the signal a learner maximises in MLambda.AI.Learning.");
}

static async Task Cleaner()
{
    Console.WriteLine("Two cleaners, same goal, same rules. One has dirt it can reach; the other");
    Console.WriteLine("has dirt behind a locked door.");
    Console.WriteLine();

    var engine = Clean.CleanerEngineFactory.Create();
    engine.AssertAll([
        new Clean.DesireFact("open", "Spotless"),
        new Clean.AttentionFact("open", "Cleaning"),
        new Clean.DirtFact("open", "kitchen"),
        new Clean.ReachableFact("open", "kitchen"),

        new Clean.DesireFact("shut", "Spotless"),
        new Clean.AttentionFact("shut", "Cleaning"),
        new Clean.DirtFact("shut", "cellar"),
        new Clean.BlockedFact("shut", "cellar"),
    ]);

    foreach (var who in new[] { "open", "shut" })
    {
        var plans = new List<string>();

        await foreach (var row in engine.Intentions(who))
        {
            plans.Add(row.Plan);
        }

        plans.Sort(StringComparer.Ordinal);

        Console.WriteLine($"  cleaner '{who}'");
        Console.WriteLine($"    intends    {(plans.Count == 0 ? "—" : string.Join(", ", plans))}");
        Console.WriteLine($"    minding    {await Listed(engine.Looking(who))}");
        Console.WriteLine($"    abandoned  {await Listed(engine.Abandoned(who))}");
        Console.WriteLine();
    }

    Console.WriteLine("The second one stopped, and said so. The source says why in words:");
    Console.WriteLine("  because \"there is dirt in a room I cannot get to\"");
    Console.WriteLine();
    Console.WriteLine("That sentence is required by the compiler and does not reach the generated");
    Console.WriteLine("code — today it lives in Cleaner.ha alone. See docs/hilbert/06-diagnostics.md.");
}

static void Agency()
{
    Console.WriteLine("The theory all three agents extend. Belief, desire and intention are");
    Console.WriteLine("MODALITIES: `B(i) p` is not a field lookup, it is `p holds at every world i");
    Console.WriteLine("takes to be possible`.");
    Console.WriteLine();
    Console.WriteLine("Belief gets KD45 — it knows what it believes and what it does not.");
    Console.WriteLine("Desire and intention get KD: consistency, and no more.");

    Verdicts("Agency", typeof(AgencyProofs));
}
