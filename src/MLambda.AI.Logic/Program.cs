// Program.cs — the one C# file here: it runs Animals.hs and prints what happened.
//
// EVERY CLAIM BELOW IS HILBERT'S, not C#'s. Nothing here decides that fluffy is an animal;
// `Animals.hs` says how classification climbs and the engine works it out. The `await foreach` is
// because `kind?: Thing` marks an OUTPUT — the query streams answers rather than agreeing to a pair
// you already knew.
//
// THE ENGINE TYPE IS SINGULAR. A theory called `Animals` generates `IAnimalEngine` and
// `AnimalEngineFactory`, the same way upstream's `Requirements` generates `IRequirementEngine`.
using MLambda.AI.Logic;
using MLambda.AI.Logic.Animals;

var zoo = AnimalEngineFactory.Create();
zoo.AssertAll([
    new ThingFact("fluffy"), new ThingFact("cat"),
    new ThingFact("bird"), new ThingFact("animal"),
    new KindOfFact("cat", "animal"),
    new KindOfFact("bird", "animal"),
    new KnownFact("fluffy", "cat"),
]);

Console.WriteLine("Told the engine two things about kinds, and one about Fluffy:");
Console.WriteLine("  a cat is a kind of animal");
Console.WriteLine("  a bird is a kind of animal");
Console.WriteLine("  fluffy is known to be a cat");
Console.WriteLine();

Console.Write("What is fluffy?      ");
await foreach (var kind in zoo.Kinds("fluffy"))
{
    Console.Write($"{kind} ");
}

Console.WriteLine();
Console.WriteLine("  'animal' is in that list and nobody put it there.");
Console.WriteLine();

Console.WriteLine("And what the build checked before this program was allowed to run:");

foreach (var claim in AnimalsProofs.All)
{
    Console.WriteLine($"  {claim.Verdict,-8} {claim.Name}");
    Console.WriteLine($"           {claim.Claim}");
}
