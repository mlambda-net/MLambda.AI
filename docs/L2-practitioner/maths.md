# Mathematics, L2 — Calculus and Solve

**Source:** [`Calculus.hb`](../../src/MLambda.AI.Maths/Calculus.hb) ·
[`Solve.hb`](../../src/MLambda.AI.Maths/Solve.hb)

Two samples, and the most instructive thing on this page is a method that does not exist.

## A derivative is a term, not a feature

At [L1](../L1-novice/maths.md#the-compiler-knows-no-algebra) a law removed a sine. This is the same
machinery pointed at calculus:

```
fn slope(x) ↦ ∂ x² / ∂ x
```

`∂ u / ∂ x` is not special syntax the compiler understands. It is an **ordinary term** — one that
thirteen laws in `Prelude.Calculus` know how to remove. Each law is oriented so that applying it
deletes one `∂`:

```
law self    = ∂ x / ∂ x = 1
law power   = ∂ (u^n) / ∂ x = n · u^(n − 1) · ∂ u / ∂ x where x ∉ n
law product = ∂ (u · v) / ∂ x = ∂ u / ∂ x · v + u · ∂ v / ∂ x
law sine    = ∂ sin(u) / ∂ x = cos(u) · ∂ u / ∂ x
```

Apply them until no `∂` is left, and what remains is arithmetic.

## So the derivative costs nothing at run time

That reduction happens while the project is building. Here is the generated C# for `slope`, in full:

```csharp
/// <remarks>Reduced by power, self, unit, power_one.</remarks>
public static double Slope(double x) =>
    (2d * x);
```

and for the chain-rule case:

```csharp
/// <remarks>Reduced by sine, power, self, unit, power_one.</remarks>
public static double Chain(double x) =>
    (System.Math.Cos(System.Math.Pow(x, 2d)) * (2d * x));
```

**Nothing differentiates while your program runs.** There is no expression tree, no tape, no
autodiff pass — there is `2d * x`, which is what you would have written by hand, arrived at by a
route you can audit. The `<remarks>` line is generated too: it names the four laws that fired.

The same laws are also callable at run time, over text, through the file's `Compute`:

```
  ∂ sin(x²) / ∂ x  ↦  cos(x²) · (2x)
                       by [sine, power, self, unit, power_one]
```

Same five laws, same answer, different moment. Which one you want depends on whether the formula is
known when you compile or only when you run.

## The failure worth understanding

Now the important part. `Prelude.Calculus` has thirteen rules. **None of them names `tan`.**

[`Calculus.hb`](../../src/MLambda.AI.Maths/Calculus.hb) declares a doorway for it anyway:

```
fn tangent(x) ↦ ∂ tan(x) / ∂ x
```

The build accepts this. It prints no error and no warning. The build is green.

And `Calculus.Tangent` **does not exist**.

Look in the generated class and there is `Slope`, there is `Chain`, and there is nothing called
`Tangent` at all. The reduction ran out of laws with the `∂` still in place, and the compiler's own
guide states the consequence plainly:

> A doorway that still contains a derivative after reduction is **not emitted, and no error is
> printed** — this happens when `Prelude.Calculus` is not opened, and for functions it has no rule
> for (`∂ tan(x) / ∂ x`, `∂ x^x / ∂ x`). If a method you expect is missing from the generated class,
> look for a derivative that could not be taken.

**If a method you expected is missing, this is the first thing to check.** Nothing else will tell
you. The C# compiler will report a name that does not exist, several steps downstream, in a file
that has nothing to do with the cause.

The test asserts the absence by reflection, so the claim cannot quietly stop being true:

```csharp
Assert.Null(typeof(Calculus).GetMethod("Tangent"));

// THE CONTROL, so this cannot pass by a typo: the two that reduce ARE here.
Assert.NotNull(typeof(Calculus).GetMethod("Slope", [typeof(double)]));
```

The control matters. Without it, renaming the sample would make the first line pass for the wrong
reason forever.

### The CAS is the kinder of the two

Ask the run-time CAS for the same derivative and it does not go quiet:

```
  ∂ tan(x) / ∂ x   ↦  ∂ tan(x) / ∂ x
```

It comes back `Unresolved`, carrying the residue — the term as far as it got, `∂` and all. It could
not do it either, but it *says* so. A build that deletes a method and a CAS that hands back what it
could not finish are two different levels of courtesy about the same missing law.

---

## The equation that will not move

The second refusal is sharper, because at first it looks like a bug.

`Algebra.hb` has fifteen laws in scope. Ask it to solve an equation a child could do:

```
  Algebra.hb   2x + 3 = 7
```

That is not the answer. That is `Unsolved`, printing the closest goal the search reached — which is
where it started.

**Every one of those fifteen laws is a term law.** A term law rewrites a *sub-term* wherever it
matches: `x · 1` becomes `x`, anywhere in an expression. Isolating `x` is a different kind of move —
it rewrites the **equation itself**, and only a law written with `⇔` can do that.

[`Solve.hb`](../../src/MLambda.AI.Maths/Solve.hb) declares two of them:

```
model Transposing {
  law sub_add = a + b = c ⇔ a = c − b
  law div_mul = a · b = c ⇔ b = c / a
}
```

and the same question now answers:

```
  Solve.hb     x = 2
```

**No prelude ships those two laws, and that is deliberate.** Transposition by `div_mul` is valid only
when `a` is not zero, and the only guard the law language has is `x ∉ u` — *not free in*. There is no
way to write `a ≠ 0`. So the prelude declines to state a law it cannot qualify, and leaves you to
declare it in the file that depends on it, where a reader can see what was assumed.

The refusal was the honest answer. A system that guessed a transposition it had never been given
would have been more convenient and less trustworthy.

## Run it

```bash
dotnet run --project src/MLambda.AI.Maths -- Calculus
dotnet run --project src/MLambda.AI.Maths -- Solve
```

```
∂ x² / ∂ x, taken while this project was building.

  slope(3)    6
  chain(3)    -5.466781571308061

The generated C# for `slope` is `(2d * x)`. There is no derivative in it, and no
differentiation happened just now.

The same laws, at run time:

  ∂ sin(x²) / ∂ x  ↦  cos(x²) · (2x)
                       by [sine, power, self, unit, power_one]

  ∂ tan(x) / ∂ x   ↦  ∂ tan(x) / ∂ x
                       no law names tan, so the ∂ is still there.
```

```
2 · x + 3 = 7 — asked of two files.

  Algebra.hb   2x + 3 = 7
  Solve.hb     x = 2
```

## What the tests assert

[`CalculusTests.cs`](../../test/MLambda.AI.Maths.Tests/CalculusTests.cs) ·
[`SolveTests.cs`](../../test/MLambda.AI.Maths.Tests/SolveTests.cs)

- `Slope(3)` is 6 and `Slope(-2)` is −4 — the build-time derivative, checked as ordinary arithmetic.
- `Chain(3)` equals `cos(9) · 6`, so the chain rule came out of `sine` and `power` together rather
  than a special case.
- `Compute.Derivate("sin(x²)", "x")` is `Resolved` and its steps include `sine`.
- **`typeof(Calculus).GetMethod("Tangent")` is null**, with `Slope` and `Chain` asserted present as
  a control.
- `Compute.Derivate("tan(x)", "x")` is `Unresolved` and its text still contains a `∂`.
- `Algebra.Compute.Solve(…)` is `Unsolved`; `Solve.Compute.Solve(…)` prints exactly `x = 2`.

## Try it yourself

1. Add `fn power(x) ↦ ∂ x^x / ∂ x` to `Calculus.hb` and rebuild. The build stays green and `Power`
   never appears. Now you have met the trap twice and will recognise it in your own code.
2. Remove `law div_mul` from `Solve.hb`, leaving only `sub_add`. The answer goes back to `Unsolved`,
   and the goal it prints is the closest the search reached — you can read off exactly how far one
   equation law got before it ran out.
3. Give `Solve.hb` a *wrong* equation law, such as `law bad = a + b = c ⇔ a = c + b`, and solve
   again. The CAS will faithfully use it and confidently produce nonsense. The laws are the
   knowledge, which means the laws are also the liability.
