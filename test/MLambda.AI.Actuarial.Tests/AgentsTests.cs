// AgentsTests.cs — three agents, three views of one purchase, and they are allowed to disagree.
namespace MLambda.AI.Actuarial.Tests;

using MLambda.AI.Actuarial;
using Cond = MLambda.AI.Actuarial.ConditionAgent;
using Cred = MLambda.AI.Actuarial.CreditAgent;
using Rsk = MLambda.AI.Actuarial.Risk;
using Val = MLambda.AI.Actuarial.ValuationAgent;

public class AgentsTests
{
    private static async Task<List<string>> Rows<T>(IAsyncEnumerable<T> source, Func<T, string> pick)
    {
        var all = new List<string>();

        await foreach (var row in source)
        {
            all.Add(pick(row));
        }

        all.Sort(StringComparer.Ordinal);

        return all;
    }

    // ── the valuation agent, in hundredths of a spread: buy up to 1.00, negotiate up to 3.00 ──

    private static Val.IValuationagentEngine Valuing(int excess)
    {
        var engine = Val.ValuationagentEngineFactory.Create();
        engine.AssertAll([
            new Val.DesireFact("v", "Decided"),
            new Val.ExcessFact("v", excess),
            new Val.ToleranceFact("v", 100),
            new Val.CeilingFact("v", 300),
        ]);

        return engine;
    }

    [Theory]
    [InlineData(-50, "Buy")]
    [InlineData(100, "Buy")]
    [InlineData(101, "Negotiate")]
    [InlineData(300, "Negotiate")]
    [InlineData(301, "Walk")]
    public async Task The_valuation_agent_draws_its_lines_exactly_where_it_says(int excess, string plan)
    {
        // THE BOUNDARIES, because an off-by-one here would be invisible in every other test. A bargain
        // (negative excess) is a buy; exactly at tolerance is still a buy; one hundredth over is not.
        Assert.Equal([plan], await Rows(Valuing(excess).Intentions("v"), row => row.Plan));
    }

    [Fact]
    public async Task And_it_proposes_exactly_one_verdict_at_a_time()
    {
        // NON-VACUOUS FOR THE THEORY ABOVE: the three bands do not overlap, so no excess licenses two.
        foreach (var excess in new[] { -50, 0, 100, 150, 300, 500 })
        {
            Assert.Single(await Rows(Valuing(excess).Intentions("v"), row => row.Plan));
        }
    }

    [Fact]
    public async Task A_decision_is_met_once_the_host_says_it_acted()
    {
        // HS0062 forced an `achieved` rule, and this is what it means: the agent does not decide that it
        // has decided. The host acts, then asserts `settled`.
        var engine = Valuing(50);

        Assert.Empty(await Rows(engine.Met("v"), goal => goal));

        engine.AssertAll([new Val.SettledFact("v", 1)]);

        Assert.Equal(["Decided"], await Rows(engine.Met("v"), goal => goal));
    }

    // ── the other two agents share the shape ─────────────────────────────────────────────────

    [Theory]
    [InlineData(0, "Buy")]
    [InlineData(20, "Negotiate")]
    [InlineData(60, "Walk")]
    public async Task The_condition_agent_judges_repair_exposure_in_thousands(int thousands, string plan)
    {
        var engine = Cond.ConditionagentEngineFactory.Create();
        engine.AssertAll([
            new Cond.DesireFact("c", "Decided"),
            new Cond.ExposureFact("c", thousands),
            new Cond.ToleranceFact("c", 10),
            new Cond.CeilingFact("c", 40),
        ]);

        Assert.Equal([plan], await Rows(engine.Intentions("c"), row => row.Plan));
    }

    [Theory]
    [InlineData(900, "Buy")]
    [InlineData(1500, "Negotiate")]
    [InlineData(2982, "Walk")]
    public async Task The_credit_agent_judges_synthesised_default_chance_in_basis_points(int basisPoints, string plan)
    {
        // 2982 bp is the 29.82% the synthesised loan on a $280 000 offer produces -- CreditTests shows
        // where that comes from, and that it measures nothing.
        var engine = Cred.CreditagentEngineFactory.Create();
        engine.AssertAll([
            new Cred.DesireFact("k", "Decided"),
            new Cred.ChanceFact("k", basisPoints),
            new Cred.ToleranceFact("k", 1000),
            new Cred.CeilingFact("k", 2000),
        ]);

        Assert.Equal([plan], await Rows(engine.Intentions("k"), row => row.Plan));
    }

