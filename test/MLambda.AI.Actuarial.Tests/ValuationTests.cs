// ValuationTests.cs — overpayment, measured against what 2 930 real sales paid.
//
// EVERY EXPECTED NUMBER WAS COMPUTED INDEPENDENTLY FIRST, by solving the normal equations exactly with
// pivoted elimination in a separate script. Hilbert's `coefficients` uses conjugate gradient on a gram
// whose diagonal spans six orders of magnitude, which is exactly the setting where an iterative solve
// can quietly return the wrong answer -- so it was checked against an exact one, and it agrees to four
// decimals on every coefficient.
namespace MLambda.AI.Actuarial.Tests;

using MLambda.AI.Actuarial;
using MLambda.Hilbert.Runtime;

public class ValuationTests
{
    /// <summary>The design: quality, living area, age, and a column of ones for the intercept.</summary>
    private static Matrix Design(House house)
    {
        var rows = new double[house.Rows, 4];

        for (var row = 0; row < house.Rows; row++)
        {
            rows[row, 0] = house.Quality.Values[row];
            rows[row, 1] = house.Living.Values[row];
            rows[row, 2] = house.HouseAge.Values[row];
            rows[row, 3] = 1d;
        }

        return rows;
    }

    // FIT ONCE. Each call over 2 930 rows costs about a second and a half through the generated code.
    private static readonly Lazy<(House House, Tensor Coefficients, double Spread)> Fitted = new(() =>
    {
        var house = new House("data/houses.csv");
        var design = Design(house);

        return (house, Valuation.FairValueCoefficients(design, house.Price), Valuation.SpreadOf(design, house.Price).Values[0]);
    });

    [Fact]
    public void The_fit_matches_an_exact_solve_of_the_normal_equations()
    {
        // Solved independently: $26 009.12 per quality point, $63.07 per square foot, −$495.41 per
        // year of age, and an intercept of −$54 266.41.
        var b = Fitted.Value.Coefficients.Values;

        Assert.Equal(26009.1216d, b[0], 2);
        Assert.Equal(63.0729d, b[1], 3);
        Assert.Equal(-495.4100d, b[2], 2);
        Assert.Equal(-54266.4060d, b[3], 1);
    }

    [Fact]
    public void Quality_and_space_raise_the_price_and_age_lowers_it()
    {
        // The signs a buyer would expect. Tested separately from the exact values above, because a
        // fit could match to four decimals on the wrong design and this is the sanity check on that.
        var b = Fitted.Value.Coefficients.Values;

        Assert.True(b[0] > 0, "quality should raise the price");
        Assert.True(b[1] > 0, "living area should raise the price");
        Assert.True(b[2] < 0, "age should lower the price");
    }

    [Fact]
    public void The_spread_is_the_sample_standard_deviation_of_the_residuals()
    {
        // $39 602.14, which is the n − 1 standard deviation. The population figure is $39 595.38, and
        // the ratio between them is exactly √(2930/2929) -- Prelude.Statistics' `std` divides by n − 1.
        Assert.Equal(39602.14d, Fitted.Value.Spread, 1);
    }

    [Fact]
    public void A_house_has_a_fair_value_from_what_houses_like_it_sold_for()
    {
        // Quality 7, 1 800 square feet, 20 years old: $231 420.47, computed independently.
        Matrix house = new double[,] { { 7d, 1800d, 20d, 1d } };

        var fair = Valuation.FairValueOf(Fitted.Value.Coefficients, house).Values[0];

        Assert.Equal(231420.47d, fair, 0);
    }

    [Fact]
    public void An_offer_above_fair_value_is_measured_in_spreads_not_dollars()
    {
        // $280 000 on a fair value of $231 420.47 is a premium of $48 579.53 -- which sounds large and
        // is 1.2267 spreads. Whether that is a warning depends on the corpus's own disagreement, not
        // on the dollar figure, and that is why the risk is reported this way.
        var excess = Valuation.ExcessInSpreads(280000d, 231420.47d, Fitted.Value.Spread);

        Assert.Equal(1.2267d, excess, 3);
    }

    [Fact]
    public void An_offer_below_fair_value_is_a_negative_premium()
    {
        // NON-VACUOUS: the test above could pass for a function that returned the magnitude.
        Assert.True(Valuation.ExcessInSpreads(200000d, 231420.47d, Fitted.Value.Spread) < 0d);
    }
}
