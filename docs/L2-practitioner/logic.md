# Logic, L2 — Worlds and Counting

**Source:** [`src/MLambda.AI.Logic/Worlds.hs`](../../src/MLambda.AI.Logic/Worlds.hs) ·
[`Worlds.hp`](../../src/MLambda.AI.Logic/Worlds.hp) ·
[`Counting.hs`](../../src/MLambda.AI.Logic/Counting.hs) ·
[`Counting.hp`](../../src/MLambda.AI.Logic/Counting.hp)

Two samples, and one idea each. `Worlds`: **a logic is a choice of which laws are open.** `Counting`:
**a claim can be decided rather than assumed.**

---

## Worlds — the frame conditions

### A world is a way things could be

Modal logic is about necessity and possibility, and the machinery underneath is simpler than the
words suggest. There are *worlds* — ways things could be — and a relation between them, `sees(w, u)`,
meaning *from w, u is possible*.

"Necessarily p" then means: p holds at every world this one sees. "Possibly p": p holds at some
world it sees. Everything else is a question about the **shape** of that relation.

### Four shapes, and the logic each gives you

```
law refl   = ∀ w,     place(w) ⇒ sees(w, w)                -- T
law trans  = ∀ w u v, sees(w, u) ∧ sees(u, v) ⇒ sees(w, v) -- 4
law euclid = ∀ w u v, sees(w, u) ∧ sees(w, v) ⇒ sees(u, v) -- 5
law symm   = ∀ w u,   sees(w, u) ⇒ sees(u, w)              -- B
```

Nothing there mentions a modality. Each condition is about `sees` alone, which is exactly what makes
them composable — a theory may take any subset without the rest:

| Logic | Conditions open |
|---|---|
| K | none |
| T | `refl` |
| S4 | `refl`, `trans` |
| B | `refl`, `symm` |
| K5 | `euclid` |
| S5 | `refl`, `trans`, `euclid` |
| KD | `serial` |
| KD45 | `serial`, `trans`, `euclid` |

**And the choice is one line in the `.hp` file:**

```
axioms [refl, trans, euclid, symm]
```

The question *"did you mean K5 or S5?"* is therefore never asked of the language. Both are writable,
both are checked by the same kernel, and the `axioms [...]` line of each `.hp` says which set its
theorems may lean on. A proof that reaches for a law outside the list is refused, so
`"proved under T and 5"` is a different statement from `"proved"`, and the file keeps them apart.

### The theorems worth having are the ones nobody gives you

```
theorem b_from_t_and_5 : ∀ w u, place(w) ⇒ sees(w, u) ⇒ sees(u, w)
proof
  intro w u declared a
  have here : sees(w, w) by apply refl [declared]
  apply euclid [a, here]
qed
```

Take `euclid` at `(w, u, w)`: from `sees(w,u)` and `sees(w,w)` — which T gives for nothing — it
concludes `sees(u,w)`. **So a reflexive euclidean relation is symmetric, and S5 has axiom B without
anyone stating it.** Axiom 4 comes the same way. That is why S5's accessibility is an equivalence
relation, and why S5 contains S4 and B without being handed either.

One step there is easy to get wrong. Turning `sees(w,u)` into `sees(u,w)` needs `euclid` at
`(w, u, w)`, whose **second** premise is `sees(w,w)`. Writing `euclid [a, a]` instead concludes
`sees(u,u)` — a true thing, and not the one you needed.

### Two restrictions that are the language telling you something

**Reflexivity needed a premise.** Every textbook writes T as *every world sees itself*:

```
law refl = ∀ w, ⊤ ⇒ sees(w, w)        -- HS0020
```

That does not compile. `⊤` is not an atom, so `w` is bound by nothing and the rule's head would have
to **invent a world**. The fix is a generator relation — a `place` the host asserts — and the claim
weakens honestly: not *every world sees itself* but *every world somebody declared sees itself*. The
theorem says so, in its own text.

**Seriality is the same shape, one law further on.** Axiom D is `∀ w, ∃ u, sees(w, u)` — *every world
sees something* — and a Horn rule cannot conclude that something **exists** (`HS0022`). So the
successor is named:

```
def seed(from: Place, to: Place)
law serial = ∀ w u, seed(w, u) ⇒ sees(w, u)
```

**The consequence is worth stating plainly: axiom D holds of a frame exactly insofar as something
seeded it.** A world nobody seeds a successor for has none, and nothing complains. D is a
precondition on the runtime, not an inference — and a reader who expects otherwise will be surprised
by a frame that quietly fails to be serial.

