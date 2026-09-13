// AnimalsTests.cs — what the theory concludes, and what the build proved before this ran.
//
// EVERY NEGATIVE ASSERTION HERE IS PAIRED WITH A POSITIVE ONE, on purpose. A query that answers
// nothing satisfies every `DoesNotContain` ever written, so a test that only checks for an absence
// passes just as happily against a broken engine as a working one. An earlier version of
// `Classification_does_not_run_downwards` asked whether "cat" was among the kinds of "animal" —
// which is the empty list, so it asserted nothing at all. Pinning the whole answer set is what
// stops that coming back.
namespace MLambda.AI.Logic.Tests;

using MLambda.AI.Logic;
using MLambda.AI.Logic.Animals;

public class AnimalsTests
{
    /// <summary>fluffy is a cat, tweety is a bird, and both are kinds of animal.</summary>
    private static IAnimalEngine Zoo()
    {
        var engine = AnimalEngineFactory.Create();
        engine.AssertAll([
            new ThingFact("fluffy"), new ThingFact("tweety"),
            new ThingFact("cat"), new ThingFact("bird"), new ThingFact("animal"),
            new KindOfFact("cat", "animal"),
            new KindOfFact("bird", "animal"),
            new KnownFact("fluffy", "cat"),
            new KnownFact("tweety", "bird"),
        ]);

        return engine;
    }

    /// <summary>Everything the query streamed, sorted so a test can pin the whole set.</summary>
    private static async Task<List<string>> Kinds(IAnimalEngine engine, string of)
    {
        var all = new List<string>();

        await foreach (var row in engine.Kinds(of))
        {
            all.Add(row);
        }

        all.Sort(StringComparer.Ordinal);

        return all;
    }

    [Fact]
    public async Task What_we_were_told_is_still_true()
    {
        Assert.Contains("cat", await Kinds(Zoo(), "fluffy"));
    }

    [Fact]
    public async Task And_climbing_reaches_the_kind_nobody_mentioned()
    {
        // THE POINT OF THE WHOLE SAMPLE. Nothing was asserted about fluffy and animals; `climbing`
        // composes being a cat with a cat being a kind of animal, and the fixpoint does the rest.
        Assert.Contains("animal", await Kinds(Zoo(), "fluffy"));
    }

    [Fact]
    public async Task Fluffy_is_exactly_a_cat_and_an_animal_and_nothing_else()
    {
        // The whole answer, not a sample of it: one thing told, one thing concluded, no third.
        Assert.Equal(["animal", "cat"], await Kinds(Zoo(), "fluffy"));
    }

    [Fact]
    public async Task Classification_does_not_run_downwards()
    {
        // tweety IS an animal, and a cat is a kind of animal -- yet tweety is not a cat. The rule
        // climbs and never descends, and the theory says that only by never saying otherwise.
        //
        // NON-VACUOUS: tweety has kinds, and the assertion pins all of them.
        var kinds = await Kinds(Zoo(), "tweety");

        Assert.Equal(["animal", "bird"], kinds);
        Assert.DoesNotContain("cat", kinds);
    }

    [Fact]
    public async Task A_kind_nobody_placed_fluffy_in_stays_empty_of_it()
    {
        var kinds = await Kinds(Zoo(), "fluffy");

        Assert.NotEmpty(kinds);
        Assert.DoesNotContain("bird", kinds);
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

        Assert.Equal(["animal", "cat", "living_thing"], await Kinds(engine, "fluffy"));
    }

    [Fact]
    public async Task Knowledge_added_later_is_derived_over()
    {
        // AN EXPERT SYSTEM, not a batch deducer. The engine answers, learns, and answers again.
        var engine = AnimalEngineFactory.Create();
        engine.AssertAll([
            new ThingFact("rex"), new ThingFact("dog"), new KnownFact("rex", "dog"),
        ]);

        // NON-VACUOUS: rex is already known to be a dog, so this list is not empty -- it simply
        // does not reach `animal` yet, because nothing has said what a dog is.
        Assert.Equal(["dog"], await Kinds(engine, "rex"));

        engine.AssertAll([new ThingFact("animal"), new KindOfFact("dog", "animal")]);

        Assert.Equal(["animal", "dog"], await Kinds(engine, "rex"));
    }

    [Fact]
    public async Task A_creature_nobody_classified_has_no_kinds()
    {
        // The empty answer, asserted deliberately in ONE place, so that every other test in this
        // file can assume a non-empty one and mean something by it.
        var engine = AnimalEngineFactory.Create();
        engine.AssertAll([new ThingFact("mystery")]);

        Assert.Empty(await Kinds(engine, "mystery"));
    }

    // ── Animals.hp, theorem by theorem: each one proved by the kernel while the test runs ──

    [Fact]
    public void Theorem_what_is_known_is_so() => Theorem.Proved(AnimalsProofs.Prove("what_is_known_is_so"));

    [Fact]
    public void Theorem_a_cat_is_an_animal() => Theorem.Proved(AnimalsProofs.Prove("a_cat_is_an_animal"));

    [Fact]
    public void Theorem_classification_climbs_twice() => Theorem.Proved(AnimalsProofs.Prove("classification_climbs_twice"));

    [Fact]
    public void Theorem_an_animal_found_automatically() => Theorem.Proved(AnimalsProofs.Prove("an_animal_found_automatically"));

    [Fact]
    public void Every_theorem_in_Animals_hp_has_a_test_above()
    {
        // A THEOREM ADDED TO THE FILE WITHOUT A TEST HERE FAILS THIS, so the list above stays whole.
        string[] tested =
        [
            "what_is_known_is_so",
            "a_cat_is_an_animal",
            "classification_climbs_twice",
            "an_animal_found_automatically",
        ];

        Assert.Equal(tested, AnimalsProofs.All.Select(claim => claim.Name));
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
