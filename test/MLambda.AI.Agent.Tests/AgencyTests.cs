// AgencyTests.cs — what holds of any agent, whatever it believes.
//
// EVERY THEOREM HERE IS RE-PROVED AT TEST TIME, not read back from the build. `Prove` hands the
// kernel the theory and the proof as the build saw them and lets it decide again, now.
namespace MLambda.AI.Agent.Tests;

using MLambda.AI.Agent;

public class AgencyTests
{
    [Fact]
    public void Every_theorem_was_proved_at_build_time()
    {
        Assert.Equal(7, AgencyProofs.All.Count);
        Assert.All(AgencyProofs.All, claim => Assert.Equal("Proved", claim.Verdict));
    }

    [Fact]
    public void And_every_theorem_proves_again_when_asked()
    {
        // NOT THE BUILD'S VERDICT READ BACK. This parses Agency.hs and Agency.hp, elaborates, and
        // replays the term through the kernel in this process. A verdict that only ever came from
        // a build is a fact about a build; this is a fact about the mathematics.
        Assert.All(
            AgencyProofs.All,
            claim => Assert.Equal("Proved", AgencyProofs.Prove(claim.Name).Status));
    }

    [Fact]
    public void Belief_is_introspective_and_wanting_is_not()
    {
        // KD45 FOR BELIEF, KD FOR THE OTHER TWO -- Rao and Georgeff's own assignment. An agent
        // knows what it believes and knows what it does not believe; nothing of the kind is true
        // of wanting, so there is no transD and no euclidD to prove anything from.
        Assert.Equal("Proved", AgencyProofs.BeliefComposes.Verdict);
        Assert.Equal("Proved", AgencyProofs.BeliefIsEuclidean.Verdict);

        Assert.Contains("transB", AgencyProofs.BeliefComposes.Axioms);
        Assert.DoesNotContain("transD", AgencyProofs.BeliefComposes.Axioms);
        Assert.DoesNotContain("transI", AgencyProofs.BeliefComposes.Axioms);
    }

    [Fact]
    public void Realism_is_stated_over_relations_not_modalities()
    {
        // `I(i) p ⇒ D(i) p` is an implication under a universal in a head, which Horn cannot
        // express (HS0020). Correspondence theory gives R_D ⊆ R_I instead, and the claim shows it:
        // two relations, no modality.
        var realism = AgencyProofs.WhatIsDesiredIsIntended.Claim;

        Assert.Contains("desires", realism);
        Assert.Contains("intends", realism);
        Assert.DoesNotContain("D(", realism);
        Assert.DoesNotContain("I(", realism);
    }

    [Fact]
    public void Seriality_says_a_seeded_successor_is_accessible_and_no_more()
    {
        // WHAT AXIOM D CANNOT SAY HERE. The theorem is an implication FROM a seed, not a claim
        // that every world has one -- because an existential in a head is HS0022. Seriality holds
        // by construction of the seeding, and a reader who expects otherwise will meet a frame
        // that quietly fails to be serial.
        var seeded = AgencyProofs.ASeededBeliefIsAccessible.Claim;

        Assert.Contains("beliefSucc", seeded);
        Assert.Contains("⇒", seeded);
        Assert.DoesNotContain("∃", seeded);
    }

    [Fact]
    public void A_seeded_desire_becomes_an_intention_in_two_steps()
    {
        // Seriality makes the seed a desire; realism carries it across. Every "an agent that wants
        // X will try for X" claim reduces to this, and it is worth seeing it reduce.
        Assert.Equal("Proved", AgencyProofs.ASeededDesireIsIntended.Verdict);
        Assert.Equal("Proved", AgencyProofs.Prove("a_seeded_desire_is_intended").Status);
    }
}
