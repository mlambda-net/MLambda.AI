-- Credit.hb — default risk, computed from LOAN DATA THAT DOES NOT EXIST.
--
-- ════════════════════════════════════════════════════════════════════════════════════════════════
-- READ THIS BEFORE READING THE FORMULAS.
--
-- `houses.csv` RECORDS SALES, NOT MORTGAGES. It has no loan-to-value, no interest rate, no borrower,
-- and no default outcomes. So EVERY INPUT to this model is invented here, and so is every coefficient:
--
--   * the loan-to-value and the rate are SYNTHESISED, deterministically, from the sale price and a
--     fixed seed;
--   * the hazard coefficients are DECLARED, not fitted -- there are no defaults to fit them to;
--   * the recovery on foreclosure is a DECLARED 75% of the sale price.
--
-- SO THIS AGENT'S NUMBER IS A DEMONSTRATION OF A METHOD, NOT A MEASUREMENT OF ANYTHING. It shows what
-- a proportional-hazards model does with loan attributes. It says nothing about any real borrower, and
-- no output of this repository may present it as if it did.
--
-- DETERMINISTIC ON PURPOSE. A fixed seed makes the invented loan reproducible and therefore testable,
-- and means nobody can mistake variation between runs for signal.
--
-- This warning is repeated in `docs/actuarial/credit.md` and printed by `Program.cs` every time this
-- agent reports. Three places, because one is too easy to miss.
-- ════════════════════════════════════════════════════════════════════════════════════════════════
--
-- Contrast `Valuation.hb` and `Condition.hb`, which use only what the corpus measured.
open Prelude.Sequences

-- ── the synthesis: arithmetic, not measurement ─────────────────────────────────────────────────

-- A NUMBER IN [0, 1) THAT DEPENDS ONLY ON PRICE AND SEED: the fractional part of a fixed affine map.
-- Not random, and not pretending to be.
--
-- A `def`, NOT A `fn`, BECAUSE THE FUNCTIONS BELOW CALL IT. Written as `fn scatter01` it is accepted
-- without a word, and then the generated C# fails to compile: the emitter routes the call to
-- `System.Math.Scatter01` in the double overload and `Tensor.Scatter01` in the tensor ones, treating
-- a sibling `fn` as if it were a built-in like `floor`. A helper that other definitions use is a `def`,
-- which is what the Prelude does throughout -- see docs/hilbert/06-diagnostics.md.
def scatter01(price, seed) ≔ (price · 0.000037 + seed · 0.61) − floor(price · 0.000037 + seed · 0.61)

-- A loan-to-value between 60% and 95%, and a rate between 3% and 8%.
fn loanToValueOf(price, seed) ↦ 0.60 + 0.35 · scatter01(price, seed)
fn rateOf(price, seed)        ↦ 0.03 + 0.05 · scatter01(price, seed + 1)

-- ── the hazard: a real method on invented inputs ───────────────────────────────────────────────

-- COX'S PROPORTIONAL HAZARDS, from `Prelude.Sequences`: `exp(x · b)`, clamped. `x` is one row per loan,
-- [loan-to-value, rate, 1]; `b` is the DECLARED coefficients. What comes back is a hazard RATIO.
fn defaultHazard(x, b) ↦ hazardOf(x, b)

-- The chance of at least one default event when the hazard is `h`: the Poisson complement.
fn defaultChance(h) ↦ 1 − exp(0 − h)

-- WHAT IS LOST IF IT DEFAULTS: the loan, less a DECLARED recovery of 75% of the price.
fn lossGivenDefault(price, ltv) ↦ price · max(ltv − 0.75, 0)
