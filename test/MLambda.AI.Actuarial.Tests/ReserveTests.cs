// ReserveTests.cs — a reserve sized against ever running dry, and the case where it refuses to exist.
//
// EVERY EXPECTED NUMBER WAS COMPUTED INDEPENDENTLY FIRST from the Cramér–Lundberg formulas. And one of
// them is checked two ways: the reserve for a 5% chance of ruin, fed back into the Lundberg bound, must
// give 5% back -- which catches a reserve that is the right size for the wrong question.
namespace MLambda.AI.Actuarial.Tests;

using MLambda.AI.Actuarial;

public class ReserveTests
{
    // The fixture: $12 000 a year put aside, against claims arriving twice every four years (rate 0.5)
    // with a mean size of $18 000. Expected claims are $9 000 a year, so the premium beats them.
    private const double Premium = 12000d;
    private const double Rate = 0.5d;
    private const double MeanClaim = 18000d;

    [Fact]
    public void A_premium_that_beats_expected_claims_has_a_positive_loading()
    {
        // θ = 12 000 / (0.5 · 18 000) − 1 = one third.
        Assert.Equal(1d / 3d, Reserve.LoadingOf(Premium, Rate, MeanClaim), 9);
    }

    [Fact]
    public void And_a_positive_adjustment_coefficient()
    {
        // R = 1/18 000 − 0.5/12 000 = 1.388 889 × 10⁻⁵.
        Assert.Equal(1.388889e-5d, Reserve.AdjustmentOf(Premium, Rate, MeanClaim), 11);
    }

    [Fact]
    public void The_reserve_for_a_five_percent_chance_of_ruin()
    {
        // u = ln(1/0.05) / R = $215 692.72.
        Assert.Equal(215692.72d, Reserve.ReserveFor(Premium, Rate, MeanClaim, 0.05d), 1);
    }

    [Fact]
    public void Fed_back_into_the_bound_that_reserve_gives_five_percent()
    {
        // THE SAME ANSWER FROM THE OTHER DIRECTION. A reserve that did not reproduce its own target
        // would be sized for some other question, and every test above could still pass.
        var reserve = Reserve.ReserveFor(Premium, Rate, MeanClaim, 0.05d);

        Assert.Equal(0.05d, Reserve.RuinBound(reserve, Premium, Rate, MeanClaim), 9);
    }

    [Fact]
    public void More_reserve_means_a_smaller_chance_of_ruin()
    {
        Assert.True(
            Reserve.RuinBound(300000d, Premium, Rate, MeanClaim) < Reserve.RuinBound(100000d, Premium, Rate, MeanClaim));
    }

    [Theory]
    [InlineData(8000d)]
    [InlineData(9000d)]
    public void Below_or_at_expected_claims_ruin_is_certain_and_the_adjustment_is_zero(double premium)
    {
        // THE PRECONDITION. At $8 000 the premium loses to $9 000 of expected claims; at exactly $9 000 it
        // only ties -- and a fund that only breaks even is ruined eventually too. Either way R = 0.
        Assert.True(Reserve.LoadingOf(premium, Rate, MeanClaim) <= 0d);
        Assert.Equal(0d, Reserve.AdjustmentOf(premium, Rate, MeanClaim));
    }

    [Fact]
    public void And_the_reserve_formula_then_answers_infinity_which_is_why_nobody_may_print_it()
    {
        // WHAT THE ADJUDICATOR IS PROTECTING AGAINST. With R = 0 the formula divides by zero and returns
        // a number -- positive infinity. Printed, that reads as "a very large reserve", when the truth is
        // "no reserve is enough". So the caller must check the loading first and refuse.
        Assert.True(double.IsPositiveInfinity(Reserve.ReserveFor(8000d, Rate, MeanClaim, 0.05d)));
    }

    [Fact]
    public void One_year_of_the_fund_is_premium_in_and_claim_out()
    {
        Assert.Equal(25000d, Reserve.AfterOneYear(20000d, Premium, 7000d));
    }
}
