// Theorem.cs — what "this theorem is proved" means in a test.
//
// NOT A STRING THE BUILD WROTE. The generated `<File>Proofs` class carries no verdict at all: each
// theorem is a method, `AnimalsProofs.ACatIsAnAnimal()`, that reads the embedded `.hp` and `.hs`,
// runs the proof script, and has the kernel replay the term — so a test passes only when the kernel
// accepts the proof while the test is running.
//
// A COPY OF THE ONE IN MLambda.AI.Logic.Tests, because it is `internal` and each test assembly
// needs its own. Five lines is cheaper than a shared project to hold them.
namespace MLambda.AI.Agent.Tests;

using System.Reflection;
using MLambda.Hilbert.Proof;

internal static class Theorem
{
    /// <summary>Passes when the kernel proved it; otherwise fails with what the kernel said.</summary>
    public static void Proved(Judged verdict) =>
        Assert.True(verdict.Status == "Proved", $"{verdict.Name} is {verdict.Status}: {verdict.Detail}");

    /// <summary>Every theorem a generated proof class proves, as its method names, in file order.</summary>
    ///
    /// <remarks>FOUND, NOT LISTED: the class has no list of its theorems, only the methods, so a
    /// theorem added to the `.hp` is a new method here without anyone remembering to add it.</remarks>
    public static IEnumerable<string> Of(Type proofs) =>
        proofs.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(m => m.ReturnType == typeof(Judged) && m.GetParameters().Length == 0)
            .OrderBy(m => m.MetadataToken)
            .Select(m => m.Name);

    /// <summary>`a_cat_is_an_animal` as the generator names its method: `ACatIsAnAnimal`.</summary>
    public static string Method(string theorem) =>
        string.Concat(theorem.Split('_', StringSplitOptions.RemoveEmptyEntries)
            .Select(part => char.ToUpperInvariant(part[0]) + part[1..]));
}
