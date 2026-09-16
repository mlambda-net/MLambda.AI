-- Algebra.hb — the laws are the knowledge, and they say so.
--
-- THE COMPILER KNOWS NO ALGEBRA. Not one identity, not one derivative. `sin(x)² + cos(x)² = 1` is
-- true below because `Prelude.Trigonometry` states it as a law named `pythagorean` and this file
-- opened it. Close that `open` and `energy` stops folding.
--
-- A FOLD IS NOT A FASTER ROUTE TO THE SAME NUMBER. In doubles, sin(x)² + cos(x)² is not 1 — it is
-- 0.9999999999999999 at x = 0.009. Folded, `energy` returns the literal 1, because no sine is ever
-- taken. The law did not optimise the computation; it removed it.
open Prelude.Arithmetic
open Prelude.Trigonometry

-- ── the identity that disappears ───────────────────────────────────────────────────────────────

fn energy(x) ↦ sin(x)² + cos(x)²

-- ── and an ordinary formula, for the CAS to chew on ────────────────────────────────────────────

fn scaled(x) ↦ x · 1 + 0
