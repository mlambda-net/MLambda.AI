-- Perceptron.hb — a two-layer network, declared: the architecture, the step, and what it answers.
--
-- WHAT IS NEW SINCE `Line`: A MODEL HAS WEIGHTS, AND A `fn` DOES NOT. `Line.hb` held functions from
-- data to an answer. This holds state. The constructor takes the architecture and `train` takes the
-- data, so a caller may step the network, watch it, and ask it anything at any point in the fit —
-- which is the whole difference between a model and a formula that happens to fit.
--
-- ONE GRADIENT STEP PER CALL. `stepFirst` and `stepSecond` each compute one descent step, and
-- because both read the weights this call was ENTERED with, the pair is one step on both layers.
-- `mlpFirst`/`mlpSecond` would each run the ENTIRE fit, and a caller naming both would run it twice.
--
-- THE SCALE ON THE DRAW IS NOT DECORATION. A rectified network started from a standard normal and
-- stepped at any useful rate saturates on the first turns, every hidden unit dies, and the object
-- answers exactly zero forever. Half a standard deviation keeps them alive.
open Prelude.Networks
open Prelude.LinearAlgebra

model Perceptron(inputs, hidden, seed) : Learner {

  weight first  : Tensor[inputs, hidden] from gaussian(seed) · 0.5
  weight second : Tensor[hidden, 1]      from gaussian(seed) · 0.5

  def train(x, y, rate) {
    first  ≔ stepFirst(rate, x, y, first, second)
    second ≔ stepSecond(rate, x, y, first, second)
  }

  def eval(x) ≔ outputs(x, first, second)

  -- THE LOSS LIVES HERE, not in the caller. Mean squared error is mathematics, and a test that
  -- computed it in C# would be the second copy of a formula -- the one that drifts.
  --
  -- NOT `mean(…)`. The network answers a MATRIX -- one row per example, one column per output --
  -- and `Statistics.mean` is `total(x) / count(x)` with `total` an `einsum("i->", …)` over a
  -- VECTOR. Handed rank 2 it refuses at run time. So the sum runs over both indices, and dividing
  -- by the row count gives the mean because there is exactly one output column.
  def loss(x, y) ≔ einsum("ij->", (outputs(x, first, second) − y) · (outputs(x, first, second) − y)) / rows(x)
}
