// Program.cs — runs one Minds sample and prints what happened.
//
// A SWITCH, NOT A FRAMEWORK, exactly as in MLambda.AI.Logic. Every claim printed is Hilbert's: the
// theories say what follows, the engines work it out, and the kernel proves the theorems as this runs.
using System.Reflection;
using MLambda.AI.Minds;
using MLambda.Hilbert.Proof;
using MLambda.Shin.Runtime;

using Light = MLambda.AI.Minds.Traffic;
using Rules = MLambda.AI.Minds.Duty;
using Door = MLambda.AI.Minds.Knowledge;
using Kitchen = MLambda.AI.Minds.Mind;

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
    case "duty": await Duty(); break;
    case "knowledge": await Knowledge(); break;
    case "mind": await Mind(); break;
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

static async Task Duty()
{
    Console.WriteLine("An office: ana is the keyholder and staff, ben is staff.");
    Console.WriteLine("Keyholders must lock up. Staff may not smoke.");
    Console.WriteLine();

    var office = Rules.DutyEngineFactory.Create();
    office.AssertAll([
        new Rules.PersonFact("ana"), new Rules.PersonFact("ben"),
        new Rules.ActFact("lock_up"), new Rules.ActFact("smoke"), new Rules.ActFact("sweep"),
        new Rules.PlaysFact("ana", "keyholder"), new Rules.PlaysFact("ana", "staff"), new Rules.PlaysFact("ben", "staff"),
        new Rules.ObligesFact("keyholder", "lock_up"),
        new Rules.ForbidsFact("staff", "smoke"),
    ]);

    Console.WriteLine($"  ana must:      {await Listed(office.Obligations("ana"))}");
    Console.WriteLine($"  ben may:       {await Listed(office.Permissions("ben"))}");

    var unmet = new List<string>();
    await foreach (var row in office.Violations())
    {
        unmet.Add($"{row.Who} has not done {row.What}");
    }

    Console.WriteLine($"  unmet:         {(unmet.Count == 0 ? "(nothing)" : string.Join("; ", unmet))}");
    Console.WriteLine("  An obligation says what ought to be. It does not make it so.");

    Verdicts("Duty", typeof(DutyProofs));
}

static async Task Knowledge()
{
    Console.WriteLine("At home the door is unlocked. alice glanced and settled on 'locked';");
    Console.WriteLine("bob tried the handle.");
    Console.WriteLine();

    var door = Door.KnowledgeEngineFactory.Create();
    door.AssertAll([
        new Door.WorldFact("home"), new Door.WorldFact("locked_home"),
        new Door.AgentFact("alice"), new Door.AgentFact("bob"),
        new Door.PropFact("locked"), new Door.PropFact("unlocked"),
        new Door.HoldsFact("home", "unlocked"), new Door.HoldsFact("locked_home", "locked"),
        new Door.GlimpseFact("alice", "home", "locked_home"),
        new Door.GuessFact("alice", "home", "locked_home"),
        new Door.GuessFact("bob", "home", "home"),
    ]);

    Console.WriteLine("  agent   believes    knows       mistaken about");
    foreach (var who in new[] { "alice", "bob" })
    {
        Console.WriteLine($"  {who,-6}  {await Listed(door.Believed(who, "home")),-10}  {await Listed(door.Known(who, "home")),-10}  {await Listed(door.Mistaken(who, "home"))}");
    }

    Console.WriteLine();
    Console.WriteLine("  alice believes the door is locked and does not know it: what is known is true,");
    Console.WriteLine("  and it is not. Belief is KD45 and has no law that makes it true; knowledge is S5.");

    Verdicts("Knowledge", typeof(KnowledgeProofs));
}

static async Task Mind()
{
    Console.WriteLine("A kitchen robot. The kettle is on; the cup is dirty, but the robot could not see it.");
    Console.WriteLine("It wants tea and has planned for a clean cup.");
    Console.WriteLine();

    var robot = Kitchen.MindEngineFactory.Create();
    robot.AssertAll([
        new Kitchen.AgentFact("robot"),
        new Kitchen.WorldFact("now"), new Kitchen.WorldFact("clean_cup"), new Kitchen.WorldFact("tea"),
        new Kitchen.PropFact("kettle_on"), new Kitchen.PropFact("cup_clean"), new Kitchen.PropFact("tea_made"),
        new Kitchen.HoldsFact("now", "kettle_on"),
        new Kitchen.HoldsFact("clean_cup", "kettle_on"), new Kitchen.HoldsFact("clean_cup", "cup_clean"),
        new Kitchen.HoldsFact("tea", "kettle_on"), new Kitchen.HoldsFact("tea", "cup_clean"), new Kitchen.HoldsFact("tea", "tea_made"),
        new Kitchen.GlimpseFact("robot", "now", "clean_cup"),
        new Kitchen.GuessFact("robot", "now", "clean_cup"),
        new Kitchen.WishFact("robot", "now", "tea"),
        new Kitchen.PlanFact("robot", "now", "clean_cup"),
    ]);

    var known = await Listed(robot.Known("robot", "now"));
    var believed = await Listed(robot.Believed("robot", "now"));
    var desired = await Listed(robot.Desired("robot", "now"));
    var intended = await Listed(robot.Intended("robot", "now"));

    static string Mark(string list, string p) => list.Split(", ").Contains(p) ? "yes" : "-";

    Console.WriteLine("  proposition   knows  believes  desires  intends");
    foreach (var p in new[] { "kettle_on", "cup_clean", "tea_made" })
    {
        Console.WriteLine($"  {p,-12}  {Mark(known, p),-5}  {Mark(believed, p),-8}  {Mark(desired, p),-7}  {Mark(intended, p)}");
    }

    Console.WriteLine();
    Console.WriteLine("  It believes the cup is clean and does not know it. It wants tea that is not made.");
    Console.WriteLine("  It intends a clean cup, because every world it is committed to has one.");

    Verdicts("Mind", typeof(MindProofs));
}
