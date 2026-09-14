# Agents, L3 — Agency, the theory underneath

**Source:** [`src/MLambda.AI.Agent/Agency.hs`](../../src/MLambda.AI.Agent/Agency.hs) ·
[`Agency.hp`](../../src/MLambda.AI.Agent/Agency.hp)

Every agent in this project extends one theory. This page is about that theory, and about the two
places where what BDI logic *wants to say* and what a Horn engine *can say* come apart — because
both times, the gap is instructive rather than embarrassing.

## Three attitudes, three relations

```
def believes(who: Agent, from: World, to: World)
def desires (who: Agent, from: World, to: World)
def intends (who: Agent, from: World, to: World)

def holds(at: World, prop: Prop)

modality B(i) over believes at holds
modality D(i) over desires  at holds
modality I(i) over intends  at holds
```

Each attitude is a **relation between worlds**, indexed by the agent. `B(i) p` is then the standard
translation: *p holds at every world i can reach by `believes`*.

The agent index comes first, which is the epistemic literature's convention — `K_a p` reads
`knows(a, w, u)`. An unindexed relation could not tell two agents' beliefs apart, and the whole
subject is about agents that believe different things.

## Belief is KD45; wanting is not

```
law transB  = ∀ i w u v, believes(i, w, u) ∧ believes(i, u, v) ⇒ believes(i, w, v)   -- 4
law euclidB = ∀ i w u v, believes(i, w, u) ∧ believes(i, w, v) ⇒ believes(i, u, v)   -- 5
```

Axiom 4 is *positive introspection* — an agent knows what it believes. Axiom 5 is *negative
introspection* — it knows what it does not believe. Belief gets both, plus consistency, which is
KD45.

**Desire and intention get consistency and nothing else.** There is no `transD`, no `euclidD`, and
their absence is the content: an agent does not automatically want what it wants to want. That is
Rao and Georgeff's own assignment, and the test asserts it by checking that belief's theorems lean
on `transB` and on **no** `transD` or `transI`.

## Where BDI logic and Horn come apart

### Seriality wants an existential

Axiom D — *an agent may not believe both p and ¬p* — is, as a frame condition:

```
∀ w, ∃ u, R(w, u)        -- HS0022
```

A Horn rule **cannot conclude that something exists**. So the successor is named, and the frame
condition follows from a rule that binds `u` in its body:

```
def beliefSucc(who: Agent, from: World, to: World)
law serialB = ∀ i w u, beliefSucc(i, w, u) ⇒ believes(i, w, u)
```

**The consequence deserves saying plainly: seriality holds by construction of the seeding, not by
inference.** A world nobody seeds a successor for has none, and nothing complains. Axiom D becomes a
**precondition on the runtime** rather than a theorem, and the strongest thing `Agency.hp` can prove
is that a *seeded* successor is accessible:

```
theorem a_seeded_belief_is_accessible : ∀ i w u, beliefSucc(i, w, u) ⇒ believes(i, w, u)
```

A test asserts that this claim contains `⇒` and no `∃`, because what the theorem *cannot* say is the
part a reader most needs to know.

### Realism wants an implication under a universal

Strong realism says *what an agent intends, it desires*:

```
I(i) p ⇒ D(i) p
```

Translate it and the problem appears:

```
(∀u, intends(i,w,u) ⇒ holds(u,p)) ⇒ (∀u, desires(i,w,u) ⇒ holds(u,p))
```

That is an implication under a universal, **in a head**, which Horn cannot express — `HS0020`, for
exactly the reason an existential cannot.

**Correspondence theory gives the expressible form.** `□_I p ⇒ □_D p` holds precisely when
`R_D ⊆ R_I`, so realism is an *inclusion between relations*:

```
law realism = ∀ i w u, desires(i, w, u) ⇒ intends(i, w, u)
```

One line, first-order, and **the modal reading is unchanged** — only the spelling is. This is the
same move `Worlds.hs` makes for reflexivity and transitivity, and it is worth recognising as a
general technique: *when a modal axiom will not fit, ask what condition on the accessibility
relation it corresponds to.*

A test asserts that the realism claim mentions `desires` and `intends` and contains no `D(` or `I(`
at all — because the point is that the modality has been traded for a relation.

## What follows, in two steps

```
theorem a_seeded_desire_is_intended : ∀ i w u, desireSucc(i, w, u) ⇒ intends(i, w, u)
proof
  intro i w u seeded
  have wanted : desires(i, w, u) by apply serialD [seeded]
  apply realism [wanted]
qed
```

Seriality makes the seed a desire; realism carries it across to intention. **Every "an agent that
wants X will try for X" claim reduces to this**, and it is worth seeing it reduce — two named steps,
neither of them mysterious.

## Proved twice

```csharp
[Fact]
public void Theorem_belief_composes() => Theorem.Proved(AgencyProofs.BeliefComposes());
```

Every theorem is checked at build time *and* re-proved in the test process. The generated
`AgencyProofs.BeliefComposes()` carries no verdict: it reads the theory and the proof embedded in the
assembly, hands both to the kernel, and lets it decide again.

The distinction is real: **a verdict from a build is a fact about that build.** Re-proving is a fact
about the mathematics, available to any host that wants to ask.

## Run it

```bash
dotnet run --project src/MLambda.AI.Agent -- Agency
```

It prints every theorem with the kernel's answer, proved as the program runs.

## What the tests assert

[`AgencyTests.cs`](../../test/MLambda.AI.Agent.Tests/AgencyTests.cs)

- All seven theorems proved at build time, and all seven proved again by the kernel in the test.
- Belief is introspective: `belief_composes` and `belief_is_euclidean` both prove.
- A seeded desire becomes an intention in two steps.

## Try it yourself

1. Add `law transD` to `Agency.hs` and prove that desire is introspective. It will check — and then
   ask yourself whether you believe it about any agent you have met.
2. Reverse `realism` to `intends ⇒ desires` and see which theorem stops proving.
3. Write a theorem that needs both `serialB` and `transB`, then remove one of them from the
   `axioms [...]` line in `Agency.hp` and see the build refuse it.
