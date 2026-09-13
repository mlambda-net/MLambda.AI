# Machine learning, L1 — Line

**Source:** [`src/MLambda.AI.ML/Line.hb`](../../src/MLambda.AI.ML/Line.hb)

## A model is a formula you can read

Here are four points: (1, 4), (2, 7), (3, 10), (4, 13). There is a straight line through all of
them — y = 3x + 1 — and "fitting a line" means finding it from the points alone.

In Hilbert, the fit is one line:

```
fn lineOf(x, y) ↦ coefficients(x, y)
```

And that is the thing worth noticing first. **The model is a formula.** Not a class with a `Fit()`
method hiding a loop somewhere — a formula you can read, and that the compiler turns into code.

## What `coefficients` does

`coefficients` comes from `Prelude.Multivariate`, and it is ordinary least squares: of all the
straight lines you could draw, pick the one whose total squared miss is smallest. Hand it the points
and it hands back two numbers — the slope and the intercept.

**This sample opens one Prelude module, and it is the only L1 sample that has to.** A first
machine-learning example that rewrote least squares from scratch would be teaching least squares.
That is not the subject. The subject is that a model is something you can read.

> A note for the curious: least squares is in `Multivariate`, not `Regression`. The `Regression`
> module's own first line says it holds *"the linear models that are **not** least squares"* —
> logistic, lasso and friends. It is an easy thing to look for in the wrong place.

## The column of ones

The inputs are written like this:

```
x     1
─     ─
1     1
2     1
3     1
4     1
```

Why the second column of ones? Because `coefficients` only knows how to **weight columns** — slope
times the first column, something times the second. A column that is always one, weighted by some
number, *is* a constant. That is how a straight line gets its `+ 1`.

Remember this column. It comes back at [L2](../L2-practitioner/ml.md), where leaving it out breaks a
neural network in a way no amount of training can fix.

## A number without a spread

```
fn spreadOf(x, y) ↦ std(residuals(x, y))
```

The fit tells you where the line is. The spread tells you **how much the data disagrees with it**.

On the four clean points the spread is exactly zero — every point sits on the line. Nudge two of the
y values and the spread becomes 0.775. The line barely moves; the spread is what tells you something
changed.

**A fit reported without its spread invites false confidence**, and that habit is worth having from
the first sample.

## One formula, many methods

Look inside `Generated/Hilbert/Line.g.cs` after a build and you will find something surprising. This
one line of Hilbert:

```
fn errorOf(predicted, actual) ↦ (predicted − actual) · (predicted − actual)
```

became **a dozen C# methods**:

- one for plain numbers (`double`),
- one for complex numbers,
- one each for vectors, matrices and tensors — with a plain number allowed on either side,
- and an **ONNX graph**, a format other machine-learning tools can run.

You wrote the formula once. So a whole batch of predictions costs one call and no extra source: hand
`errorOf` two vectors and it answers a vector.

## Run it

```bash
dotnet run --project src/MLambda.AI.ML -- Line
```

```
  recovered   y = 3.000x + 1.000
  at x = 10   predicts 31.000   (the line says 31)
  spread      0.000   no noise in, no spread out
```

## What the tests assert

[`LineTests.cs`](../../test/MLambda.AI.ML.Tests/LineTests.cs)

- **A fit recovers a line it was given.** Points from y = 3x + 1, fitted, and 3 and 1 come back.
  This is the strongest test a fit can have: a fit that cannot recover a line with no noise in it is
  not a fit, whatever else it does.
- It predicts where the line goes next.
- No noise in, no spread out — and noisy data has a spread above zero.
- The same `errorOf` runs over a whole vector at once, and a plain number broadcasts across one.
- The formula is also an ONNX graph.

## Try it yourself

1. Change the outputs to come from y = −2x + 5 and check the recovered line.
2. Remove the column of ones from the inputs. What does the fit do without an intercept?
3. Add a fifth point far off the line and watch the spread — and the line — respond.
