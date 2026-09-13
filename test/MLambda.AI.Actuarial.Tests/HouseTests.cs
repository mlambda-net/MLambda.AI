// HouseTests.cs — the frame reads the corpus, and the numbers were measured another way first.
//
// EVERY EXPECTED NUMBER BELOW WAS COUNTED FROM houses.csv WITH A SEPARATE SCRIPT before this file was
// written -- not read back from what the frame produced. A test that asserts whatever the code happens
// to answer is a snapshot, and a snapshot of a bug passes.
namespace MLambda.AI.Actuarial.Tests;

using MLambda.AI.Actuarial;

public class HouseTests
{
    // THE CORPUS IS READ ONCE. Parsing 2 930 rows per test would make the suite slow for no gain.
    private static readonly Lazy<House> Corpus = new(() => new House("data/houses.csv"));

    [Fact]
    public void The_corpus_has_every_sale()
    {
        Assert.Equal(2930, Corpus.Value.Rows);
    }

    [Fact]
    public void The_columns_span_the_ranges_the_raw_file_does()
    {
        var house = Corpus.Value;

        Assert.Equal(12789d, house.Price.Values.Min());
        Assert.Equal(755000d, house.Price.Values.Max());
        Assert.Equal(1872d, house.Built.Values.Min());
        Assert.Equal(1d, house.Quality.Values.Min());
        Assert.Equal(10d, house.Quality.Values.Max());
    }

    [Fact]
    public void Every_missing_frontage_was_filled()
    {
        // 490 rows have no Lot Frontage in the raw file. `fill frontage with median` means none do
        // after the frame has read them.
        Assert.DoesNotContain(Corpus.Value.Frontage.Values, double.IsNaN);
        Assert.Equal(2930, Corpus.Value.Frontage.Values.Count);
    }

    [Fact]
    public void House_age_is_derived_by_the_frame_not_computed_here()
    {
        // `def houseAge ≔ sold − built`. Checked row by row against the two columns it came from.
        var house = Corpus.Value;

        for (var row = 0; row < house.Rows; row++)
        {
            Assert.Equal(house.Sold.Values[row] - house.Built.Values[row], house.HouseAge.Values[row]);
        }
    }

    [Fact]
    public void Zoning_is_coded_and_commercial_is_the_rarest_named_zone()
    {
        // RL → 1; RM and RH → 2; C (all) → 3; everything else → 0. Counted from the raw file:
        // RL 2273, RM 462, RH 27, C (all) 25, and 143 in FV, I (all) and A (agr).
        var zones = Corpus.Value.Zone.Values;

        Assert.Equal(2273, zones.Count(zone => zone == 1d));
        Assert.Equal(489, zones.Count(zone => zone == 2d));
        Assert.Equal(25, zones.Count(zone => zone == 3d));
        Assert.Equal(143, zones.Count(zone => zone == 0d));
    }

    [Fact]
    public void The_remodel_year_is_floored_at_1950_and_the_frame_does_not_hide_it()
    {
        // A DATA QUIRK, PINNED SO NOBODY BUILDS ON THE COLUMN AS IF IT WERE CLEAN. Nothing in Ames
        // was remodelled before 1950, and 339 of the 632 pre-1950 houses record exactly 1950 -- the
        // dataset's placeholder. `sinceWork` understates how long those houses have gone untouched.
        var house = Corpus.Value;
        var oldHouses = Enumerable.Range(0, house.Rows).Where(row => house.Built.Values[row] < 1950).ToList();

        Assert.Equal(1950d, house.Remodel.Values.Min());
        Assert.Equal(632, oldHouses.Count);
        Assert.Equal(339, oldHouses.Count(row => house.Remodel.Values[row] == 1950d));
    }
}
