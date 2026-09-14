// DeadlinesTests.cs — three people, one report, and today is Thursday.
//
// ana's report was due Tuesday and she filed it Monday. ben's was due Tuesday; he was told on Monday
// and never filed. cy's was due Wednesday; nobody told him, and he filed on Thursday, too late.
namespace MLambda.AI.Minds.Tests;

using MLambda.AI.Minds;
using MLambda.AI.Minds.Deadlines;
using MLambda.Shin.Runtime;

public class DeadlinesTests
{
    private static ShinFact[] Calendar() =>
    [
        new MomentFact("mon"), new MomentFact("tue"), new MomentFact("wed"), new MomentFact("thu"),
        new NextFact("mon", "tue"), new NextFact("tue", "wed"), new NextFact("wed", "thu"),
        new TodayFact("thu"),
    ];

    private static IDeadlineEngine Week()
    {
        var engine = DeadlineEngineFactory.Create();
        engine.AssertAll([
            .. Calendar(),
            new DueFact("ana", "report", "tue"), new DueFact("ben", "report", "tue"), new DueFact("cy", "report", "wed"),
            new DidFact("mon", "ana", "report"),
            new InformedFact("ben", "report", "mon"),
            new DidFact("thu", "cy", "report"),
        ]);

        return engine;
    }

    [Fact]
    public async Task Doing_it_before_the_deadline_keeps_it()
    {
        Assert.Equal([new KeptRow("ana", "report", "tue")], await Answers.Rows(Week().Kept()));
    }

    [Fact]
    public async Task A_duty_you_were_told_of_and_missed_is_a_knowing_breach()
    {
        Assert.Equal([new BreachesOfRow("ben", "report")], await Answers.Rows(Week().BreachesOf()));
    }

    [Fact]
    public async Task Doing_it_late_does_not_keep_it_and_not_knowing_excuses_it()
    {
        var week = Week();

        Assert.Equal([new ExcusedOfRow("cy", "report")], await Answers.Rows(week.ExcusedOf()));
        Assert.DoesNotContain(new KeptRow("cy", "report", "wed"), await Answers.Rows(week.Kept()));
    }

    [Fact]
    public async Task A_kept_duty_is_never_breached()
    {
        var week = Week();

        Assert.Contains(new KeptRow("ana", "report", "tue"), await Answers.Rows(week.Kept()));
        Assert.DoesNotContain(new BreachesOfRow("ana", "report"), await Answers.Rows(week.BreachesOf()));
        Assert.DoesNotContain(new ExcusedOfRow("ana", "report"), await Answers.Rows(week.ExcusedOf()));
    }

    [Fact]
    public async Task Telling_someone_turns_an_excuse_into_a_breach()
    {
        // ONE BATCH, as every scene here is asserted: a conclusion drawn from `¬ knows_due` is not
        // withdrawn by a later batch today — see DutyTests.But_today_a_deed_told_later_… and
        // docs/hilbert/06-diagnostics.md.
        var week = DeadlineEngineFactory.Create();
        week.AssertAll([
            .. Calendar(),
            new DueFact("cy", "report", "wed"),
            new DidFact("thu", "cy", "report"),
            new InformedFact("cy", "report", "tue"),
        ]);

        Assert.Empty(await Answers.Rows(week.ExcusedOf()));
        Assert.Contains(new BreachesOfRow("cy", "report"), await Answers.Rows(week.BreachesOf()));
    }

    // ── Deadlines.hp, theorem by theorem: each one proved by the kernel while the test runs ──

    [Fact]
    public void Theorem_what_you_were_told_you_still_know_later() => Theorem.Proved(DeadlinesProofs.WhatYouWereToldYouStillKnowLater());

    [Fact]
    public void Theorem_told_the_day_before_is_known_on_the_day() => Theorem.Proved(DeadlinesProofs.ToldTheDayBeforeIsKnownOnTheDay());

    [Fact]
    public void Theorem_doing_it_in_time_fulfils_it() => Theorem.Proved(DeadlinesProofs.DoingItInTimeFulfilsIt());

    [Fact]
    public void Every_theorem_in_Deadlines_hp_has_a_test_above()
    {
        // A THEOREM ADDED TO THE FILE WITHOUT A TEST HERE FAILS THIS, so the list above stays whole.
        string[] tested =
        [
            "what_you_were_told_you_still_know_later",
            "told_the_day_before_is_known_on_the_day",
            "doing_it_in_time_fulfils_it",
        ];

        Assert.Equal(tested.Select(Theorem.Method), Theorem.Of(typeof(DeadlinesProofs)));
    }
}
