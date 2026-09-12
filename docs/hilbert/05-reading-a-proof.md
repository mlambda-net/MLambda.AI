# 5. Reading a proof

A `.hp` file proves things about a theory. Here is the whole of one, from
[`Animals.hp`](../../src/MLambda.AI.Logic/Animals.hp), with every piece named.

## The header

```
open Animals
axioms [directly, climbing]
```

`open` names the theory being reasoned about — the relations and laws come from there, so nothing is
restated. `axioms` lists the laws you are allowed to lean on. This matters: *"proved from `directly`
and `climbing`"* is a different statement from *"proved"*, and the verdict the build records keeps
the axiom list beside the claim for exactly that reason.

## A theorem

```
theorem what_is_known_is_so : ∀ x k, known(x, k) ⇒ is_a(x, k)
proof
  intro x k said
  apply directly [said]
qed
```

- **`theorem <name> : <claim>`** states what you will show. Read `∀ x k` as "for any x and any k",
  `⇒` as "then", `∧` as "and".
- **`intro`** names what you are *given*. The claim says "for any x and k, if `known(x, k)` …", so
  `intro x k said` takes an arbitrary x and k, and names the assumption `known(x, k)` as `said`. You
  choose these names; pick ones that read.
- **`apply <law> [premises]`** uses a law, handing it its premises **in the order the law asks for
  them**.
- **`qed`** ends it.

## A step in the middle

```
theorem a_cat_is_an_animal : ∀ x c a, known(x, c) ⇒ kind_of(c, a) ⇒ is_a(x, a)
proof
  intro x c a said wider
  have surely : is_a(x, c) by apply directly [said]
  apply climbing [surely, wider]
qed
```

**`have <name> : <claim> by <tactic>`** proves something on the way and names it. Here `climbing`
needs an `is_a` for its first premise, and all we were given is that we were *told* x is a c. So
`directly` turns the telling into the being, we call that `surely`, and `climbing` takes it.

That is the whole shape of a proof: name what you are given, build what you need, apply a law.

## Letting the machine find it

```
theorem an_animal_found_automatically : ∀ x c a, known(x, c) ⇒ kind_of(c, a) ⇒ is_a(x, a)
proof
  intro x c a said wider
  auto
qed
```

Same claim as `a_cat_is_an_animal`, no steps. **`auto`** searches the theory's laws and the
hypotheses in scope and finds the derivation itself — and the kernel then checks the term it built
exactly as it checks a hand-written one. `auto` is not a promise; it is a search whose result is
verified.

Worth comparing the two side by side. The hand-written one teaches you *why*; `auto` is what you
reach for once you know.

## What the build does with it

Each proof becomes a `ProvedClaim` in `Generated/Proof/`:

```csharp
public static ProvedClaim ACatIsAnAnimal { get; } = new(
    "theorem",
    "a_cat_is_an_animal",
    "∀ x c a, known(x, c) ⇒ kind_of(c, a) ⇒ is_a(x, a)",
    "directly, climbing",
    "Proved");
```

Your program can read these and print what it was shown to satisfy, without running a prover — the
checking already happened, at build time.

Next: [diagnostics](06-diagnostics.md).
