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

This is also why a proving theory is not always a lowerable one: an axiom schema like
`⊤ ⇒ holds(imp(p, imp(q, p)))` has head variables nothing binds, so it is fine to *reason about* and
`HS0020` to *run*. Import only the Proof targets for such a theory.

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
