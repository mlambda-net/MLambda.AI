-- Maths.hs — the differentiation rules the CAS spends, restated as a theory the kernel can read.
--
-- THIS IS `Prelude/Calculus.hb` RE-READ AS LAWS. Calculus.hb opened that prelude and let thirteen
-- rewrite rules turn `∂ x² / ∂ x` into `2x`. Nothing checked them; they were assumed, the way a
-- textbook's inside cover is assumed. Here the rules are a theory, and the claim that they give
-- `2x` is a theorem the build refuses to compile without.
--
-- THE GUARD BECOMES STRUCTURE. The `.hb` rule is `∂ u / ∂ x = 0 where x ∉ u` — a side condition on
-- a rewrite. A `.hs` law has no `where`, and needs none if the term algebra says the same thing:
-- `var` IS the variable and `konst(c)` is anything that is not it, so "x does not occur in u" is
-- `u = konst(c)`, and matching checks it.
--
-- ONE LINE, BECAUSE A DECLARATION CANNOT WRAP. Newlines are tokens: a `sort` written down the page
-- is read as ending after its first constructor, and so is a `law`.
sort Fn = var | konst(Double) | plus(Fn, Fn) | times(Fn, Fn)

theory Maths (Fn, Double)
{
  -- ── the two rules this proof needs ───────────────────────────────────────────────────────────

  law d_var   = ⊤ ⇒ d(var) = konst(1)
  law d_konst = ∀ c, ⊤ ⇒ d(konst(c)) = konst(0)
  law d_times = ∀ u v, ⊤ ⇒ d(times(u, v)) = plus(times(d(u), v), times(u, d(v)))

  -- ── and the arithmetic that tidies up after them ─────────────────────────────────────────────

  law unit_left  = ∀ u, ⊤ ⇒ times(konst(1), u) = u
  law unit_right = ∀ u, ⊤ ⇒ times(u, konst(1)) = u
  law double     = ∀ u, ⊤ ⇒ plus(u, u) = times(konst(2), u)
}
