// Theorem.cs — what "this theorem is proved" means in a test.
//
// NOT A STRING THE BUILD WROTE. The generated `MathsProofs` class carries no verdict at all: each
// theorem is a method that reads the embedded `.hp` and `.hs`, runs the proof script, and has the
// kernel replay the term — so a test passes only when the kernel accepts the proof while the test
// is running.
namespace MLambda.AI.Maths.Tests;

using MLambda.Hilbert.Proof;

internal static class Theorem
{
    /// <summary>Passes when the kernel proved it; otherwise fails with what the kernel said.</summary>
    public static void Proved(Judged verdict) =>
        Assert.True(verdict.Status == "Proved", $"{verdict.Name} is {verdict.Status}: {verdict.Detail}");
}
