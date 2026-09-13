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

## A query that answers nothing

Not an error code — it compiles and returns an empty stream.

A query's generated method takes its parameters in **declaration** order, but the goal is built in
**body-atom** order, and the two are assumed to match. When they do not, inputs and outputs silently
swap:

```
query members(kind: Thing, who?: Thing) :- is_a(who, kind)
```

declares `(kind, who?)` but its body atom is `(who, kind)`. `Members("animal")` binds `"animal"` to
`who` and asks `is_a("animal", ?)` — which is empty.

**Until this is fixed in the compiler:** write the body atom's arguments in the same order as the
query's parameters, inputs first and the `?` output last. The reverse query in `Animals.hs` is
commented out rather than worked around, because the workaround needs a reversed relation and a
first sample is the wrong place to teach one.

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

### The claim is linear and `1 / 2` is in the wrong place — a gap in Hilbert

Measured, one spelling at a time, all from `c ≤ t`:

| Written as | `linarith` |
|---|---|
| `c / 2 ≤ t / 2` | proves |
| `0.5 * c ≤ 0.5 * t` | proves |
| `2 * (t − c) ≥ 0` | proves |
| `c + (t − c) / 2 ≤ t` | proves |
| `(1 / 2) * c ≤ (1 / 2) * t` | **refused** |
| `c + (1 / 2) * (t − c) ≤ t` | **refused** |

Today `linarith` does not fold `1 / 2` into a constant when it **multiplies** something, so `(1/2) · c`
is treated as a product of two terms. It is not the mathematics — `ring` accepts
`(1 / 2) * (t − c)` without complaint, and `0.5 * c` goes through `linarith` fine.

**The shape that fixes it:** write the half as a divisor, `(t − c) / 2`, or as a decimal.

`IdentitiesTests` pins the refusal against inline source, with a control proving the other spellings
through the same harness — so the day Hilbert fixes it, a test fails and says the workaround can go.

## `CS0117` inside `Generated/Hilbert` — a `fn` calling another `fn`

```
error CS0117: 'Math' does not contain a definition for 'Scatter01'
error CS0117: 'Tensor' does not contain a definition for 'Scatter01'
```

The error is **C#**, in a generated file — but the cause is in the `.hb`:

```
fn scatter01(price, seed) ↦ …
fn loanToValueOf(price, seed) ↦ 0.60 + 0.35 · scatter01(price, seed)     -- does not compile
```

A `fn` that calls another `fn` in the same file is accepted by the Hilbert compiler, and the emitter
then routes the call to `System.Math.Scatter01` in the `double` overload and
`MLambda.Hilbert.Runtime.Tensor.Scatter01` in the tensor ones — as though your function were a built-in
like `floor` or `exp`.

**The shape that fixes it:** a helper that other definitions call is a `def`.

```
def scatter01(price, seed) ≔ …
fn loanToValueOf(price, seed) ↦ 0.60 + 0.35 · scatter01(price, seed)     -- compiles
```

That is how every Prelude module is written: `def` for anything reused, `fn` for what a caller calls.

**Whatever the intended rule, the silence is a defect.** A program the Hilbert compiler accepts should
not produce C# that fails to compile, and the error should name the `.hb` line rather than a generated
one. Found writing [`Credit.hb`](../../src/MLambda.AI.Actuarial/Credit.hb).
