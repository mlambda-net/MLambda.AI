-- Solve.hb — isolating a variable is a thing you teach, not a thing you get.
--
-- A TERM LAW MOVES A SUB-TERM. `x · 1 = x` rewrites part of an expression wherever it matches,
-- which is enough to simplify and enough to differentiate. It is not enough to solve: turning
-- `2x + 3 = 7` into `x = 2` means moving the *equation*, and only a law written with `⇔` can.
--
-- NO PRELUDE SHIPS ONE, AND THAT IS DELIBERATE. Transposition is valid only when what you divide
-- by is not zero, and a rewrite rule has no way to say `a ≠ 0` — the only guard the language has
-- is `x ∉ u`, not-free-in. Declaring the two laws here makes the assumption yours, and visible in
-- the file that relies on it.
--
-- SO `Solve` REFUSES BEFORE IT GUESSES. Ask Algebra.hb's CAS to solve this and it answers
-- `Unsolved` with the closest goal it reached. It does not invent a transposition it was never
-- given.
open Prelude.Arithmetic

model Transposing {
  law sub_add = a + b = c ⇔ a = c − b
  law div_mul = a · b = c ⇔ b = c / a
}
