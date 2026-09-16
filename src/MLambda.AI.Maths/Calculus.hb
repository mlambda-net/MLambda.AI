-- Calculus.hb — differentiation is rewriting, and it happens before you run anything.
--
-- `∂ u / ∂ x` IS AN ORDINARY TERM. It is not syntax the compiler treats specially: it is a term
-- that thirteen laws in `Prelude.Calculus` know how to remove — self, constant, sum, difference,
-- negation, product, quotient, power, sine, cosine, exponential, logarithm and root — each one
-- oriented so that applying it deletes a `∂`. When the last `∂` is gone, what remains is
-- arithmetic, and that arithmetic is what the compiler emits.
--
-- SO THE DERIVATIVE COSTS NOTHING AT RUN TIME. `slope` contains no derivative in the generated
-- C#; it contains `2 * x`. Nothing differentiates while the program is running.
--
-- AND THERE IS NO GENERAL CHAIN RULE. `chain` works because `sine` and `power` between them
-- reach every `∂` in it. A head no law names is a different story, and it is the next section.
open Prelude.Calculus

-- ── derivatives the laws can take ──────────────────────────────────────────────────────────────

fn slope(x) ↦ ∂ x² / ∂ x
fn chain(x) ↦ ∂ sin(x²) / ∂ x

-- ── and the one that cannot be taken ───────────────────────────────────────────────────────────
--
-- THERE IS NO LAW FOR TAN. The thirteen rules cover sine and cosine; none of them names `tan`, so
-- `∂ tan(x) / ∂ x` survives reduction with its `∂` intact.
--
-- WHAT HAPPENS NEXT IS THE THING TO REMEMBER. A doorway that still contains a derivative is not
-- emitted, and no error is printed. `tangent` compiles, the build is green, and the method is
-- simply not in the generated class. If a method you expected is missing, look for a derivative
-- that could not be taken — nothing else will tell you.
fn tangent(x) ↦ ∂ tan(x) / ∂ x
