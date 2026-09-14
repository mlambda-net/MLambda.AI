// AgencyTests.cs — what holds of any agent, whatever it believes.
//
// EVERY THEOREM HERE IS PROVED AT TEST TIME, not read back from the build. `Theorem.Proved` hands
// the kernel the theory and the proof and lets it decide again, now -- and on failure reports what
// the kernel said, which a comparison against the string "Proved" never could.
namespace MLambda.AI.Agent.Tests;

using MLambda.AI.Agent;

public class AgencyTests
{
    [Fact]
    public void Belief_is_introspective_and_wanting_is_not()
    {
        // KD45 FOR BELIEF, KD FOR THE OTHER TWO -- Rao and Georgeff's own assignment. An agent
        // knows what it believes and knows what it does not believe; nothing of the kind is true
        // of wanting, so there is no transD and no euclidD to prove anything from.
        Theorem.Proved(AgencyProofs.BeliefComposes());
        Theorem.Proved(AgencyProofs.BeliefIsEuclidean());
    }

    [Fact]
    public void A_seeded_desire_becomes_an_intention_in_two_steps()
    {
        // Seriality makes the seed a desire; realism carries it across. Every "an agent that wants
        // X will try for X" claim reduces to this, and it is worth seeing it reduce.
        Theorem.Proved(AgencyProofs.ASeededDesireIsIntended());
    }

    // ── Agency.hp, theorem by theorem: each one proved by the kernel while the test runs ──

    [Fact]
    public void Theorem_belief_composes() => Theorem.Proved(AgencyProofs.BeliefComposes());

    [Fact]
    public void Theorem_belief_is_euclidean() => Theorem.Proved(AgencyProofs.BeliefIsEuclidean());

    [Fact]
    public void Theorem_belief_reaches_three_deep() => Theorem.Proved(AgencyProofs.BeliefReachesThreeDeep());

    [Fact]
    public void Theorem_what_is_desired_is_intended() => Theorem.Proved(AgencyProofs.WhatIsDesiredIsIntended());

    [Fact]
    public void Theorem_a_seeded_desire_is_intended() => Theorem.Proved(AgencyProofs.ASeededDesireIsIntended());

    [Fact]
    public void Theorem_a_seeded_belief_is_accessible() => Theorem.Proved(AgencyProofs.ASeededBeliefIsAccessible());

    [Fact]
    public void Theorem_an_intention_seeded_is_held() => Theorem.Proved(AgencyProofs.AnIntentionSeededIsHeld());

    [Fact]
    public void Every_theorem_in_Agency_hp_has_a_test_above()
    {
        // A THEOREM ADDED TO THE FILE WITHOUT A TEST HERE FAILS THIS, so the list above stays whole.
        string[] tested =
        [
            "belief_composes",
            "belief_is_euclidean",
            "belief_reaches_three_deep",
            "what_is_desired_is_intended",
            "a_seeded_desire_is_intended",
            "a_seeded_belief_is_accessible",
            "an_intention_seeded_is_held",
        ];

        Assert.Equal(tested.Select(Theorem.Method), Theorem.Of(typeof(AgencyProofs)));
    }
}
