// RefusalTests.cs — the claims these logics must NOT prove, asked of the kernel.
//
// A `.hp` FILE CAN ONLY SAY WHAT PROVES. That belief is not factive, and that what ought to be is not
// thereby so, are statements about the ABSENCE of a proof — so each is attempted here under the axioms
// its logic allows, and must come back refused. Every refusal is paired with a control that proves
// through the same harness, so a broken harness cannot pass for a refusal.
namespace MLambda.AI.Minds.Tests;

public class RefusalTests
{
    private const string Belief = "serialB, transB, euclidB";

    [Fact]
    public void Knowledge_is_factive_through_this_harness()
    {
        Theorem.Proved(Theorem.Attempt(
            "Knowledge", "reflK", "∀ i w, agent(i) ⇒ world(w) ⇒ knows(i, w, w)", "  intro i w a x\n  apply reflK [a, x]"));
    }

    [Fact]
    public void Belief_is_not()
    {
        // KD45 has no reflexivity. Nothing it permits concludes `believes(i, w, w)`.
        var verdict = Theorem.Attempt(
            "Knowledge", Belief, "∀ i w, agent(i) ⇒ world(w) ⇒ believes(i, w, w)", "  intro i w a x\n  auto");

        Assert.Equal("Rejected", verdict.Status);
    }

    [Fact]
    public void And_borrowing_knowledges_law_for_belief_is_refused_by_name()
    {
        // Even with `reflK` permitted, it concludes `knows`, and the goal is `believes`.
        var verdict = Theorem.Attempt(
            "Knowledge", Belief + ", reflK", "∀ i w, agent(i) ⇒ world(w) ⇒ believes(i, w, w)", "  intro i w a x\n  apply reflK [a, x]");

        Assert.Equal("Rejected", verdict.Status);
        Assert.Contains("knows", verdict.Detail, StringComparison.Ordinal);
    }

    [Fact]
    public void Desire_is_not_factive_either()
    {
        // Wanting tea does not make tea. KD and realism conclude nothing about `desires(i, w, w)`.
        var verdict = Theorem.Attempt(
            "Mind", "serialD, realism", "∀ i w, agent(i) ⇒ world(w) ⇒ desires(i, w, w)", "  intro i w a x\n  auto");

        Assert.Equal("Rejected", verdict.Status);
    }

    [Fact]
    public void Duty_has_an_ideal_where_one_was_seeded_through_this_harness()
    {
        Theorem.Proved(Theorem.Attempt(
            "Duty", "serialO", "∀ w u, idealSeed(w, u) ⇒ ideal(w, u)", "  intro w u seeded\n  apply serialO [seeded]"));
    }

    [Fact]
    public void But_what_ought_to_be_is_not_thereby_what_is()
    {
        var verdict = Theorem.Attempt("Duty", "serialO", "∀ w, situation(w) ⇒ ideal(w, w)", "  intro w s\n  auto");

        Assert.Equal("Rejected", verdict.Status);
    }
}
