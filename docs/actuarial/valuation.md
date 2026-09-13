# Valuation — overpayment, measured against 2 930 sales

**Source:** [`Valuation.hb`](../../src/MLambda.AI.Actuarial/Valuation.hb) ·
[`ValuationAgent.ha`](../../src/MLambda.AI.Actuarial/ValuationAgent.ha) ·
[`ValuationTests.cs`](../../test/MLambda.AI.Actuarial.Tests/ValuationTests.cs)

## Fair value is what houses like it sold for

Fit the corpus's prices against what a buyer can see — quality, living area, age — and every house gets a
fair value: what 2 930 real sales say a house like it fetched.

```
fn fairValueCoefficients(x, y) ↦ coefficients(x, y)
fn fairValueOf(b, x)          ↦ apply(x, b)
```

The fit says $26 009 per quality point, $63.07 per square foot, and −$495 per year of age.

## A premium is measured in spreads, not dollars

```
fn spreadOf(x, y)                       ↦ std(residuals(x, y))
fn excessInSpreads(offer, fair, spread) ↦ (offer − fair) / spread
```

The corpus disagrees with itself about houses like any given one by about $39 600 — the standard
deviation of what the fit leaves over. So a premium of $48 580 **sounds** large and is 1.23 spreads: the
kind of overpayment the market routinely made. The same $48 580 on a spread of $5 000 would be a warning.

**A number without a spread invites false confidence.** Reporting the premium in spreads is what stops
it.

## Checked against an exact answer, before any test

`Prelude.Multivariate`'s `coefficients` solves the normal equations by **conjugate gradient** — an
iterative method. And this design's gram matrix has a diagonal spanning **six orders of magnitude**
(2.9×10³ for the intercept column to 7.3×10⁹ for living area). That is exactly where an iterative solve
can quietly return a wrong answer that looks right.

So before any test was written, the normal equations were solved **exactly**, by pivoted elimination in a
separate script. Hilbert agrees to four decimals on every coefficient. The worry was unfounded — and it
was only unfounded because somebody checked.

## And a convention worth knowing

The spread came back **$39 602.14**; the separate script said **$39 595.38**. The ratio between them is
exactly √(2930/2929). `Prelude.Statistics`' `std` divides by **n − 1** — the sample standard deviation —
and the script had divided by n. Not an error on either side; a convention, and the test says which.

## What the agent believes

Guard arithmetic in `.ha` is integer, so the agent believes the excess in **hundredths of a spread**:

```
intend Buy       for Decided when B(self) excess(e) ∧ B(self) tolerance(t) ∧ e ≤ t
intend Negotiate for Decided when B(self) excess(e) ∧ B(self) tolerance(t) ∧ B(self) ceiling(c) ∧ e > t ∧ e ≤ c
intend Walk      for Decided when B(self) excess(e) ∧ B(self) ceiling(c) ∧ e > c
```

With tolerance 100 and ceiling 300: buy up to one spread over, negotiate up to three, walk beyond. A test
pins every boundary — at tolerance is a buy, one hundredth over is not.
