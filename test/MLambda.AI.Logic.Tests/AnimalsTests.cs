// AnimalsTests.cs — what the theory concludes, and what the build proved before this ran.
namespace MLambda.AI.Logic.Tests;

using MLambda.AI.Logic;
using MLambda.AI.Logic.Animals;

public class AnimalsTests
{
    /// <summary>fluffy is a cat, and a cat and a bird are both kinds of animal.</summary>
    private static IAnimalEngine Zoo()
    {
        var engine = AnimalEngineFactory.Create();
        engine.AssertAll([
            new ThingFact("fluffy"), new ThingFact("cat"),
            new ThingFact("bird"), new ThingFact("animal"),
            new KindOfFact("cat", "animal"),
            new KindOfFact("bird", "animal"),
            new KnownFact("fluffy", "cat"),
        ]);

        return engine;
    }

    /// <summary>Whether the query streamed <paramref name="wanted"/> among its answers.</summary>
    private static async Task<bool> Has(IAsyncEnumerable<string> rows, string wanted)
    {
        await foreach (var row in rows)
        {
            if (row == wanted)
            {
                return true;
            }
        }

        return false;
    }

    [Fact]
    public async Task What_we_were_told_is_still_true()
    {
        Assert.True(await Has(Zoo().Kinds("fluffy"), "cat"));
    }

    [Fact]
    public async Task And_climbing_reaches_the_kind_nobody_mentioned()
    {
        // THE POINT OF THE WHOLE SAMPLE. Nothing was asserted about fluffy and animals; `climbing`
        // composes being a cat with a cat being a kind of animal, and the fixpoint does the rest.
        Assert.True(await Has(Zoo().Kinds("fluffy"), "animal"));
    }

    [Fact]
    public async Task Classification_does_not_run_downwards()
    {
        // A cat is a kind of animal; it does not follow that an animal is a cat.
        Assert.False(await Has(Zoo().Kinds("animal"), "cat"));
    }

    [Fact]
    public async Task A_kind_nobody_placed_fluffy_in_stays_empty_of_it()
    {
        // fluffy is not a bird, and nothing in the theory would make it one.
        Assert.False(await Has(Zoo().Kinds("fluffy"), "bird"));
    }

    [Fact]
    public async Task Climbing_goes_as_far_as_the_kinds_go()
    {
        // `classification_climbs_twice` is proved for any chain; this is the engine walking one.
        var engine = AnimalEngineFactory.Create();
        engine.AssertAll([
            new ThingFact("fluffy"), new ThingFact("cat"),
            new ThingFact("animal"), new ThingFact("living_thing"),
            new KnownFact("fluffy", "cat"),
            new KindOfFact("cat", "animal"),
            new KindOfFact("animal", "living_thing"),
        ]);

        Assert.True(await Has(engine.Kinds("fluffy"), "living_thing"));
    }

    [Fact]
    public async Task Knowledge_added_later_is_derived_over()
    {
        // AN EXPERT SYSTEM, not a batch deducer. The engine answers, learns, and answers again.
        var engine = AnimalEngineFactory.Create();
        engine.AssertAll([
            new ThingFact("rex"), new ThingFact("dog"), new KnownFact("rex", "dog"),
        ]);

        Assert.False(await Has(engine.Kinds("rex"), "animal"));

        engine.AssertAll([new ThingFact("animal"), new KindOfFact("dog", "animal")]);

        Assert.True(await Has(engine.Kinds("rex"), "animal"));
    }

    [Fact]
    public void Every_theorem_in_this_sample_was_proved()
    {
        Assert.NotEmpty(AnimalsProofs.All);
        Assert.All(AnimalsProofs.All, claim => Assert.Equal("Proved", claim.Verdict));
    }

    [Fact]
    public void The_proofs_name_the_axioms_they_leaned_on()
    {
        // "Proved under these axioms" is a different statement from "proved".
        var climbed = AnimalsProofs.ACatIsAnAnimal;

        Assert.Equal("theorem", climbed.Kind);
        Assert.Contains("directly", climbed.Axioms);
        Assert.Contains("climbing", climbed.Axioms);
    }
}
