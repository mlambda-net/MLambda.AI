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
and `climbing`"* is a different statement from *"proved"*. A proof that reaches for a law the list
does not name is refused. (An **empty** list, `axioms []`, restricts nothing: it allows every law.)

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

The build checks every proof first, and a theorem that does not check fails the build. Then each
`.hp` becomes a class in `Generated/Proof/` with one method per theorem, and nothing else:

```csharp
public static class AnimalsProofs
{
    public static Judged ACatIsAnAnimal() => Prover.Prove(typeof(AnimalsProofs).Assembly, "Animals.hp", "a_cat_is_an_animal");
    // ...one line per theorem, in the order the file states them
}
```

**There is no verdict in the generated code.** The `.hp` and its `.hs` theories are embedded in the
assembly, and calling the method does the work: it parses both, elaborates the script, and lets the
kernel replay the term.

```csharp
Judged verdict = AnimalsProofs.ACatIsAnAnimal();
// verdict.Status == "Proved" — decided now, not read back
```

`Judged` is `(Kind, Name, Status, Detail, Line)`. `Detail` says what the kernel objected to when the
status is not `Proved`. The class has no list of its theorems; a test that wants all of them finds the
methods by reflection (see `Theorem.Of` in `test/MLambda.AI.Logic.Tests/Theorem.cs`). The prover is the
`MLambda.Hilbert.Proof` library; the proof targets add the reference for you, so there is no compiler
at run time.

Next: [diagnostics](06-diagnostics.md).
