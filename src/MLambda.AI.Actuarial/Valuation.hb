-- Valuation.hb — is the asking price above what the corpus says the house is worth?
--
-- THE RISK THIS OWNS IS OVERPAYMENT. Fit the corpus's own prices against what a buyer can see —
-- quality, living area, age — and a house has a FAIR VALUE: what 2 930 real sales say a house like it
-- fetched. An offer above that is a premium, and the question is whether the premium is large.
--
-- A NUMBER WITHOUT A SPREAD INVITES FALSE CONFIDENCE, so the spread is the point. The corpus disagrees
-- with itself about houses like this one by some amount -- the standard deviation of what the fit
-- leaves over. An offer twenty thousand above fair value is noise on a spread of forty thousand, and
-- a warning on a spread of five. So the premium is reported IN SPREADS, not in dollars.
--
-- LEAST SQUARES IS `Multivariate`'S, NOT `Regression`'S -- Regression.hb holds the linear models that
-- are NOT least squares. The fit is `coefficients(x, y) ≔ solve(gram(x), moment(x, y))`.
open Prelude.Multivariate
open Prelude.Statistics

-- ── the fit ────────────────────────────────────────────────────────────────────────────────────

fn fairValueCoefficients(x, y) ↦ coefficients(x, y)
fn fairValueOf(b, x)          ↦ apply(x, b)
fn spreadOf(x, y)             ↦ std(residuals(x, y))

-- ── the risk ───────────────────────────────────────────────────────────────────────────────────

-- HOW MANY SPREADS ABOVE FAIR VALUE THE OFFER SITS. Negative is a bargain; zero is exactly what the
-- corpus would expect; two is a premium the corpus rarely paid.
fn excessInSpreads(offer, fair, spread) ↦ (offer − fair) / spread
