# 6. Diagnostics

The errors you will actually hit, and the shape that fixes each.

## `HP0002` — a corpus may not admit a theorem

```
error HP0002: 'an_animal_found_automatically' is admitted by `sorry`,
and a corpus may not admit a theorem.
```

You wrote `sorry` — the "prove this later" marker — in a project with `HilbertProofStrict` on.

**The fix:** finish the proof, or delete the theorem. There is no third option, deliberately. A
half-proved corpus is a list of things somebody meant to prove, and it is worse than no corpus
because it reads like one.

## `HS0020` — a head the engine cannot conclude

Raised when a rule's head has an implication under a universal, or a head variable that no positive
body atom binds.

The classic case is *realism* in agent logic — "whatever an agent intends, it desires":

```
I(i) p ⇒ D(i) p          -- HS0020
```

**The fix is correspondence theory:** state it as relation *inclusion* rather than as a formula. The
desire relation is contained in the intention relation, and that is expressible.

### The `⊤ ⇒` case, which is easy to walk into

A law with no premise is written `⊤ ⇒ …`, and **it does not lower**:

```
law refl = ∀ w, ⊤ ⇒ sees(w, w)        -- HS0020: 'w' is not bound by any positive body atom
```

`⊤` is not an atom, so nothing binds `w`, and the head would have to invent a world. Reflexivity is
the usual victim, because "every world sees itself, from nothing" is exactly how a textbook states
it.

**The fix is a generator relation** — the same shape seriality already needs:

```
def place(id: Place)
law refl = ∀ w, place(w) ⇒ sees(w, w)
```

The claim weakens honestly, from *every world* to *every world the host declared*, and the theorem
should say so: `∀ w, place(w) ⇒ sees(w, w)`. See
[`src/MLambda.AI.Logic/Worlds.hs`](../../src/MLambda.AI.Logic/Worlds.hs).

**Or keep the unbound form and do not lower the theory.** An axiom schema like
`⊤ ⇒ holds(imp(p, imp(q, p)))` has head variables nothing binds, so it is fine to *reason about* and
`HS0020` to *run*. A theory like that imports only the Proof targets — which is what a proof corpus
does, and why a theory that proves is not always a theory that runs.

## `HS0022` — an existential in a head

Horn clauses cannot conclude that something *exists*. Seriality — every world can see some world —
is the usual victim:

```
∀ w, ∃ u, R(w, u)        -- HS0022
```

**The fix:** carry a generator relation that produces the witness, and let seriality hold by
construction of the seeding rather than by inference.

## `HS0052` — a modality in a rule body

A modality in the *body* of a rule does not compile. The rule lowers to the **ground reading**: a
condition written `B(self) count(seen)` runs as `count(id, seen)` at the actual world.

**The fix is to know which you meant.** Prove the modal claim in `.hp`, run the ground one, and say
so wherever a reader could be misled — the same line then means two things in two places, and that
is worth a sentence in the docs rather than a silent surprise.

## A query whose parameters are in a different order than its body — fixed

```
query members(kind: Thing, who?: Thing) :- is_a(who, kind)
```

declares `(kind, who?)` but its body atom is `(who, kind)`. The generated method used to pass its
arguments by position, so `Members("animal")` bound `"animal"` to `who` and asked `is_a("animal", ?)`:
an empty stream, and no error. A yes/no query with every parameter an input was worse: it answered
the **reversed** question.

MLambda.Shin now binds a query's inputs and outputs **by name**, so the declaration order and the
body order are free to differ. `Animals.hs` asks `is_a` from both ends, `kinds` and `members`.

## Writing an agent: three things the language insists on

None of these is a bug. Each is the `.ha` dialect taking a position, and the error message says so.

### `HS0060` — no commitment strategy

```
error HS0060: 'Thermostat' declares no commitment strategy;
write `commit blind`, `commit single_minded` or `commit open_minded`.
```

**You may not write an agent that does not say how long its intentions survive.** The three answers
are the three in the literature:

