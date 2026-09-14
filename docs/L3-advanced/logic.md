# Logic, L3 — Sorts, and the law that is missing

**Source:** [`src/MLambda.AI.Logic/Sorts.hs`](../../src/MLambda.AI.Logic/Sorts.hs) ·
[`Sorts.hp`](../../src/MLambda.AI.Logic/Sorts.hp)

This page is built around something the corpus **does not** prove. That is not a gap in the sample;
it is the sample.

## A sortal does two jobs

A *sortal* is a kind-term — horse, passenger, person — and it does two jobs at once. Taking both
seriously is the whole subject.

1. **It classifies.** It sorts things into those that are horses and those that are not.
2. **It individuates.** It says what makes *this* horse the same horse as *that* one — how to count
   them, when one ends and another begins.

Adjectives only do the first. "Red" tells you which things are red and tells you nothing whatever
about how to count reds: is a red car with a red door one red or two? The question has no answer,
and that is the difference between an adjective and a sortal.

## Which is why identity has three places

```
def same(left: Thing, right: Thing, under: Kind)
```

Not `same(x, y)` but `same(x, y, s)` — **the same *what***. Two things may be the same F and
different Gs, and nothing in this logic collapses that. Ordinary first-order identity cannot express
it at all: `x = y` is either true or false, and there is nowhere to put the sortal.

## The theory

```
law sub_refl  = ∀ s,     kind(s) ⇒ sub(s, s)
law sub_trans = ∀ s t v, sub(s, t) ∧ sub(t, v) ⇒ sub(s, v)

law inst_sub  = ∀ x s t, inst(x, s) ∧ sub(s, t) ⇒ inst(x, t)

law disjoint_symm     = ∀ s t,   disjoint(s, t) ⇒ disjoint(t, s)
law disjoint_down     = ∀ s t u, disjoint(s, t) ∧ sub(u, s) ⇒ disjoint(u, t)
law disjoint_excludes = ∀ x s t, disjoint(s, t) ∧ inst(x, s) ∧ inst(x, t) ⇒ absurd(x)

law same_refl  = ∀ x s,     inst(x, s) ⇒ same(x, x, s)
law same_symm  = ∀ x y s,   same(x, y, s) ⇒ same(y, x, s)
law same_trans = ∀ x y z s, same(x, y, s) ∧ same(y, z, s) ⇒ same(x, z, s)
law same_up    = ∀ x y s t, same(x, y, s) ∧ sub(s, t) ⇒ same(x, y, t)
```

Three groups: subsumption is a pre-order, classification climbs it, and identity is an equivalence
**on the instances of each sort**.

`sub_refl` needed the same generator-relation treatment as `Worlds`' reflexivity — `kind(s)` is what
binds `s`, since `⊤ ⇒ sub(s, s)` would have the head invent a sort. See
[L2](../L2-practitioner/logic.md#two-restrictions-that-are-the-language-telling-you-something).

## How this theory says "no"

There is no `¬` goal the elaborator can introduce, so emptiness is written as a derivation **of
absurdity** from the instance that would witness it:

```
law disjoint_excludes = ∀ x s t, disjoint(s, t) ∧ inst(x, s) ∧ inst(x, t) ⇒ absurd(x)
```

Which is what "S and T are disjoint" means anyway: nothing can be both. And because disjointness is
inherited downward, a tabby bird is a contradiction even though nobody said tabbies and birds were
disjoint — `disjoint_down` reaches it from cats and birds.

## The non-theorem

Sameness **climbs**:

```
theorem sameness_climbs : ∀ x y s t, same(x, y, s) ⇒ sub(s, t) ⇒ same(x, y, t)
```

If alice and bob are the same passenger, and every passenger is a traveller, then they are the same
traveller. Fine.

**And it does not descend.** This claim is *false*:

```
∀ x y s t, same(x, y, t) ⇒ sub(s, t) ⇒ same(x, y, s)
```

Every person is a passenger. Alice and bob are the same **passenger** — same seat reservation, on
successive days. It does not follow, and must not follow, that they are the same **person**.

No rearrangement of the laws in `Sorts.hs` proves it. `same_up` goes one way only, deliberately.

### How an absence is recorded

Three ways, and the third is the one that matters:

1. **As a comment** in `Sorts.hp`, stating the false claim in full so a reader meets it.
2. **Against the corpus**, asserting no theorem of that name exists:

   ```csharp
   Assert.DoesNotContain(
       Theorem.Of(typeof(SortsProofs)),
       name => name.Contains("Descend", StringComparison.Ordinal));
   ```

3. **Against the running engine** — which is the real test:

   ```csharp
   var shared = await Sorted(engine.SortsSharing("alice", "bob"));

   Assert.Contains("passenger", shared);
   Assert.DoesNotContain("person", shared);
   ```

   That is non-vacuous: `passenger` **is** in the answer, so the query works and the absence of
   `person` is a real absence rather than a query that never returns anything. It was verified by
   mutation — adding a `same_down` law to the theory fails exactly this one test.

### And not as `sorry`

`sorry` is the marker for a proof you intend to finish. `HilbertProofStrict` rejects it, and this
absence must not be written that way even if it did not:

> **`sorry` means "not proved yet". This is "not provable, on purpose".** The two must not look
> alike, because a reader who finds `sorry` will try to discharge it, and a reader who finds this
> should understand why nobody can.

## A general point about corpora

A corpus that quietly stops short is worse than one that says where it stopped — because it reads
like completeness. Every absence in this repository is deliberate and recorded, and a test fails the
day one is silently filled in.

## Run it

```bash
dotnet run --project src/MLambda.AI.Logic -- Sorts
```

## What the tests assert

[`SortsTests.cs`](../../test/MLambda.AI.Logic.Tests/SortsTests.cs)

- Classification climbs the whole order: fluffy is a tabby, and therefore a cat, and therefore a
  mammal — two steps nobody asserted.
- A thing in two disjoint sorts is a contradiction, and disjointness reaches down into subsorts.
- A consistent world reports **no** contradictions — non-vacuous, because the two tests above show
  the query reports them when there are any.
- Sameness climbs.
- **And it does not descend**, asserted against the running engine.

## Try it yourself

1. Add `law same_down = ∀ x y s t, same(x, y, t) ∧ sub(s, t) ⇒ same(x, y, s)` to `Sorts.hs` and run
   the tests. Exactly one fails. Now read it, and decide whether you believe the law.
2. Make `tabby` disjoint from `mammal` and ask what fluffy is. The contradiction is derived, not
   detected — nothing scans for inconsistency; it simply follows.
3. Assert that alice and bob are the same person and ask again which sorts they share. Climbing does
   the rest.
