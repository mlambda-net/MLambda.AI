// CreditTests.cs — a real method on invented inputs, and tests that say which part is which.
//
// NOTHING HERE MEASURES A BORROWER. Credit.hb synthesises the loan and declares the coefficients, and
// these tests check two things only: that the synthesis is DETERMINISTIC, and that the arithmetic is
// what the formulas say. Every expected number was computed independently first.
namespace MLambda.AI.Actuarial.Tests;

using MLambda.AI.Actuarial;
using MLambda.Hilbert.Runtime;

public class CreditTests
{
    private const double Seed = 42d;

    /// <summary>The DECLARED hazard coefficients: [loan-to-value, rate, 1]. Not fitted -- there is nothing to fit.</summary>
    private static readonly Vector Declared = new double[] { 4d, 20d, -6d };

    [Fact]
    public void The_synthesised_loan_is_reproducible()
    {
        // A FIXED SEED IS WHAT MAKES AN INVENTED NUMBER TESTABLE. If this ever varies between runs, the
        // synthesis has become a random-number generator wearing a model's clothes.
        Assert.Equal(Credit.LoanToValueOf(231420.47d, Seed), Credit.LoanToValueOf(231420.47d, Seed));
        Assert.Equal(Credit.RateOf(231420.47d, Seed), Credit.RateOf(231420.47d, Seed));
    }

    [Fact]
    public void And_it_depends_on_the_price()
    {
        // NON-VACUOUS: the test above would pass for a synthesis that returned a constant.
        Assert.NotEqual(Credit.LoanToValueOf(200000d, Seed), Credit.LoanToValueOf(280000d, Seed));
    }

    [Theory]
    [InlineData(200000d, 0.607000d, 0.061500d)]
    [InlineData(231420.47d, 0.663895d, 0.069628d)]
    [InlineData(280000d, 0.943000d, 0.059500d)]
    public void The_synthesis_is_the_arithmetic_it_declares(double price, double ltv, double rate)
    {
        // Computed independently from the same formulas: the fractional part of price · 0.000037 +
        // seed · 0.61, stretched to [0.60, 0.95) for loan-to-value and to [0.03, 0.08) for the rate.
        Assert.Equal(ltv, Credit.LoanToValueOf(price, Seed), 6);
        Assert.Equal(rate, Credit.RateOf(price, Seed), 6);
    }

    [Fact]
    public void Every_synthesised_loan_stays_inside_its_declared_range()
    {
        foreach (var price in new[] { 12789d, 150000d, 231420.47d, 755000d })
        {
            Assert.InRange(Credit.LoanToValueOf(price, Seed), 0.60d, 0.95d);
            Assert.InRange(Credit.RateOf(price, Seed), 0.03d, 0.08d);
        }
    }

    [Theory]
    [InlineData(0.607000d, 0.061500d, 0.096135d, 0.091659d)]
    [InlineData(0.943000d, 0.059500d, 0.354162d, 0.298239d)]
    public void The_hazard_is_proportional_hazards_on_the_declared_coefficients(double ltv, double rate, double hazard, double chance)
    {
        // exp(4 · ltv + 20 · rate − 6), then the chance of at least one event, 1 − exp(−h).
        Matrix loan = new double[,] { { ltv, rate, 1d } };

        var h = Credit.DefaultHazard((Tensor)loan, (Tensor)Declared).Values[0];

        Assert.Equal(hazard, h, 5);
        Assert.Equal(chance, Credit.DefaultChance(h), 5);
    }

    [Fact]
    public void A_higher_loan_to_value_carries_a_higher_hazard()
    {
        // The one qualitative claim the declared coefficients make: borrowing more of the price is
        // riskier. It is a property of the numbers chosen, and the test says so rather than implying
        // the corpus showed it.
        Matrix low = new double[,] { { 0.60d, 0.05d, 1d } };
        Matrix high = new double[,] { { 0.95d, 0.05d, 1d } };

        Assert.True(
            Credit.DefaultHazard((Tensor)high, (Tensor)Declared).Values[0] >
            Credit.DefaultHazard((Tensor)low, (Tensor)Declared).Values[0]);
    }

    [Theory]
    [InlineData(280000d, 0.943d, 54040d)]
    [InlineData(200000d, 0.607d, 0d)]
    public void Nothing_is_lost_until_the_loan_exceeds_the_declared_recovery(double price, double ltv, double loss)
    {
        // Recovery is a DECLARED 75% of the price. Borrow less than that and a default costs nothing;
        // borrow 94.3% of $280 000 and it costs the 19.3% above the line: $54 040.
        Assert.Equal(loss, Credit.LossGivenDefault(price, ltv), 2);
    }
}
