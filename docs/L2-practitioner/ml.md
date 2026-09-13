# Machine learning, L2 — Perceptron and Classifier

**Source:** [`Perceptron.hb`](../../src/MLambda.AI.ML/Perceptron.hb) ·
[`Classifier.hb`](../../src/MLambda.AI.ML/Classifier.hb)

Two models, and the most instructive thing on this page is a failure.

---

## A `model` has weights, and a `fn` does not

`Line.hb` held **functions** — data in, answer out, nothing remembered. `Perceptron.hb` holds a
**model**:

```
model Perceptron(inputs, hidden, seed) : Learner {

  weight first  : Tensor[inputs, hidden] from gaussian(seed) · 0.5
  weight second : Tensor[hidden, 1]      from gaussian(seed) · 0.5

  def train(x, y, rate) {
    first  ≔ stepFirst(rate, x, y, first, second)
    second ≔ stepSecond(rate, x, y, first, second)
  }

  def eval(x) ≔ outputs(x, first, second)
}
```

The constructor takes the **architecture**; `train` takes the **data**. The weights are fields on one
object, visible between steps as `First` and `Second`, so a caller can step the network, watch it,
and ask it anything at any point in the fit. That is the whole difference between a model and a
formula that happens to fit.

### One gradient step per call

`stepFirst` and `stepSecond` each compute one step of gradient descent, and **both read the weights
this call was entered with** — so the pair is one step on both layers, not a step on the first layer
followed by a step computed from the already-changed one.

### The scale on the draw is not decoration

```
weight first : Tensor[inputs, hidden] from gaussian(seed) · 0.5
```

That `· 0.5` matters. Start a rectified network from a full standard normal, step it at any useful
rate, and it saturates on the first turns: every hidden unit dies and the network answers exactly
zero forever. It looks like *training did nothing*. It is really *the network was dead before it
started*. A test asserts a fresh network is not.

### Watch the error fall

XOR is the fixture — the smallest problem one layer cannot solve and two can.

```
  step   loss
     0   0.5255
    10   0.2729
    50   0.01231
   100   3.895E-08
   250   6.951E-26
   500   1.042E-31
```

**The test asserts only that the loss fell by half**, not that it reached 10⁻³¹. The second would be a
claim about seed 7 on four points, and nobody can defend a number like that. "The error fell" is what
training *means*.

---

## A class is one more index

`Classifier.hb` is the Perceptron with one change:

```
weight second : Tensor[hidden, classes] from gaussian(seed) · 0.5
```

The last layer is `classes` wide instead of 1. **That is all a class is.** Most courses present
classification as a different subject from regression; it is the same network with a wider last
layer, and targets written one column per class.

### The softmax lives at the call site

```
def eval(x)   ≔ outputs(x, first, second)
def shares(x) ≔ softmaxRows(outputs(x, first, second))
```

`eval` answers **scores**; `shares` answers a **distribution**. Baking the softmax into the network
would make every caller pay for it and make the scores unreachable — which matters, because the
scores are what you train on.

And nothing is lost by keeping it outside, because **the softmax is monotone**: it never changes which
class wins, only how sure the answer sounds. A test asserts exactly that, row by row.

---

## The failure worth understanding

Written as two classes, XOR needs this at the origin:

```
input (0, 0)  →  [1, 0]      "off"
```

Train the classifier on the plain inputs, and at `(0, 0)` it answers:

```
scores at (0,0)    0.000   0.000
```

**Zero for both classes, and no amount of training moves it.**

Here is why. These networks have **no bias term**. Every unit computes *weights × input* and nothing
else. At the input `(0, 0)`, that product is zero **whatever the weights are**. So the network
answers zero at the origin, always, and a gradient step changes the weights without changing a
product of zero.

**The fix is the column of ones from [L1](../L1-novice/ml.md#the-column-of-ones):**

```
0  0  1
0  1  1
1  0  1
1  1  1
```

A constant input, weighted, is a bias. It is exactly how `Line` got its intercept, and it turns out a
neural network needs it for exactly the same reason.

### And an honest note about the Perceptron

This means one of the Perceptron's four XOR answers **was never learned**. Its target at `(0, 0)` is
`0` — which the bias-free network outputs anyway. The other three rows are genuinely learned; that
one came free. The test file says so, because a result that is partly luck should say which part.

---

## A bad start is not rescued by more training

With the bias column in place, try four seeds:

| Seed | Loss after 500 steps | Loss after 3000 | Every row right |
|---|---|---|---|
| 1 | 3 × 10⁻²⁶ | 3 × 10⁻³¹ | yes |
| 2 | 3 × 10⁻²⁰ | 5 × 10⁻³¹ | yes |
| 3 | 9 × 10⁻²⁸ | 3 × 10⁻³³ | yes |
| **7** | **0.25** | **0.25** | **no** |

Seed 7 lands on a **plateau**: two rows parked near an even split, and **3000 steps move it not at
all**. Gradient descent follows the slope it is standing on. Where that slope is flat it stops —
whether or not a better answer exists somewhere else.

That is why real training restarts from several seeds and keeps the best, and why *"the loss stopped
falling"* is not the same claim as *"the model is as good as it gets"*.

**The tests do not quietly switch to a seed that passes.** They use seed 1, say why, and pin seed 7's
plateau as a test of its own. Choosing a passing seed without saying so would be cherry-picking, and a
reader who later tried seed 7 would find out the hard way.

## Run them

```bash
dotnet run --project src/MLambda.AI.ML -- Perceptron
dotnet run --project src/MLambda.AI.ML -- Classifier
```

## What the tests assert

[`PerceptronTests.cs`](../../test/MLambda.AI.ML.Tests/PerceptronTests.cs) ·
[`ClassifierTests.cs`](../../test/MLambda.AI.ML.Tests/ClassifierTests.cs)

- A fresh network is not dead on arrival.
- Training reduces the error; one call is one step and moves the weights.
- The same seed draws the same network — **and a different seed draws a different one**, which is
  what stops the first test passing for a network that ignored its seed.
- Every row of shares is a distribution, and the softmax never changes which class wins.
- Once trained, the classifier picks the right class for every row.
- A bias-free network is stuck at zero on the origin.
- A bad start is not rescued by more training.

## Try it yourself

1. Remove the `· 0.5` from the Perceptron's weights and watch whether the network survives.
2. Train the classifier from seeds 4 through 10. How many get stuck?
3. Give the classifier three classes and a fixture that needs all three.
