-- Line.hb — fitting a straight line, and watching the error fall.
--
-- THE FIRST `.hb` IN THIS REPOSITORY. Everything before this was `.hs` and `.hp` — theories and
-- proofs, which reason about relations. This dialect is the other half: mathematics over tensors
-- and reals, where a formula IS a computation.
--
-- ONE `fn` BECOMES MANY METHODS. The compiler emits each formula once for reals, once for complex
-- numbers, once for every tensor rank with scalar broadcasting, and once more as an ONNX graph an
-- executor can run. Nothing below says any of that; it is what declaring a formula buys.
--
-- ORDINARY LEAST SQUARES IS `Multivariate`'S, NOT `Regression`'S. Regression.hb's own header says
-- it holds "the linear models that are NOT least squares" — logistic, lasso, Poisson, SVM. The
-- plain fit is `coefficients(x, y) ≔ solve(gram(x), moment(x, y))`, one module over.
--
-- AND THIS IS THE ONE PRELUDE MODULE AN L1 SAMPLE OPENS. A first machine-learning sample cannot
-- avoid the Prelude, and rewriting least squares here would teach least squares — which is not the
-- subject. The subject is that a model is a formula you can read.
open Prelude.Multivariate
open Prelude.Statistics

-- ── the arithmetic, with nothing borrowed ──────────────────────────────────────────────────────

fn slopeOf(rise, run) ↦ rise / run

fn errorOf(predicted, actual) ↦ (predicted − actual) · (predicted − actual)

-- ── and the fit, with everything borrowed ──────────────────────────────────────────────────────

fn lineOf(x, y)      ↦ coefficients(x, y)
fn predictWith(b, x) ↦ apply(x, b)
fn fittedTo(x, y)    ↦ fitted(x, y)

-- A NUMBER WITHOUT A SPREAD INVITES FALSE CONFIDENCE. The fit says where the line is; this says
-- how much the data disagrees with it.
fn spreadOf(x, y)    ↦ std(residuals(x, y))
