// ProofTests.cs — each theorem in Policy.hp, proved by the kernel while the test runs.
namespace MLambda.AI.Airline.Tests;

using System.Reflection;
using MLambda.Hilbert.Proof;

public class ProofTests
{
    [Fact]
    public void Every_seeded_situation_has_an_ideal() =>
        Proved(PolicyProofs.EverySeededSituationHasAnIdeal());

    [Fact]
    public void What_a_policy_obliges_holds_wherever_things_are_ideal() =>
        Proved(PolicyProofs.WhatAPolicyObligesHoldsWhereverThingsAreIdeal());

    [Fact]
    public void Doing_what_a_policy_forbids_is_a_violation() =>
        Proved(PolicyProofs.DoingWhatAPolicyForbidsIsAViolation());

    [Fact]
    public void Two_steps_are_later() =>
        Proved(PolicyProofs.TwoStepsAreLater());

    [Fact]
    public void Flying_leaves_the_trip_flown_two_moments_on() =>
        Proved(PolicyProofs.FlyingLeavesTheTripFlownTwoMomentsOn());

    [Fact]
    public void A_bereavement_fare_asked_for_after_flying_is_forbidden() =>
        Proved(PolicyProofs.ABereavementFareAskedForAfterFlyingIsForbidden());

    [Fact]
    public void So_granting_it_anyway_is_a_violation() =>
        Proved(PolicyProofs.SoGrantingItAnywayIsAViolation());

    [Fact]
    public void A_cancellation_by_the_airline_obliges_a_refund() =>
        Proved(PolicyProofs.ACancellationByTheAirlineObligesARefund());

    [Fact]
    public void Every_theorem_in_the_file_has_a_test_here()
    {
        var tested = typeof(ProofTests).GetMethods()
            .Select(m => string.Concat(m.Name.Split('_', StringSplitOptions.RemoveEmptyEntries).Select(p => char.ToUpperInvariant(p[0]) + p[1..])))
            .ToHashSet(StringComparer.Ordinal);

        var theorems = typeof(PolicyProofs).GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(m => m.ReturnType == typeof(Judged) && m.GetParameters().Length == 0)
            .Select(m => m.Name);

        Assert.All(theorems, theorem => Assert.Contains(theorem, tested));
    }

    private static void Proved(Judged verdict) =>
        Assert.True(verdict.Status == "Proved", $"{verdict.Name} is {verdict.Status}: {verdict.Detail}");
}
