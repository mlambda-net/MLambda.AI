-- Condition.hb — what a house in poor condition will cost, priced by the market that sold it.
--
-- THE RISK THIS OWNS IS OBSOLESCENCE: money the buyer will have to spend to bring the house up to what
-- houses like it usually are.
--
-- IT DOES NOT MODEL DETERIORATION FROM AGE, AND THE REASON IS MEASURED. In this corpus older houses are
-- rated in BETTER condition, not worse: houses under ten years average 5.01 and 98% are rated exactly
-- 5, while houses over sixty average 6.15. The correlation of condition with age is +0.37. That is not
-- old houses being sounder -- new houses get a default "average", and the old houses still being sold
-- are the survivors somebody kept up. A model fitted to that would conclude old houses need fewer
-- repairs, and it would be confidently wrong. So the rating is used as the inspector gave it.
--
-- NO FAILURE DATA IS INVENTED EITHER. There are no repair costs or failure times in `houses.csv`, so a
-- survival curve over components would need a second made-up series. Only `Credit.hb` is allowed to
-- synthesise its inputs, and it says so at length.
--
-- WHAT A CONDITION POINT IS WORTH COMES FROM THE PRICES. Fit price on quality, living area, age AND
-- condition, and the condition coefficient is what the market paid per point with the others held
-- fixed. The host fits it within the house's own AGE BAND, because the same confound bites here: pooled
-- across all 2 930 sales a point is worth $5 030, but $8 614 among houses under thirty and $8 762 among
-- older ones. The bands agree with each other to within two percent and not with the pool -- so the pool
-- is the biased one, and for a RISK estimate understating the cost is optimism, not caution.
--
-- ONE HOUSE MOVES THAT FIRST FIGURE BY $630. It was sold in 2007 and built in 2008 -- an age of −1 --
-- at quality 10 and 5 095 square feet, for $183 850: a partial sale of an unfinished new house, and the
-- best-known outlier in the Ames data. It is kept, not quietly dropped; `ConditionTests` pins it.
open Prelude.Multivariate

-- ── what the market pays per condition point ───────────────────────────────────────────────────

-- The design is [quality, living, age, condition, 1]; the condition coefficient is column 3.
fn conditionModel(x, y) ↦ coefficients(x, y)

-- ── the exposure ───────────────────────────────────────────────────────────────────────────────

-- HOW FAR BELOW TYPICAL. Five is both the mode and the median rating in this corpus -- 1 654 of 2 930
-- houses -- so it is what "usually" means here. A house rated above five owes nothing.
fn pointsBelowTypical(cond) ↦ max(5 − cond, 0)

fn repairExposure(cond, perPoint) ↦ max(5 − cond, 0) · perPoint