    [Fact]
    public async Task The_three_agents_can_disagree_about_the_same_house()
    {
        // THIS IS THE DESIGN, NOT A DEFECT. On one purchase, valuation sees a fair price, condition sees
        // a repair bill to haggle over, and credit sees a borrower stretched too far. Each is right about
        // its own question. An architecture that averaged them into one number would throw away exactly
        // the information that makes the decision hard.
        var valuation = Valuing(80);

        var condition = Cond.ConditionagentEngineFactory.Create();
        condition.AssertAll([
            new Cond.DesireFact("c", "Decided"), new Cond.ExposureFact("c", 18),
            new Cond.ToleranceFact("c", 10), new Cond.CeilingFact("c", 40),
        ]);

        var credit = Cred.CreditagentEngineFactory.Create();
        credit.AssertAll([
            new Cred.DesireFact("k", "Decided"), new Cred.ChanceFact("k", 2982),
            new Cred.ToleranceFact("k", 1000), new Cred.CeilingFact("k", 2000),
        ]);

        Assert.Equal(["Buy"], await Rows(valuation.Intentions("v"), row => row.Plan));
        Assert.Equal(["Negotiate"], await Rows(condition.Intentions("c"), row => row.Plan));
        Assert.Equal(["Walk"], await Rows(credit.Intentions("k"), row => row.Plan));
    }

    // ── the risk theory's engine ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task An_assessed_offer_within_tolerance_is_acceptable()
    {
        var engine = Rsk.RiskEngineFactory.Create();
        engine.AssertAll([new Rsk.AssessedFact("v", "o1"), new Rsk.WithinFact("v", "o1")]);

        Assert.Equal(["o1"], await Rows(engine.AcceptableOffers("v"), offer => offer));
    }

    [Fact]
    public async Task An_agent_calling_one_offer_acceptable_and_ruinous_is_contradicted()
    {
        var engine = Rsk.RiskEngineFactory.Create();
        engine.AssertAll([
            new Rsk.AssessedFact("v", "o1"), new Rsk.WithinFact("v", "o1"), new Rsk.RuinousFact("v", "o1"),
        ]);

        Assert.Equal(["v"], await Rows(engine.Contradicted(), who => who));
    }

    [Fact]
    public async Task A_coherent_agent_is_not()
    {
        // NON-VACUOUS: the test above shows contradictions are reported when they exist.
        var engine = Rsk.RiskEngineFactory.Create();
        engine.AssertAll([new Rsk.AssessedFact("v", "o1"), new Rsk.WithinFact("v", "o1")]);

        Assert.Empty(await Rows(engine.Contradicted(), who => who));
    }

    // ── Risk.hp, theorem by theorem: each one proved by the kernel while the test runs ──

    [Fact]
    public void Theorem_an_assessed_offer_within_tolerance_is_acceptable() => Theorem.Proved(RiskProofs.AnAssessedOfferWithinToleranceIsAcceptable());

    [Fact]
    public void Theorem_an_acceptable_offer_is_not_also_ruinous() => Theorem.Proved(RiskProofs.AnAcceptableOfferIsNotAlsoRuinous());

    [Fact]
    public void Theorem_belief_composes() => Theorem.Proved(RiskProofs.BeliefComposes());

    [Fact]
    public void Theorem_what_is_desired_is_intended() => Theorem.Proved(RiskProofs.WhatIsDesiredIsIntended());

    [Fact]
    public void Theorem_a_seeded_desire_is_intended() => Theorem.Proved(RiskProofs.ASeededDesireIsIntended());

    [Fact]
    public void Every_theorem_in_Risk_hp_has_a_test_above()
    {
        // A THEOREM ADDED TO THE FILE WITHOUT A TEST HERE FAILS THIS, so the list above stays whole.
        string[] tested =
        [
            "an_assessed_offer_within_tolerance_is_acceptable",
            "an_acceptable_offer_is_not_also_ruinous",
            "belief_composes",
            "what_is_desired_is_intended",
            "a_seeded_desire_is_intended",
        ];

        Assert.Equal(tested.Select(Theorem.Method), Theorem.Of(typeof(RiskProofs)));
    }
}
