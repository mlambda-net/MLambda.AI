// Theorem.cs — what "this theorem is proved" means in a test.
//
// NOT A STRING THE BUILD WROTE. A generated `ProvedClaim.Verdict` is the build's answer, copied into
// C#; asserting on it checks the copy. Every theorem test here calls `<File>Proofs.Prove(name)`
// instead, which parses the `.hp` and its `.hs`, runs the proof script, and has the kernel replay
// the term — so a test passes only when the kernel accepts the proof while the test is running.
namespace MLambda.AI.Logic.Tests;

using MLambda.Hilbert.Proof;

internal static class Theorem
{
    /// <summary>Passes when the kernel proved it; otherwise fails with what the kernel said.</summary>
    public static void Proved(Judged verdict) =>
        Assert.True(verdict.Status == "Proved", $"{verdict.Name} is {verdict.Status}: {verdict.Detail}");
}