Both restrictions are in [diagnostics](../hilbert/06-diagnostics.md), with the shapes that fix them.

### Run it

```bash
dotnet run --project src/MLambda.AI.Logic -- Worlds
```

The output is worth reading twice. Asked what `a` sees, the engine answers `a, b, c` — and `a`
itself is there even though nobody declared it a place, so reflexivity never touched it. Symmetry
turned `a→b` into `b→a`, and transitivity composed them. **Open a different set of conditions and
that answer changes**, which is the whole lesson in one line of output.

---

## Counting — a claim that is decided, not assumed

### There is no engine in this sample

`Counting.hs` declares a sort and a single predicate, and no laws at all:

```
theory Counting (Double)
{
  def counted(value: Double)
}
```

A theory with no laws is a legitimate theory. It says what there is, and nothing about what follows
— which is right here, because what follows is **decided**.

### Two tactics, and what they hand the kernel

```
theorem the_square_of_a_sum : (a + b) * (a + b) = a * a + 2 * a * b + b * b
proof
  ring
qed
```

**`ring`** normalises both sides to the same polynomial. If they are the same polynomial, they are
equal for every value of every variable — no hypotheses needed, which is why these claims have free
variables and no `∀`.

```
theorem a_strict_step_survives : ∀ x y z, x < y ⇒ y <= z ⇒ x < z
proof
  intro x y z first second
  linarith
qed
```

**`linarith`** refutes the negation of the goal. Each premise says some polynomial is `≥ 0`; so does
the negated goal. A non-negative combination of them is itself `≥ 0` — so if every variable cancels
and a **negative constant** is left, the premises cannot all have held, and the goal was true.

**The kernel adds that combination up itself, with exact rational arithmetic, and looks at the
sign.** It never runs the search that found the multipliers. That is the part worth understanding:
the search may be clever, buggy or lucky, and it does not matter, because what is checked is the
arithmetic of the certificate.

This one needs the strictness bookkeeping to be real: the combination reaches exactly zero, and only
the strict premise `x < y` makes zero a contradiction.

### `axioms []` is the point

```
open Counting
axioms []
```

Every theorem stands on its own certificate: `ring` normalises a polynomial and `linarith` finds a
Farkas combination, and neither consults a law.

**Careful: `axioms []` is not what makes that true.** To the kernel an empty axiom list means *every*
law is allowed, and a list that names laws restricts a file to exactly those. What makes this corpus
assume nothing is that `Counting.hs` declares no laws at all, so there is nothing to lean on. **There
is nothing in this corpus a reader has to take on trust**, which is a stronger thing to be able to say
than "these theorems are proved".

### A theory that proves but does not run

`Counting.hs` quantifies over `Double` so the certificates have numbers to work with. The proof
checker is happy: it only parses the theory. The Shin lowering is not — `Double` is a built-in sort
and may not be a theory's own (`HS0040`).

It is right not to be, because there is **no engine worth having here**: none of these claims is a
rule anything could run. One line in the project file says so:

```xml
<HilbertShinFile Remove="$(MSBuildProjectDirectory)\Counting.hs" />
```

which leaves the theory in the proof inputs and out of the engine inputs. Compare `Worlds`, which
*does* have an engine worth having, and where the right answer was to bind the variable instead.
**The choice is about the theory, not about the error.**

---

## What the tests assert

[`WorldsTests.cs`](../../test/MLambda.AI.Logic.Tests/WorldsTests.cs) ·
[`CountingTests.cs`](../../test/MLambda.AI.Logic.Tests/CountingTests.cs)

- A declared world sees itself; an undeclared one sees nothing.
- Transitivity composes a chain, and symmetry runs it backwards.
- A seeded successor becomes accessible — and only a seeded one.
- B and 4 both come back `Proved`, from T and 5.
- Every `Counting` theorem is `Proved` by the kernel while the test runs, and a slack step keeps a
  strict inequality strict.

## Try it yourself

1. Remove `symm` from the `axioms [...]` line in `Worlds.hp` and build. Which theorems still check?
   You have just moved from S5 to S4 and found out what it cost.
2. Add a theorem to `Counting.hp` that is **false** — `a + b = a` — and watch the build fail. The
   certificate cannot be produced, and `HilbertProofStrict` will not let it through.
3. Give `Worlds` a second declared place and ask what it sees.
