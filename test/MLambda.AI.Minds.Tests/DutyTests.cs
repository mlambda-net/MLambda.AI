// DutyTests.cs — a small office rulebook, and the frame behind it.
//
// ana is the keyholder and staff; ben is staff. Keyholders must lock up; staff may not smoke.
namespace MLambda.AI.Minds.Tests;

using MLambda.AI.Minds;
using MLambda.AI.Minds.Duty;

public class DutyTests
{
    private static IDutyEngine Office()
    {
        var engine = DutyEngineFactory.Create();
        engine.AssertAll([
            new PersonFact("ana"), new PersonFact("ben"),
            new ActFact("lock_up"), new ActFact("smoke"), new ActFact("sweep"),
            new PlaysFact("ana", "keyholder"), new PlaysFact("ana", "staff"), new PlaysFact("ben", "staff"),
            new ObligesFact("keyholder", "lock_up"),
            new ForbidsFact("staff", "smoke"),
        ]);

        return engine;
    }

    [Fact]
    public async Task A_role_passes_its_obligations_to_whoever_plays_it()
    {
        var office = Office();

        Assert.Equal(["lock_up"], await Answers.Sorted(office.Obligations("ana")));
        Assert.Empty(await Answers.Sorted(office.Obligations("ben")));
    }

    [Fact]
    public async Task What_is_not_forbidden_is_permitted()
    {
        Assert.Equal(["lock_up", "sweep"], await Answers.Sorted(Office().Permissions("ben")));
    }

    [Fact]
    public async Task An_obligation_does_not_make_itself_true()
    {
        // Nobody asserted that ana locked up, so the obligation stands unmet.
        Assert.Equal([new ViolationsRow("ana", "lock_up")], await Answers.Rows(Office().Violations()));
    }

    [Fact]
    public async Task And_doing_it_discharges_it()
    {
        // THE SAME OFFICE, TOLD IN ONE BATCH that ana locked up. See the next test for why one batch.
        var office = DutyEngineFactory.Create();
        office.AssertAll([
            new PersonFact("ana"), new ActFact("lock_up"),
            new PlaysFact("ana", "keyholder"), new ObligesFact("keyholder", "lock_up"),
            new DidFact("ana", "lock_up"),
        ]);

        Assert.Equal(["lock_up"], await Answers.Sorted(office.Obligations("ana")));
        Assert.Empty(await Answers.Rows(office.Violations()));
    }

    [Fact]
    public async Task But_today_a_deed_told_later_does_not_withdraw_the_violation_already_drawn()
    {
        // A LIMIT OF THE SHIN ENGINE, NOT OF THE LOGIC, pinned so its fix is noticed.
        //
        // `violation` rests on `¬ did`. The engine derived it from the first batch, when nothing said
        // ana had locked up — and a later batch that says so does not revisit it. A conclusion drawn
        // from an ABSENCE is not withdrawn when the absence ends: the forward engine recomputes what
        // new facts ADD, not what they DEFEAT. Stratified semantics says the violation should go.
        //
        // IF THIS TEST STARTS FAILING, SHIN FIXED IT. Delete this test, fold the one above back into a
        // second `AssertAll` on `Office()`, and remove the limit from docs/L3-advanced/minds.md.
        var office = Office();
        office.AssertAll([new DidFact("ana", "lock_up")]);

        Assert.Equal([new ViolationsRow("ana", "lock_up")], await Answers.Rows(office.Violations()));
    }

    [Fact]
    public async Task A_rulebook_that_obliges_and_forbids_the_same_act_is_caught()
    {
        var office = Office();

        Assert.Empty(await Answers.Rows(office.Conflicts()));

        office.AssertAll([new ObligesFact("staff", "smoke")]);

        var clashes = await Answers.Rows(office.Conflicts());
        Assert.Contains(new ConflictsRow("ana", "smoke"), clashes);
        Assert.Contains(new ConflictsRow("ben", "smoke"), clashes);
    }

    // ── Duty.hp, theorem by theorem: each one proved by the kernel while the test runs ──

    [Fact]
    public void Theorem_every_seeded_situation_has_an_ideal() => Theorem.Proved(DutyProofs.EverySeededSituationHasAnIdeal());

    [Fact]
    public void Theorem_a_role_passes_its_obligations_to_whoever_plays_it() => Theorem.Proved(DutyProofs.ARolePassesItsObligationsToWhoeverPlaysIt());

    [Fact]
    public void Theorem_and_its_prohibitions_too() => Theorem.Proved(DutyProofs.AndItsProhibitionsToo());

    [Fact]
    public void Every_theorem_in_Duty_hp_has_a_test_above()
    {
        // A THEOREM ADDED TO THE FILE WITHOUT A TEST HERE FAILS THIS, so the list above stays whole.
        string[] tested =
        [
            "every_seeded_situation_has_an_ideal",
            "a_role_passes_its_obligations_to_whoever_plays_it",
            "and_its_prohibitions_too",
        ];

        Assert.Equal(tested.Select(Theorem.Method), Theorem.Of(typeof(DutyProofs)));
    }
}
