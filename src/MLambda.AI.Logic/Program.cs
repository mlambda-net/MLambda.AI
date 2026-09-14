// Program.cs — runs one sample and prints what happened.
//
// A SWITCH, NOT A FRAMEWORK. Each arm asserts some facts, asks the engine a question, and prints
// both the answer and the verdicts the build already checked. There is no registry and no lesson
// interface: which samples exist is visible in one place, and which level each belongs to is
// decided by the documentation, not by anything in this file.
//
// EVERY CLAIM PRINTED BELOW IS HILBERT'S. Nothing here decides that fluffy is an animal or that
// dave is alice's descendant -- the theories say what follows and the engines work it out.
using System.Reflection;
using MLambda.AI.Logic;
using MLambda.Hilbert.Proof;

using Zoo = MLambda.AI.Logic.Animals;
using Kin = MLambda.AI.Logic.Families;
using Claims = MLambda.AI.Logic.Chains;
using Frames = MLambda.AI.Logic.Worlds;
using Kinds = MLambda.AI.Logic.Sorts;

if (args.Length == 0)
{
    Console.WriteLine("MLambda.AI.Logic — pass a sample name:");
    Console.WriteLine();
    Console.WriteLine("  Animals   a thing belongs to the kinds its kinds belong to");
    Console.WriteLine("  Families  ancestors, and why two children of one parent are siblings");
    Console.WriteLine("  Chains    modus ponens, chained to the end");
    Console.WriteLine("  Worlds    the frame conditions, and which modal logic you chose");
    Console.WriteLine("  Counting  arithmetic decided by a checked certificate");
    Console.WriteLine("  Sorts     classification that individuates");
    return 0;
}

switch (args[0].ToLowerInvariant())
{
    case "animals": await Animals(); break;
    case "families": await Families(); break;
    case "chains": await Chains(); break;
    case "worlds": await Worlds(); break;
    case "counting": Counting(); break;
    case "sorts": await Sorts(); break;
    default:
        Console.Error.WriteLine($"There is no sample called '{args[0]}'. Run with no arguments for the list.");
        return 1;
}

return 0;

static async Task Answer(string question, IAsyncEnumerable<string> rows)
{
    var found = new List<string>();

    await foreach (var row in rows)
    {
        found.Add(row);
    }

    found.Sort(StringComparer.Ordinal);

    Console.WriteLine($"  {question,-42} {(found.Count == 0 ? "(nothing)" : string.Join(", ", found))}");
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
    Console.WriteLine($"And the theorems ({title}) -- proved by the kernel as this runs:");

    foreach (var verdict in Proved(proofs))
    {
        Console.WriteLine($"  {verdict.Status,-8} {verdict.Name}");
    }
}

static async Task Animals()
{
    Console.WriteLine("Told the engine that a cat and a bird are kinds of animal,");
    Console.WriteLine("and that fluffy is known to be a cat.");
    Console.WriteLine();

    var zoo = Zoo.AnimalEngineFactory.Create();
    zoo.AssertAll([
        new Zoo.ThingFact("fluffy"), new Zoo.ThingFact("cat"),
        new Zoo.ThingFact("bird"), new Zoo.ThingFact("animal"),
        new Zoo.KindOfFact("cat", "animal"),
        new Zoo.KindOfFact("bird", "animal"),
        new Zoo.KnownFact("fluffy", "cat"),
    ]);

    await Answer("What is fluffy?", zoo.Kinds("fluffy"));
    Console.WriteLine("  'animal' is in that list and nobody put it there.");

    Verdicts("Animals", typeof(AnimalsProofs));
}

static async Task Families()
{
    Console.WriteLine("The family:  alice ─┬─ bob ── dave");
    Console.WriteLine("                    └─ carol");
    Console.WriteLine();
    Console.WriteLine("Three `parent` facts were asserted. Nothing else.");
    Console.WriteLine();

    var family = Kin.FamilyEngineFactory.Create();
    family.AssertAll([
        new Kin.PersonFact("alice"), 
        new Kin.PersonFact("bob"),
        new Kin.PersonFact("carol"), 
        new Kin.PersonFact("dave"),
        new Kin.ParentFact("alice", "bob"),
        new Kin.ParentFact("alice", "carol"),
        new Kin.ParentFact("bob", "dave"),
    ]);

    await Answer("Who descends from alice?", family.Ancestors("alice"));
    Console.WriteLine("  dave is her grandchild, reached by a fixpoint rather than a rule about grandchildren.");
    await Answer("Who are bob's siblings?", family.SiblingsOf("bob"));
    Console.WriteLine("  carol, and NOT bob himself — the `x ≠ y` guard is the whole of that.");
    Console.WriteLine("  Without it the rule matches bob twice as a child of alice.");
    await Answer("And dave's?", family.SiblingsOf("dave"));

    Verdicts("Families", typeof(FamiliesProofs));
}

