-- House.hb — the Ames corpus: 2 930 sales and 82 columns, of which this frame keeps eleven.
--
-- A FRAME IS A SCHEMA, NOT A LOADER. Each line below names a column in `houses.csv` and what it
-- becomes; the compiler writes the reading, the blank-filling and the derived columns. The C# that
-- uses a house never parses a line of CSV.
--
-- ZONE'S THIRD ARM WANTS A GUARD AND CANNOT HAVE ONE. What it means to say is "any zoning starting
-- with C is 3", but the frame emitter's word-match pass refuses every guarded arm. `"C (all)"` is
-- written instead, a bare literal, because it is the only `MS Zoning` value in THIS corpus starting
-- with "C" -- checked against the full set: A (agr), C (all), FV, I (all), RH, RL, RM.
--
-- THAT EQUIVALENCE IS A FACT ABOUT THE DATA, NOT ABOUT THE DECLARATION. The day a row reads
-- "C (comm)", this line keeps silently meaning "exactly C (all)". Whoever gives the emitter guarded
-- arms should replace it, not merely notice the corpus changed.
--
-- AND THE REMODEL YEAR IS FLOORED AT 1950, which nothing in the column says. Of the 632 houses built
-- before 1950, 339 record a remodel in exactly 1950 -- the dataset's placeholder, not a building
-- boom. So `sinceWork` UNDERSTATES how long an old house has gone without work, and `Condition.hb`
-- says how it copes. Measured before anything was built on it.
model House(path) : Frame(path) {
  quality   : Scalar from "Overall Qual"
  condition : Scalar from "Overall Cond"
  living    : Scalar from "Gr Liv Area"
  frontage  : Scalar from "Lot Frontage"
  built     : Scalar from "Year Built"
  remodel   : Scalar from "Year Remod/Add"
  sold      : Scalar from "Yr Sold"
  price     : Scalar from "SalePrice"

  zoning    : Text from "MS Zoning"

  def houseAge  ≔ sold − built
  def sinceWork ≔ sold − remodel

  def zone ≔ match zoning {
    when "RL" ↦ 1
    when "RM" | "RH" ↦ 2
    when "C (all)" ↦ 3
    other ↦ 0
  }

  -- 490 of 2 930 rows have no frontage. The median is the least surprising stand-in, and the fill
  -- is declared here so no caller has to remember it.
  fill frontage with median
}
