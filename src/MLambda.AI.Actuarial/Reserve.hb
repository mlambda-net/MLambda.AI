-- Reserve.hb — how much to hold back against the purchase going wrong, and when that has no answer.
--
-- THE AGGREGATE SHORTFALL IS TREATED AS CLAIMS against an insurer, and the classical ruin model does the
-- rest. The "premium" is what the buyer can put aside each year; the "claims" are the repairs and losses
-- the agents predicted, arriving at some rate with some mean size. Cramér and Lundberg worked out when
-- such a fund survives, and this file spends their answer from `Prelude.Fields`.
--
-- TWO THINGS THIS FILE INSISTS ON:
--
-- A SIMULATION CAN ONLY EVER SAY RUIN DID NOT HAPPEN YET. A surplus that survives a finite horizon proves
-- nothing about the infinite one. The Lundberg bound is the answer to the real question -- the chance of
-- EVER running dry -- and it is what the reserve is sized against.
--
-- AND THE SAFETY LOADING IS A PRECONDITION, NOT A DIAGNOSTIC. If the premium does not exceed the expected
-- claims, ruin is CERTAIN and the adjustment coefficient is zero. The formula below would then divide by
-- zero and return infinity -- a number, and a meaningless one. The adjudicator checks `loadingOf` first and
-- refuses to report a reserve at all, rather than printing something comfortable or something infinite.
--
-- THE FORMULAS, as `Prelude/Fields.hb` states them, for claims of mean size m arriving at rate λ against a
-- premium c:
--
--   safety loading   θ = c / (λ · m) − 1
--   adjustment       R = max(1/m − λ/c, 0)
--   Lundberg bound   ψ(u) ≤ exp(−R · u)       -- the chance a fund starting at u is ever ruined
--
-- so the smallest reserve u with ψ(u) ≤ ε is ln(1/ε) / R.
open Prelude.Fields

fn loadingOf(premium, rate, claimMean)          ↦ safetyLoading(premium, rate, claimMean)
fn adjustmentOf(premium, rate, claimMean)       ↦ adjustment(premium, rate, claimMean)
fn ruinBound(surplus, premium, rate, claimMean) ↦ lundberg(surplus, premium, rate, claimMean)

-- THE RESERVE FOR A TOLERATED CHANCE OF RUIN ε. Meaningful ONLY when the loading is positive; see above.
fn reserveFor(premium, rate, claimMean, eps) ↦ ln(1 / eps) / adjustment(premium, rate, claimMean)

-- One year of the fund: what it started with, plus a year of premium, less a claim.
fn afterOneYear(surplus, premium, claim) ↦ surplusAfter(surplus, premium, 1, claim)