static async Task Chains()
{
    Console.WriteLine("Four claims, and only one of them asserted true:");
    Console.WriteLine("  raining → wet_ground → slippery → be_careful");
    Console.WriteLine();

    var weather = Claims.ChainEngineFactory.Create();
    weather.AssertAll([
        new Claims.HoldsFact("raining"),
        new Claims.SaysFact("raining", "wet_ground"),
        new Claims.SaysFact("wet_ground", "slippery"),
        new Claims.SaysFact("slippery", "be_careful"),
    ]);

    await Answer("What is true?", weather.Truths());
    Console.WriteLine("  One was asserted. The other three followed, from a rule that never says");
    Console.WriteLine("  how long a chain may be.");

    Verdicts("Chains", typeof(ChainsProofs));
}

static async Task Worlds()
{
    Console.WriteLine("A `world` is a way things could be, and `sees(w, u)` says u is possible from w.");
    Console.WriteLine("Asserted: here is a place, and a sees b, and b sees c.");
    Console.WriteLine();

    var frame = Frames.WorldEngineFactory.Create();
    frame.AssertAll([
        new Frames.PlaceFact("here"),
        new Frames.SeesFact("a", "b"),
        new Frames.SeesFact("b", "c"),
    ]);

    await Answer("What does 'here' see?", frame.SeenFrom("here"));
    Console.WriteLine("  Itself, by reflexivity — which holds of every world the host DECLARED.");
    await Answer("What does 'a' see?", frame.SeenFrom("a"));
    Console.WriteLine("  c by transitivity — and a itself, which is worth a second look. Nobody");
    Console.WriteLine("  declared 'a' a place, so reflexivity never touched it. Symmetry turned");
    Console.WriteLine("  a→b into b→a, and transitivity composed the two. THIS IS WHY THE CHOICE");
    Console.WriteLine("  OF FRAME CONDITIONS IS THE CHOICE OF LOGIC: open a different set and");
    Console.WriteLine("  this answer changes.");

    Verdicts("Worlds", typeof(WorldsProofs));
}

static void Counting()
{
    Console.WriteLine("There is no engine in this sample, and that is the point.");
    Console.WriteLine("Every claim below was decided by a certificate the kernel checked, with");
    Console.WriteLine("`axioms []` — nothing assumed, so nothing here to take on trust.");

    Verdicts("Counting", typeof(CountingProofs));
}

static async Task Sorts()
{
    Console.WriteLine("A sortal classifies AND individuates. Asserted: tabby ⊑ cat ⊑ mammal,");
    Console.WriteLine("cat and bird are disjoint, and fluffy is a tabby.");
    Console.WriteLine();

    var kinds = Kinds.SortEngineFactory.Create();
    kinds.AssertAll([
        new Kinds.KindFact("tabby"), new Kinds.KindFact("cat"),
        new Kinds.KindFact("mammal"), new Kinds.KindFact("bird"),
        new Kinds.SubFact("tabby", "cat"),
        new Kinds.SubFact("cat", "mammal"),
        new Kinds.DisjointFact("cat", "bird"),
        new Kinds.InstFact("fluffy", "tabby"),
    ]);

    await Answer("What sorts is fluffy an instance of?", kinds.SortsOf("fluffy"));
    await Answer("Any contradictions?", kinds.Impossible());

    Console.WriteLine();
    Console.WriteLine("And the point of the whole subject. Every person is a passenger;");
    Console.WriteLine("alice and bob are the same PASSENGER:");
    Console.WriteLine();

    var travel = Kinds.SortEngineFactory.Create();
    travel.AssertAll([
        new Kinds.KindFact("person"), new Kinds.KindFact("passenger"),
        new Kinds.SubFact("person", "passenger"),
        new Kinds.SameFact("alice", "bob", "passenger"),
    ]);

    await Answer("Under which sorts are they the same?", travel.SortsSharing("alice", "bob"));
    Console.WriteLine("  'person' is NOT there. Sameness climbs and does not descend — two people");
    Console.WriteLine("  can share a seat reservation. No law proves otherwise, on purpose.");

    Verdicts("Sorts", typeof(SortsProofs));
}