| Strategy | The intention survives until |
|---|---|
| `blind` | you believe the goal achieved — come what may |
| `single_minded` | achieved, **or** believed impossible |
| `open_minded` | achieved, or you stop holding the goal — reconsidered as beliefs change |

Kinny and Georgeff's result is the reason this is forced rather than defaulted: a **bold** agent that
rarely reconsiders wins in a world that changes slowly and loses badly in one that changes fast.
Commitment is not a virtue, it is a bet about the world, and a language that let you leave the bet
unstated would be hiding it.

### `HS0063` — an attention that was never declared

```
error HS0063: 'Blocked' is not an attention this agent declares.
```

`when … attend Blocked` may only name a focus the agent has. Declare it:

```
attention Cleaning initially
attention Blocked
```

So the set of things an agent can be about is fixed and readable at the top of the file, rather than
accumulating from wherever the rules happen to point.

### `HS0001` on `impossible` — the reason is not optional

```
error HS0001: expected 'because' and the reason a person will read,
found the end of the line
```

**An agent may not declare its goal impossible without saying why**, in a sentence somebody can
read, and the `because` must be on the same line as the condition:

```
impossible Spotless when B(self) dirt(r) ∧ B(self) blocked(r) because "there is dirt in a room I cannot get to"
```

**One limit worth knowing today:** the reason is required by the parser and then **discarded**. The
generated `UnreachableFact` carries the subject and the goal, and the sentence lives only in the
source. So a test can assert *that* a goal was abandoned, and cannot yet assert *why* — see
[`CleanerTests.cs`](../../test/MLambda.AI.Agent.Tests/CleanerTests.cs). The keyword still earns its
place: it makes the reason a required part of the agent rather than a comment somebody might omit.

## `HP0020` from `linarith` on a claim that is plainly linear

```
error HP0020: 'a_half_step_does_not_pass_its_target' is not proved:
`linarith` found no linear combination that refutes c + 1 / 2 · (t - c) ≤ t.
```

**Two very different causes produce this message, and it matters which one you have.**

### The claim really is nonlinear

At a symbolic step size `a`, the claim `c + a · (t − c) ≤ t` needs the product of two hypotheses —
`(1 − a) · (t − c) ≥ 0` — and a Farkas certificate only ever **adds** hypotheses together. No spelling
fixes that, and there is no `nlinarith` (`HP0003: 'nlinarith' is not a tactic`).

**The shape that fixes it:** state the claim at a literal, where the product disappears. `Identities.hp`
does this for the step size and for ε-greedy over four actions.

### The claim is linear and a constant is spelled `1 / 2` — fixed in Hilbert

`linarith` used to scale a term only by a literal number, so `(1 / 2) * c ≤ (1 / 2) * t` from `c ≤ t`
was refused as though `1 / 2` were a variable, while `c / 2` and `0.5 * c` proved. A multiplier that
folds to a constant is now a coefficient however it is written: `1 / 2`, `(1 + 1)`, on either side of
the product. `Identities.hp` spells its half step `(1 / 2) * (t − c)` again.

**One rule stays:** a **divisor** must still be a literal number. `c / (1 + 1)` is refused, because the
kernel reads only a literal divisor as a number, and `linarith` must never find a certificate the
kernel cannot check.

## `CS0117` inside `Generated/Hilbert` — a `fn` calling another `fn` — fixed

```
error CS0117: 'Math' does not contain a definition for 'Scatter01'
error CS0117: 'Tensor' does not contain a definition for 'Scatter01'
```

A `fn` that called another `fn` in the same file used to be accepted by the Hilbert compiler, which then
routed the call to `System.Math.Scatter01` and `Tensor.Scatter01`, as though the function were a
built-in like `floor`. The error named a generated file, not the `.hb` line.

The compiler now inlines a sibling `fn` exactly as it inlines a `def`, so

```
fn scatter01(price, seed) ↦ …
fn loanToValueOf(price, seed) ↦ 0.60 + 0.35 · scatter01(price, seed)
```

compiles. `def` is still the convention for a helper that nothing outside the file calls, which is how
the Prelude is written. Found writing [`Credit.hb`](../../src/MLambda.AI.Actuarial/Credit.hb).
