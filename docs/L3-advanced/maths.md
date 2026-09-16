# Mathematics, L3 — Linear, proof, and what this project does not claim

**Source:** [`Linear.hb`](../../src/MLambda.AI.Maths/Linear.hb) ·
[`Maths.hs`](../../src/MLambda.AI.Maths/Maths.hs) ·
[`Maths.hp`](../../src/MLambda.AI.Maths/Maths.hp)

## One contraction, wearing several names

`Prelude.LinearAlgebra` does not implement matrix multiplication, transposition, the inner product
and the outer product. It implements **one** operation and names it four times:

```
def mul(a, b)    ≔ einsum("ij,jk->ik", a, b)
def transpose(a) ≔ einsum("ij->ji", a)
def dot(x, y)    ≔ einsum("i,i->", x, y)
def outer(x, y)  ≔ einsum("i,j->ij", x, y)
```

A `def` is inlined, so naming costs nothing at run time. [`Linear.hb`](../../src/MLambda.AI.Maths/Linear.hb)
writes the product twice, once by each route, and a test asserts the two agree elementwise:

```
  mul(a, a)                      [7 10; 15 22]
  einsum("ij,jk->ik", a, a)      [7 10; 15 22]
```

They are not two implementations that happen to match. After inlining they are the same arithmetic,
and "matrix multiplication" turns out to be a *name for a contraction* rather than an operation in
its own right.

## Two things called solve

```
  solve(a, b)    here, numeric    x = [2 3]    conjugate gradient
  Compute.Solve  in Solve.hb      x = 2        rewriting, by ⇔ laws
```

| | What it is | Where it lives |
|---|---|---|
| `solve(a, b)` | conjugate gradient over a real matrix — numbers in, numbers out | `Prelude.LinearAlgebra`, callable from `.hb` |
| `Compute.Solve(eq, x)` | symbolic isolation of a variable — an equation in, an expression out | the CAS, callable only from C# |

They share an English word and nothing else. There is **no `solve` you can write inside an `.hb`
expression that isolates a variable**, and no numeric conjugate gradient reachable from the CAS.
Knowing which one you are holding is most of the skill.

## Three grades of certainty

