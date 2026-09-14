// TrafficTests.cs — a light's run, read with F, G and U.
//
// THE RUN: red, red, green, green, amber, red — six moments, m0 to m5.
namespace MLambda.AI.Minds.Tests;

using MLambda.AI.Minds;
using MLambda.AI.Minds.Traffic;
using MLambda.Shin.Runtime;

public class TrafficTests
{
    private static ITrafficEngine Run()
    {
        var engine = TrafficEngineFactory.Create();
        string[] colours = ["red", "red", "green", "green", "amber", "red"];

        engine.AssertAll([
            new ColourFact("red"), new ColourFact("green"), new ColourFact("amber"),
            .. colours.Select((_, i) => (ShinFact)new MomentFact($"m{i}")),
            .. colours.Skip(1).Select((_, i) => (ShinFact)new NextFact($"m{i}", $"m{i + 1}")),
            .. colours.Select((c, i) => (ShinFact)new ShowsFact($"m{i}", c)),
        ]);

        return engine;
    }

    [Fact]
    public async Task Eventually_sees_every_colour_still_to_come()
    {
        Assert.Equal(["amber", "green", "red"], await Answers.Sorted(Run().EventuallyShows("m0")));
    }

    [Fact]
    public async Task But_not_a_colour_that_has_already_gone()
    {
        // From m4 only amber and red remain; green is in the past, and F looks forward.
        Assert.Equal(["amber", "red"], await Answers.Sorted(Run().EventuallyShows("m4")));
    }

    [Fact]
    public async Task Always_holds_only_where_nothing_later_breaks_it()
    {
        // NON-VACUOUS: from m5 the rest of the run is red, and G says so.
        Assert.Equal(["red"], await Answers.Sorted(Run().AlwaysShows("m5")));
    }

    [Fact]
    public async Task And_from_the_start_no_colour_is_always_on()
    {
        var run = Run();

        Assert.NotEmpty(await Answers.Sorted(run.EventuallyShows("m0")));
        Assert.Empty(await Answers.Sorted(run.AlwaysShows("m0")));
    }

    [Fact]
    public async Task Red_waits_until_green()
    {
        Assert.True(await Run().WaitsUntil("m0", "red", "green"));
    }

    [Fact]
    public async Task But_green_does_not_wait_until_red_when_amber_comes_between()
    {
        var run = Run();

        Assert.True(await run.WaitsUntil("m2", "green", "amber"));
        Assert.False(await run.WaitsUntil("m2", "green", "red"));
    }

    [Fact]
    public async Task The_light_never_shows_two_colours_at_once()
    {
        var run = Run();

        Assert.Equal(6, (await Answers.Sorted(run.MomentsAfter("m0"))).Count);
        Assert.Empty(await Answers.Sorted(run.Clashing()));
    }

    [Fact]
    public async Task And_a_broken_light_is_caught()
    {
        var run = Run();
        run.AssertAll([new ShowsFact("m3", "red")]);

        Assert.Equal(["m3"], await Answers.Sorted(run.Clashing()));
    }

    // ── Traffic.hp, theorem by theorem: each one proved by the kernel while the test runs ──

    [Fact]
    public void Theorem_a_step_is_later() => Theorem.Proved(TrafficProofs.AStepIsLater());

    [Fact]
    public void Theorem_later_composes() => Theorem.Proved(TrafficProofs.LaterComposes());

    [Fact]
    public void Theorem_two_steps_are_later() => Theorem.Proved(TrafficProofs.TwoStepsAreLater());

    [Fact]
    public void Theorem_waiting_ends_when_the_awaited_colour_shows() => Theorem.Proved(TrafficProofs.WaitingEndsWhenTheAwaitedColourShows());

    [Fact]
    public void Theorem_one_more_moment_of_waiting() => Theorem.Proved(TrafficProofs.OneMoreMomentOfWaiting());

    [Fact]
    public void Every_theorem_in_Traffic_hp_has_a_test_above()
    {
        // A THEOREM ADDED TO THE FILE WITHOUT A TEST HERE FAILS THIS, so the list above stays whole.
        string[] tested =
        [
            "a_step_is_later",
            "later_composes",
            "two_steps_are_later",
            "waiting_ends_when_the_awaited_colour_shows",
            "one_more_moment_of_waiting",
        ];

        Assert.Equal(tested.Select(Theorem.Method), Theorem.Of(typeof(TrafficProofs)));
    }
}
