# Credit — a method on inputs that do not exist

**Source:** [`Credit.hb`](../../src/MLambda.AI.Actuarial/Credit.hb) ·
[`CreditAgent.ha`](../../src/MLambda.AI.Actuarial/CreditAgent.ha) ·
[`CreditTests.cs`](../../test/MLambda.AI.Actuarial.Tests/CreditTests.cs)

## Read this first

**`houses.csv` records sales, not mortgages.** It has no loan-to-value, no interest rate, no borrower, and
no default outcomes. So every input to this model is invented, and so is every coefficient:

- the **loan-to-value** and the **rate** are synthesised, deterministically, from the sale price and a
  fixed seed;
- the **hazard coefficients** are declared, not fitted — there are no defaults to fit them to;
- the **recovery** on foreclosure is a declared 75% of the sale price.

**This agent's number demonstrates a method. It measures nothing.** It says nothing about any real
borrower, and nothing in this repository may present it as though it did.

That warning is in three places — the top of `Credit.hb`, this page, and the program's output every time
the credit agent reports — because one is too easy to miss.

## Why the sample exists anyway

Because a credit view is a real part of a purchase decision, and the question of how it enters — as an
agent that can veto the other two — is worth showing. A proportional-hazards model over loan attributes is
worth showing too. Showing both on invented inputs is honest **as long as nobody pretends otherwise.**

The rule this repository follows: **a number is a measurement or it is a demonstration, and which one it
is must never be ambiguous.**

## The synthesis

```
def scatter01(price, seed) ≔ (price · 0.000037 + seed · 0.61) − floor(price · 0.000037 + seed · 0.61)

fn loanToValueOf(price, seed) ↦ 0.60 + 0.35 · scatter01(price, seed)
fn rateOf(price, seed)        ↦ 0.03 + 0.05 · scatter01(price, seed + 1)
```

The fractional part of a fixed affine map: a number in [0, 1) that depends only on price and seed. **Not
random, and not pretending to be.** A fixed seed makes the invented loan reproducible, so it can be
tested, and means nobody can mistake variation between runs for signal.

## The hazard

```
fn defaultHazard(x, b) ↦ hazardOf(x, b)
fn defaultChance(h)    ↦ 1 − exp(0 − h)
```

`hazardOf` is Cox's proportional hazards from `Prelude.Sequences`: `exp(x · b)`, clamped. With the declared
coefficients `[4, 20, −6]` over `[loan-to-value, rate, 1]`, borrowing more of the price and paying more
interest both raise the hazard. **That is a property of the numbers chosen**, and the test says so rather
than implying the data showed it.

## What the tests check — exactly two things

1. **The synthesis is deterministic** — the same price and seed give the same loan every run — and it
   **depends on the price**, so a synthesis that returned a constant could not pass.
2. **The arithmetic is what the formulas say** — every value computed independently to five or six
   decimals.

Nothing more is tested, because nothing more is true.

## A Hilbert defect found here

`scatter01` is a `def`. It was first written as a `fn`, and the Hilbert compiler accepted that without a
word — then the generated C# failed to build, with the call routed to `System.Math.Scatter01`, as though a
sibling function were a built-in like `floor`. A helper that other definitions call is a `def`, which is
how the Prelude is written throughout. See [diagnostics](../hilbert/06-diagnostics.md).
