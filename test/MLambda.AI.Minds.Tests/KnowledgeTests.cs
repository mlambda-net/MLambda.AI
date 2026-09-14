// KnowledgeTests.cs — alice glanced at the door; bob checked it.
//
// THE SCENE. At `home` the door is unlocked. `locked_home` is the same except the door is locked.
// alice glanced and cannot tell the two apart (glimpse home → locked_home), and she settled on
// locked (guess home → locked_home). bob tried the handle: he guesses home → home, and nothing is
// hidden from him.
namespace MLambda.AI.Minds.Tests;

using MLambda.AI.Minds;
using MLambda.AI.Minds.Knowledge;

public class KnowledgeTests
{
    private static IKnowledgeEngine Door()
    {
        var engine = KnowledgeEngineFactory.Create();
        engine.AssertAll([
            new WorldFact("home"), new WorldFact("locked_home"),
            new AgentFact("alice"), new AgentFact("bob"),
            new PropFact("locked"), new PropFact("unlocked"),
            new HoldsFact("home", "unlocked"), new HoldsFact("locked_home", "locked"),
            new GlimpseFact("alice", "home", "locked_home"),
            new GuessFact("alice", "home", "locked_home"),
            new GuessFact("bob", "home", "home"),
        ]);

        return engine;
    }

    [Fact]
    public async Task Alice_believes_the_door_is_locked()
    {
        Assert.Equal(["locked"], await Answers.Sorted(Door().Believed("alice", "home")));
    }

    [Fact]
    public async Task But_she_does_not_know_it_because_it_is_not_so()
    {
        // KNOWLEDGE IS FACTIVE. `home` is never ruled out for her, and the door is unlocked there.
        var door = Door();

        Assert.NotEmpty(await Answers.Sorted(door.Believed("alice", "home")));
        Assert.DoesNotContain("locked", await Answers.Sorted(door.Known("alice", "home")));
    }

    [Fact]
    public async Task Her_belief_is_a_false_belief_and_the_engine_can_say_so()
    {
        Assert.Equal(["locked"], await Answers.Sorted(Door().Mistaken("alice", "home")));
    }

    [Fact]
    public async Task Bob_checked_so_he_believes_and_knows_the_same_thing()
    {
        var door = Door();

        Assert.Equal(["unlocked"], await Answers.Sorted(door.Believed("bob", "home")));
        Assert.Equal(["unlocked"], await Answers.Sorted(door.Known("bob", "home")));
    }

    [Fact]
    public async Task And_a_true_belief_is_not_a_mistake()
    {
        var door = Door();

        Assert.NotEmpty(await Answers.Sorted(door.Believed("bob", "home")));
        Assert.Empty(await Answers.Sorted(door.Mistaken("bob", "home")));
    }

    [Fact]
    public async Task Looking_properly_is_what_turns_a_belief_into_knowledge()
    {
        // carol arrives after the scene, stands where alice stood and tries the handle, like bob:
        // nothing is hidden from her.
        var door = Door();
        door.AssertAll([new AgentFact("carol"), new GuessFact("carol", "home", "home")]);

        Assert.Equal(["unlocked"], await Answers.Sorted(door.Known("carol", "home")));
        Assert.Empty(await Answers.Sorted(door.Mistaken("carol", "home")));
    }

    // ── Knowledge.hp, theorem by theorem: each one proved by the kernel while the test runs ──

    [Fact]
    public void Theorem_knowledge_is_factive() => Theorem.Proved(KnowledgeProofs.KnowledgeIsFactive());

    [Fact]
    public void Theorem_what_you_cannot_rule_out_you_cannot_rule_out() => Theorem.Proved(KnowledgeProofs.WhatYouCannotRuleOutYouCannotRuleOut());

    [Fact]
    public void Theorem_knowing_you_know() => Theorem.Proved(KnowledgeProofs.KnowingYouKnow());

    [Fact]
    public void Theorem_knowing_what_you_do_not_know() => Theorem.Proved(KnowledgeProofs.KnowingWhatYouDoNotKnow());

    [Fact]
    public void Theorem_knowledge_is_symmetric() => Theorem.Proved(KnowledgeProofs.KnowledgeIsSymmetric());

    [Fact]
    public void Theorem_a_guess_is_believed() => Theorem.Proved(KnowledgeProofs.AGuessIsBelieved());

    [Fact]
    public void Theorem_believing_you_believe() => Theorem.Proved(KnowledgeProofs.BelievingYouBelieve());

    [Fact]
    public void Theorem_what_is_believed_is_within_what_is_known() => Theorem.Proved(KnowledgeProofs.WhatIsBelievedIsWithinWhatIsKnown());

    [Fact]
    public void Every_theorem_in_Knowledge_hp_has_a_test_above()
    {
        // A THEOREM ADDED TO THE FILE WITHOUT A TEST HERE FAILS THIS, so the list above stays whole.
        string[] tested =
        [
            "knowledge_is_factive",
            "what_you_cannot_rule_out_you_cannot_rule_out",
            "knowing_you_know",
            "knowing_what_you_do_not_know",
            "knowledge_is_symmetric",
            "a_guess_is_believed",
            "believing_you_believe",
            "what_is_believed_is_within_what_is_known",
        ];

        Assert.Equal(tested.Select(Theorem.Method), Theorem.Of(typeof(KnowledgeProofs)));
    }
}
