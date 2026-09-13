// ConditionTests.cs — obsolescence, priced by the market, and the confound that shaped the model.
//
// EVERY EXPECTED NUMBER WAS COMPUTED INDEPENDENTLY FIRST, by exact least squares in a separate script.
// And two of these tests pin a fact ABOUT THE DATA rather than about the code -- that condition rises
// with age, and that pooling across ages understates what a condition point is worth -- because the
// design of Condition.hb depends on both, and the day the corpus changes they should be re-checked.
namespace MLambda.AI.Actuarial.Tests;

using MLambda.AI.Actuarial;
using MLambda.Hilbert.Runtime;

public class ConditionTests
{
    private static readonly Lazy<House> Corpus = new(() => new House("data/houses.csv"));

    /// <summary>[quality, living, age, condition, 1] over the rows the filter keeps.</summary>
    private static (Matrix Design, Vector Prices) Band(Func<double, bool> ageIn)
    {
        var house = Corpus.Value;
        var rows = Enumerable.Range(0, house.Rows).Where(row => ageIn(house.HouseAge.Values[row])).ToList();
        var design = new double[rows.Count, 5];
        var prices = new double[rows.Count];

        for (var i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            design[i, 0] = house.Quality.Values[row];
            design[i, 1] = house.Living.Values[row];
            design[i, 2] = house.HouseAge.Values[row];
            design[i, 3] = house.Condition.Values[row];
            design[i, 4] = 1d;
            prices[i] = house.Price.Values[row];
        }

        return (design, prices);
    }

    private static double PerPoint(Func<double, bool> ageIn)
    {
        var (design, prices) = Band(ageIn);

        return Condition.ConditionModel(design, prices).Values[3];
    }

    private static readonly Lazy<double> Pooled = new(() => PerPoint(_ => true));
    private static readonly Lazy<double> Young = new(() => PerPoint(age => age < 30));
    private static readonly Lazy<double> Old = new(() => PerPoint(age => age >= 30));

    [Fact]
    public void Older_houses_are_rated_in_better_condition_which_is_why_age_is_not_used()
    {
        // THE CONFOUND, PINNED. Houses under ten years average 5.01; over sixty, 6.15. New houses get
        // a default "average", and old houses still on the market are the ones that were kept up. A
        // deterioration-from-age model would read this backwards.
        var house = Corpus.Value;
        double MeanCondition(Func<double, bool> ageIn) =>
            Enumerable.Range(0, house.Rows).Where(row => ageIn(house.HouseAge.Values[row]))
                .Average(row => house.Condition.Values[row]);

        Assert.Equal(5.01d, MeanCondition(age => age < 10), 2);
        Assert.Equal(6.15d, MeanCondition(age => age >= 60), 2);
    }

    [Fact]
    public void Within_an_age_band_a_condition_point_is_worth_about_8700_dollars()
    {
        // Computed independently: $8 613.54 under thirty, $8 761.56 at thirty and over. The two bands
        // agree with each other to within two percent, which is the evidence either is trustworthy.
        //
        // AN EARLIER VERSION OF THIS TEST SAID $9 243.34, AND THE TEST WAS WRONG, NOT THE CODE. The
        // script that produced it split the bands as 0 ≤ age < 30, which silently dropped the one house
        // with a negative age; this file says age < 30, which keeps it. Hilbert agreed with the second
        // rule to the cent. See the next test for the house.
        Assert.Equal(8613.54d, Young.Value, 0);
        Assert.Equal(8761.56d, Old.Value, 0);
    }

    [Fact]
    public void One_house_was_sold_the_year_before_it_was_built_and_it_moves_the_estimate()
    {
        // A FACT ABOUT THE DATA, PINNED. Built 2008, sold 2007: age −1. Quality 10, 5 095 square feet,
        // $183 850 -- a partial sale of an unfinished house, and the best-known outlier in Ames. Leaving
        // it out moves the under-thirty value of a condition point from $8 613.54 to $9 243.34: $630 from
        // one row out of 1 267. It stays in, because hiding it would be the worse choice.
        var house = Corpus.Value;
        var odd = Enumerable.Range(0, house.Rows).Where(row => house.HouseAge.Values[row] < 0).ToList();

        var only = Assert.Single(odd);
        Assert.Equal(2008d, house.Built.Values[only]);
        Assert.Equal(2007d, house.Sold.Values[only]);
        Assert.Equal(5095d, house.Living.Values[only]);
        Assert.Equal(183850d, house.Price.Values[only]);

        Assert.Equal(9243.34d, PerPoint(age => age is >= 0 and < 30), 0);
    }

    [Fact]
    public void Pooled_across_all_ages_it_looks_like_barely_half_that()
    {
        // $5 029.80 -- lower than EITHER band. That cannot be a property of condition; it is the age
        // confound averaging the two groups together. Which is why the host fits within the house's own
        // band, and why a risk estimate that used the pooled figure would be optimistic.
        Assert.Equal(5029.80d, Pooled.Value, 0);
        Assert.True(Pooled.Value < Young.Value && Pooled.Value < Old.Value,
            $"pooled {Pooled.Value:F0} should sit below both bands ({Young.Value:F0}, {Old.Value:F0})");
    }

    [Theory]
    [InlineData(3d, 2d)]
    [InlineData(4d, 1d)]
    [InlineData(5d, 0d)]
    [InlineData(8d, 0d)]
    public void Only_a_house_below_typical_owes_anything(double rating, double points)
    {
        // Five is the corpus's mode and median. A house above it is not owed a refund.
        Assert.Equal(points, Condition.PointsBelowTypical(rating));
    }

    [Fact]
    public void Exposure_is_points_below_typical_times_what_a_point_is_worth()
    {
        Assert.Equal(18000d, Condition.RepairExposure(3d, 9000d));
        Assert.Equal(0d, Condition.RepairExposure(7d, 9000d));
    }
}