[L2](../L2-practitioner/maths.md#so-the-derivative-costs-nothing-at-run-time) watched `∂ x² / ∂ x`
become `2x`, and trusted it because the generated code said `Reduced by power, self, unit,
power_one` and those are real law names. Trusting a list of names is still trusting.

There are three different things this project can mean by "proved", and they are not
interchangeable:

```
  Demonstrate   searched, and found    theorem t : x · 1 + 0 = x by rw [unit, zero]
  Proof         checked those steps    [unit, zero]
  the .hp       checked by the build   Proved
```

**`Demonstrate` searches.** Give it a claim and no steps, and it looks for a derivation — bounded, so
it always terminates: depth ≤ 8, at most 10,000 goals, and a term may not grow past
`2 · max(|l|, |r|) + 8`. When it hits a cap it returns `NotFound` carrying the closest goal it
reached. A `NotFound` means *this search did not find one*, never *there is none*.

**`Proof` checks.** Hand it steps and it replays them. It does not look for anything, so it cannot
run out of room — and it cannot help you find a proof you do not already have.

These two round-trip: what `Demonstrate` writes back is a script `Proof` accepts, and the test
asserts exactly that.

**The `.hp` kernel decides.** [`Maths.hp`](../../src/MLambda.AI.Maths/Maths.hp) proves the product
rule derivation itself, and `HilbertProofStrict` is on, so a theorem that stops checking **fails the
build**. `sorry` is not available.

```
theorem deriv_of_x_squared : d(times(var, var)) = times(konst(2), var)
proof
  rewrite [d_times, d_var, d_var, unit_left, unit_right, double]
  ring
qed
```

Six rewrites and a `ring`: the product rule, `d_var` twice, the two unit laws, and `u + u = 2u`.
Nothing is folded silently and no step is a built-in. This is the L2 derivative, computed rather
than asserted.

Note what changed on the way in. The `.hb` rule is `∂ u / ∂ x = 0 where x ∉ u` — a side condition on
a rewrite. A `.hs` law has no `where`, and needs none, because the term algebra carries the
condition: `var` **is** the variable and `konst(c)` is anything that is not, so "x does not occur in
u" becomes a matching problem. The guard became structure.

## Where the mathematics meets the AI

`Perceptron.hb` in the [machine learning](../L2-practitioner/ml.md) subject descends a mean squared
error. For a single linear unit that loss is `(w · x − y)²`, and the update rule is written there by
hand, the way every textbook writes it.

Here it is derived, by the same thirteen laws that took the derivative at L2:

```
  Perceptron.hb's loss, for one linear unit    (w · x − y)²
  ∂ of it in w, by the same thirteen laws      2(w · x − y) · x
                                              by [power, difference, product, constant,
                                                  minus_zero, self, unit_left, nil, zero, power_one]
```

Ten laws, and the answer is the update the ML subject states. A test asserts the derived text
exactly, so if the two ever stop agreeing, something has to give.

That is the argument for a mathematics subject sitting beside the AI ones. The gradient in a
learner is not a piece of machine learning trivia to be memorised — it is a derivative, it follows
from rules you can open and read, and here it is checked rather than quoted.

## What this project does not claim

**Matching is structural.** There is no associative-commutative matching, so `x · 1 = x` and
`1 · x = x` are two separate laws and `Prelude.Arithmetic` declares both. Two expressions that are
equal as mathematics can fail to meet, and `Equivalent` returning false means *these laws did not
connect them*.

**Only shrinking rewrites are kept.** A rewrite that makes the term larger is discarded — the sole
exception being one that removes a derivative. So "expand this" is not expressible as a law at all;
`Prelude.Trigonometry` says so about its own double-angle rules.

**Guards are `x ∉ u` and nothing else.** There is no `n ≠ −1`, no `x > 0`. `Prelude.Integration`
works around it by declaring `int_reciprocal` before `int_power` and relying on first-match-wins
against the literal `−1` — and a symbolic `n` that turns out to be `−1` only at run time divides by
zero. That is a stated limitation, not a bug. Laws true only on part of a domain, like
`exp(ln(x)) = x`, are deliberately absent from the preludes: a rewrite cannot check the sign of a
run-time value, and would otherwise silently repair a formula that ought to fail.

**There is no `log`.** `ln` is an intrinsic; base-10 is not in the language.

**`Compute` reads expressions only** — never a `def` or `fn` body. You cannot hand it the name of
something in your own `.hb` and have it look up the definition.

**The samples are small on purpose.** Nothing here says anything about performance, about
conditioning, or about what conjugate gradient does to a matrix that is not well behaved. The 2×2
systems on this page are checkable by hand, which is the only property they were chosen for.

## Run it

```bash
dotnet run --project src/MLambda.AI.Maths -- Linear
dotnet run --project src/MLambda.AI.Maths -- Proof
```

```
[1 2; 3 4] squared, by two names for one contraction.

  mul(a, a)                      [7 10; 15 22]
  einsum("ij,jk->ik", a, a)      [7 10; 15 22]

And two different things are called solve:

  solve(a, b)   here, numeric      x = [2 3]   conjugate gradient
  Compute.Solve  in Solve.hb       x = 2          rewriting, by ⇔ laws
```

## What the tests assert

[`LinearTests.cs`](../../test/MLambda.AI.Maths.Tests/LinearTests.cs) ·
[`ProofTests.cs`](../../test/MLambda.AI.Maths.Tests/ProofTests.cs) ·
[`CalculusTests.cs`](../../test/MLambda.AI.Maths.Tests/CalculusTests.cs)

- `[1 2; 3 4] · I` is `[1 2; 3 4]`, elementwise.
- `Product` and `Contracted` agree in all four cells — the name and the `einsum` are one function.
- The numeric `solve` recovers `x = [2, 3]` from a diagonal system, to six places.
- `MathsProofs.DerivOfXSquared()` is judged `Proved` **by the kernel while the test runs**, not read
  back from a build log.
- `Demonstrate` then `Proof` round-trips on `x · 1 + 0 = x`.
- `Demonstrate` on `tan(x) = x` is `NotFound` — the bounded search comes back rather than hanging.
- `Derivate("(w · x − y)²", "w")` is exactly `2(w · x − y) · x`.

## Try it yourself

1. Delete a law from the `axioms [...]` list in `Maths.hp` and rebuild. The build fails, and names
   the step the kernel could not take — which is what "the build checks the proof" means in
   practice.
2. Ask `Compute.Equivalent("x · 2", "2 · x")`. Structural matching, no commutativity — and now the
   first limit on this page is something you have seen rather than read.
3. Widen the search with `Compute.WithLimits(new Limits(Depth: 12))` and try a `Demonstrate` that
   returned `NotFound` at the default depth. Some of them come back proved, which is the useful way
   to learn that `NotFound` was never a claim about mathematics.
