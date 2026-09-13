// IdentitiesTests.cs — the whiteboard algebra, proved by the kernel while the test runs.
//
// THE ARGUMENT FOR THIS WHOLE REPOSITORY, in one file. `QTable.hb` and `Sarsa.hb` compute targets and
// subtract estimates; `Identities.hp` proves the algebra those computations stand on. The models
// learn; this says the arithmetic they learn with is what the textbook says it is.
namespace MLambda.AI.Learning.Tests;

using MLambda.AI.Learning;
using MLambda.Hilbert.Proof;

public class IdentitiesTests
{
    [Fact]
    public void None_of_them_assumed_anything()
    {
        // `axioms []`. Every claim is decided by its own certificate, so there is nothing in the
        // corpus a reader has to take on trust.
        Assert.All(IdentitiesProofs.All, claim => Assert.Equal(string.Empty, claim.Axioms));
    }

    [Fact]
    public void The_step_size_the_gridworld_uses_is_the_one_proved_safe()
    {
        // `GridworldTests` trains at rate 0.5 and asserts one step moves an entry halfway. These two
        // theorems are what makes that more than an observation: halfway is exactly where a half
        // step lands, and it never passes its target.
        Theorem.Proved(IdentitiesProofs.Prove("a_half_step_lands_halfway"));
        Theorem.Proved(IdentitiesProofs.Prove("a_half_step_does_not_pass_its_target"));
    }

    // ── Identities.hp, theorem by theorem: each one proved by the kernel while the test runs ──

    [Fact]
    public void Theorem_the_td_error_is_the_backup_less_the_estimate() => Theorem.Proved(IdentitiesProofs.Prove("the_td_error_is_the_backup_less_the_estimate"));

    [Fact]
    public void Theorem_a_full_step_replaces_the_estimate() => Theorem.Proved(IdentitiesProofs.Prove("a_full_step_replaces_the_estimate"));

    [Fact]
    public void Theorem_a_null_step_changes_nothing() => Theorem.Proved(IdentitiesProofs.Prove("a_null_step_changes_nothing"));

    [Fact]
    public void Theorem_a_half_step_lands_halfway() => Theorem.Proved(IdentitiesProofs.Prove("a_half_step_lands_halfway"));

    [Fact]
    public void Theorem_a_two_step_return_unrolls() => Theorem.Proved(IdentitiesProofs.Prove("a_two_step_return_unrolls"));

    [Fact]
    public void Theorem_an_epsilon_greedy_row_sums_to_one() => Theorem.Proved(IdentitiesProofs.Prove("an_epsilon_greedy_row_sums_to_one"));

    [Fact]
    public void Theorem_a_half_step_does_not_pass_its_target() => Theorem.Proved(IdentitiesProofs.Prove("a_half_step_does_not_pass_its_target"));

    [Fact]
    public void Every_theorem_in_Identities_hp_has_a_test_above()
    {
        // A THEOREM ADDED TO THE FILE WITHOUT A TEST HERE FAILS THIS, so the list above stays whole.
        string[] tested =
        [
            "the_td_error_is_the_backup_less_the_estimate",
            "a_full_step_replaces_the_estimate",
            "a_null_step_changes_nothing",
            "a_half_step_lands_halfway",
            "a_two_step_return_unrolls",
            "an_epsilon_greedy_row_sums_to_one",
            "a_half_step_does_not_pass_its_target",
        ];

        Assert.Equal(tested, IdentitiesProofs.All.Select(claim => claim.Name));
    }

    // ── the known gap, pinned so its fix is noticed ────────────────────────────────────────────

    private static readonly ProofSource Theory = new("Gap.hs", "theory Gap (Double)\n{\n  def valued(q: Double)\n}\n");

    private static Judged Attempt(string claim) =>
        Prover.Prove(
            new ProofSource("Gap.hp", $"open Gap\naxioms []\n\ntheorem gap : {claim}\nproof\n  intro c t below\n  linarith\nqed\n"),
            [Theory],
            "gap");

    [Fact]
    public void Linarith_proves_a_half_written_as_a_divisor_or_a_decimal()
    {
        // THE CONTROL. Written these ways, the same linear claim proves. If THIS test fails, the
        // test below is meaningless -- the inline-source harness itself is broken.
        Theorem.Proved(Attempt("∀ c t, c <= t ⇒ c / 2 <= t / 2"));
        Theorem.Proved(Attempt("∀ c t, c <= t ⇒ 0.5 * c <= 0.5 * t"));
    }

    [Fact]
    public void But_today_refuses_a_half_written_as_one_over_two_times_something()
    {
        // A GAP IN HILBERT'S `linarith`, NOT IN THE MATHEMATICS. `(1 / 2) · c ≤ (1 / 2) · t` from
        // `c ≤ t` is as linear as a claim can be, and it is refused as though `1 / 2` were a variable
        // multiplying `c`. `0.5 · c` and `c / 2` prove, and `ring` accepts `(1 / 2) * (t − c)`.
        //
        // IF THIS TEST STARTS FAILING, HILBERT FIXED IT. Delete this test, respell
        // `a_half_step_does_not_pass_its_target` in Identities.hp as `(1 / 2) * (t - c)` if you like,
        // and remove the comment there that explains the spelling.
        var verdict = Attempt("∀ c t, c <= t ⇒ (1 / 2) * c <= (1 / 2) * t");

        Assert.NotEqual("Proved", verdict.Status);
    }
}
