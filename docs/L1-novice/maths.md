# Mathematics, L1 — Algebra

**Source:** [`src/MLambda.AI.Maths/Algebra.hb`](../../src/MLambda.AI.Maths/Algebra.hb) ·
[`OneLaw.hb`](../../src/MLambda.AI.Maths/OneLaw.hb)

## The compiler knows no algebra

Not one identity. Not `x · 1 = x`, not `sin²+ cos² = 1`, not anything. Everything this section does,
it does because a **law** said so and a file **opened** it.

That sounds like a limitation. It is the whole idea. Here is a formula:

```
fn energy(x) ↦ sin(x)² + cos(x)²
```

and here is the entire C# the compiler produced for it:

```csharp
/// <remarks>Reduced by pythagorean.</remarks>
public static double Energy(double x) =>
    1d;
```

No sine. No cosine. Not a faster way of computing the sum — **the sum is gone**, replaced by the
answer, before the program ever ran.

## A fold is not a rounding

It is tempting to read that as an optimisation: the compiler noticed the sum was always about one
and saved you the trouble. It is not, and floating-point arithmetic is what proves it.

In doubles, `sin(x)² + cos(x)²` is usually *not* 1. Run the sample:

```
  energy(0.009)            1
  the same sum, computed   0.9999999999999999
```

**Those are different numbers.** The second is what the arithmetic gives you. The first is what the
law gives you, and the law is exact because no sine was ever taken.

The test asserts the exact value on purpose. An approximate assertion would have passed either way,
and would therefore have tested nothing.

## What made it happen

One line in `Prelude.Trigonometry`:

```
law pythagorean = sin(x)² + cos(x)² = 1
```

and one line in [`Algebra.hb`](../../src/MLambda.AI.Maths/Algebra.hb):

```
open Prelude.Trigonometry
```

Delete the `open` and `energy` stops folding — it becomes an ordinary sum of an ordinary sine and an
ordinary cosine, and starts returning 0.9999999999999999. Nothing else in the file changes.

> A note for the curious: the law is a *rewrite*, and rewrites only run in the shrinking direction.
> `pythagorean` turns the sum into `1`, never `1` into the sum. This is why "expand this expression"
> is not something you can ask for here, and [L3](../L3-advanced/maths.md) says more about it.

## The answer arrives with its reason

The same laws are also available while the program is running. Every file that can see a law gets a
**`Compute`** — a computer algebra system holding exactly that file's laws:

```csharp
Algebra.Compute.Simplify("x · 1 + 0")
```

which answers `x`. But that is only half of what comes back. The other half is this:

```
[unit, zero]
```

**The names of the laws that did it, in the order they fired.** Not a log you switched on — the
result carries them. You can look up `unit` and `zero` in `Prelude.Arithmetic` and read the two
sentences that justify the answer.

This is the habit the section exists to build. An algebra system that will not tell you why is
asking to be trusted. This one hands you the reasoning with the result.

## A computer algebra system, in three lines

If laws really are the knowledge, then a file with one law should be a CAS that knows one thing.
[`OneLaw.hb`](../../src/MLambda.AI.Maths/OneLaw.hb) is that file, in full:

```
model OneLaw {
  law only_unit = x · 1 = x
}
```

It opens nothing at all. Ask both files the same question:

```
  Algebra.hb says    x · 1 + 0  ↦  x       by [unit, zero]
  OneLaw.hb says     x · 1 + 0  ↦  x + 0   by [only_unit]
```

`OneLaw` removed the `· 1` and left the `+ 0` exactly where it was — because nobody ever told it
that `x + 0 = x`. It is not broken and it is not wrong. It is *ignorant*, precisely and legibly.

**And the two files sit beside each other in the same project and the same namespace.** A `Compute`
is per file, never per project:

> R(F) = own(F) ∪ ⋃ R(o), for every module o that F opens

which is the rule in one line: a file's CAS knows what that file declares, plus what it opens, and
nothing else it happens to be compiled next to.

## Run it

```bash
dotnet run --project src/MLambda.AI.Maths -- Algebra
```

```
sin(x)² + cos(x)² — and what the build did with it.

  energy(0.009)            1
  the same sum, computed   0.9999999999999999

Those are different numbers. `pythagorean` removed the sine and the cosine while
this project was building, so the first one is a literal the method returns —
not a rounding of the second.

And the same laws, called at run time instead:

  Algebra.hb says    x · 1 + 0  ↦  x   by [unit, zero]
  OneLaw.hb says     x · 1 + 0  ↦  x + 0   by [only_unit]
```

## What the tests assert

[`AlgebraTests.cs`](../../test/MLambda.AI.Maths.Tests/AlgebraTests.cs) ·
[`OneLawTests.cs`](../../test/MLambda.AI.Maths.Tests/OneLawTests.cs)

- `Energy` returns the **exact** double `1`, at two inputs where the computed sum is not 1.
- A control test asserts the computed sum really is not 1 — so if the compiler ever stopped folding,
  the test above could not pass by luck.
- `Simplify` returns `x` *and* the step list `[unit, zero]`.
- `OneLaw.Compute` simplifies `x · 1` by `[only_unit]`, and leaves `x + 0` untouched with **no**
  steps at all.
- `Algebra.Compute` simplifies the same `x + 0` — the two files disagree, in one assembly.

## Try it yourself

1. Delete `open Prelude.Trigonometry` from `Algebra.hb` and rebuild. Watch `Energy` become a real
   computation, and watch the test fail on the sixteenth decimal place.
2. Add `law only_zero = x + 0 = x` to `OneLaw.hb`. Now it can finish the job — and the step list it
   reports grows from one name to two.
3. Ask `Algebra.Compute.Simplify` for something it cannot do, such as `"x + x"`. Nothing is wrong
   with the expression; there is simply no law in scope that matches it, so it comes back as it went
   in, with an empty step list.
