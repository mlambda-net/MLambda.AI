// Program.cs — runs one Minds sample and prints what happened.
//
// A SWITCH, NOT A FRAMEWORK, exactly as in MLambda.AI.Logic. Every claim printed is Hilbert's: the
// theories say what follows, the engines work it out, and the kernel proves the theorems as this runs.
using System.Reflection;
using MLambda.AI.Minds;
using MLambda.Hilbert.Proof;
using MLambda.Shin.Runtime;

using Light = MLambda.AI.Minds.Traffic;

if (args.Length == 0)
{
    Console.WriteLine("MLambda.AI.Minds — pass a sample name:");
    Console.WriteLine();
    Console.WriteLine("  Traffic    time: now, next, eventually, always, until");
    Console.WriteLine("  Duty       what ought to be, and why an obligation does not make itself true");
    Console.WriteLine("  Knowledge  knowing is not believing: S5 against KD45");
    Console.WriteLine("  Mind       belief, desire, intention and knowledge in one agent");
    Console.WriteLine("  Deadlines  duties over time, and who knew");
    return 0;
}

switch (args[0].ToLowerInvariant())
{
    case "traffic": await Traffic(); break;
    default:
        Console.Error.WriteLine($"There is no sample called '{args[0]}'. Run with no arguments for the list.");
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
    return all.Count == 0 ? "(nothing)" : string.Join(", ", all);
}

// EACH THEOREM IS A METHOD, AND CALLING IT IS THE PROOF. The generated `<File>Proofs` class carries
// no verdict: calling a method reads the `.hp` embedded in this assembly, runs its proof script, and
// has the kernel replay the term — here and now. The methods are found rather than listed, so a
// theorem added to a file is printed without anyone remembering to add it here.
static IEnumerable<Judged> Proved(Type proofs) =>
    proofs.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
        .Where(m => m.ReturnType == typeof(Judged) && m.GetParameters().Length == 0)
        .OrderBy(m => m.MetadataToken)
        .Select(m => (Judged)m.Invoke(null, null)!);

static void Verdicts(string title, Type proofs)
{
    Console.WriteLine();
    Console.WriteLine($"And the theorems ({title}) -- proved by the kernel as this runs:");

    foreach (var verdict in Proved(proofs))
    {
        Console.WriteLine($"  {verdict.Status,-8} {verdict.Name}");
    }
}

static async Task Traffic()
{
    string[] colours = ["red", "red", "green", "green", "amber", "red"];
    Console.WriteLine($"A light's run, m0 to m5: {string.Join(", ", colours)}");
    Console.WriteLine();

    var light = Light.TrafficEngineFactory.Create();
    light.AssertAll([
        new Light.ColourFact("red"), new Light.ColourFact("green"), new Light.ColourFact("amber"),
        .. colours.Select((_, i) => (ShinFact)new Light.MomentFact($"m{i}")),
        .. colours.Skip(1).Select((_, i) => (ShinFact)new Light.NextFact($"m{i}", $"m{i + 1}")),
        .. colours.Select((c, i) => (ShinFact)new Light.ShowsFact($"m{i}", c)),
    ]);

    Console.WriteLine($"  F  from m0, eventually shows:  {await Listed(light.EventuallyShows("m0"))}");
    Console.WriteLine($"  F  from m4, eventually shows:  {await Listed(light.EventuallyShows("m4"))}");
    Console.WriteLine($"  G  from m0, always shows:      {await Listed(light.AlwaysShows("m0"))}");
    Console.WriteLine($"  G  from m5, always shows:      {await Listed(light.AlwaysShows("m5"))}");
    Console.WriteLine($"  U  from m0, red until green:   {await light.WaitsUntil("m0", "red", "green")}");
    Console.WriteLine($"  U  from m2, green until red:   {await light.WaitsUntil("m2", "green", "red")}");

    Verdicts("Traffic", typeof(TrafficProofs));
}
