# Machine learning, L3 — Nearest, and what this project does not claim

**Source:** [`src/MLambda.AI.ML/Nearest.hb`](../../src/MLambda.AI.ML/Nearest.hb)

Half of this page is a model that does not train. The other half is about limits, because at this
level a reader should be able to say exactly what the samples prove and exactly what they do not.

---

## The models that do not train

A Perceptron spends its fit **compressing the data into weights**, and then throws the data away.
The models in `Nearest.hb` do the opposite: they **keep every point** and do all the work at the
moment a question is asked.

```
fn classifyNear(z, x, g, k) ↦ knnClassify(z, x, g, k)
fn sharesNear(z, x, g, k)   ↦ knnShares(z, x, g, k)
fn regressNear(z, x, y, k)  ↦ knnRegress(z, x, y, k)
fn nearestCentre(z, x, g)   ↦ centroidPredict(z, x, g)
```

Nothing is reimplemented here — every one is `Prelude.Neighbours`. `x` is the points already known,
`g` their classes one-hot, and `z` the points being asked about.

### k-nearest neighbours

The `k` closest known points vote, and the most votes wins. `sharesNear` reports the same vote as a
distribution — and on clusters as far apart as the fixture's, the share is exactly 1 and 0. **That is
not a probability anybody estimated.** It is a count of who turned up.

`regressNear` does the same for numbers: average the values of the `k` nearest. Asked about 0.9 with
known points at 0, 1, 2 and an outlier at 10, it averages the first three and ignores the outlier
entirely — which is the whole point of asking only the neighbours.

### Nearest centroid

Average each class down to **one point**, then ask which average is closest. It keeps two points where
k-NN keeps six.

On clusters this clean the two agree:

```
  asked           k-NN (k=2)   nearest centroid
  (0.5, 0.5)      A            A
  (10.5, 10.5)    B            B
```

and that is exactly the situation in which the cheaper model is the right one.

### The trade, plainly

| | Remembers | Work happens |
|---|---|---|
| Perceptron | weights only | during training |
| Nearest centroid | one point per class | at question time, cheaply |
| k-NN | every point | at question time, in full |

**None of these is better.** They trade memory for time. A reader who has met all three stops thinking
*model* means *weights* — and that is the idea this sample exists to leave behind.

---

## What this project does not claim

Worth a page of its own, because an ML sample that quietly overstated its results would teach the
wrong thing more effectively than anything else here.

### The fixtures are tiny, and the numbers are not results

Every model in this project is tested on four to six points. When a test asserts that the error
**fell**, it proves the training loop works — that gradients are computed, steps are taken, and the
loss moves the right way.

**It says nothing about generalisation.** A network that reaches 10⁻³¹ on four XOR points has
memorised four XOR points. Whether it would do anything sensible on a fifth is a question none of
these tests asks, and none of them should be read as answering it.

### A loss that stopped falling is not a model that is as good as it gets

Seed 7 kills five of the classifier's eight hidden units, and the weights then stop changing
**entirely** — bit-for-bit identical after thousands more steps — while seeds 1, 2 and 3 reach every
row. See [L2](../L2-practitioner/ml.md#a-bad-start-is-not-rescued-by-more-training).
Convergence is a property of *where you started*, not only of the problem.

### Some answers are forced rather than learned

A network with no bias term — which is `Prelude.Networks`' documented convention, not an accident —
answers zero at the origin whatever its weights are. When the target
there happens to be zero, the network is right for free — and a report that counted that row as
learned would be overstating. The Perceptron's XOR result has exactly one such row, and its test file
says which.

### Seeds are part of the result

Every stochastic claim in this project names its seed, and the tests assert that the same seed draws
the same network **and that a different seed draws a different one**. The second assertion is the one
that matters: without it, a model that ignored its seed entirely would pass the reproducibility test
and every result would be one accident repeated.

### Pretrained models are opt-in

Hilbert can load large pretrained models — MiniLM (~90 MB), ResNet50 (~102 MB), Phi (~2.6 GB). None
of this project's default samples needs one, none downloads anything, and `HILBERT_NO_NETWORK=1` is
honoured. A sample that did need one would run only when asked for by name.

---

## Run it

```bash
dotnet run --project src/MLambda.AI.ML -- Nearest
```

## What the tests assert

[`NearestTests.cs`](../../test/MLambda.AI.ML.Tests/NearestTests.cs)

- A point near the origin is class A and one near (10, 10) is class B, one-hot per row.
- The vote shares are unanimous when the clusters are this far apart.
- Nearest-centroid agrees while keeping one point per class.
- Regression averages the nearest known values — and ignores the outlier.

## Try it yourself

1. Move the two clusters toward each other until k-NN and nearest-centroid disagree. Which is right?
2. Change `k` from 2 to 6 on the fixture. With every point voting, what happens to the shares?
3. Add a single mislabelled point inside cluster A and see which model it fools.
