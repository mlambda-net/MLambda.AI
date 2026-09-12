# Agents, L2 — Collector and Cleaner

**Source:** [`Collector.ha`](../../src/MLambda.AI.Agent/Collector.ha) ·
[`Cleaner.ha`](../../src/MLambda.AI.Agent/Cleaner.ha)

This is the most important page in the agent project, because of the first section.

---

## What `B(self)` actually means

At [L1](../L1-novice/agent.md) you were told that `B(self) temperature(t)` is not a field lookup.
Here is what it is.

An agent has, in the model, a set of worlds it takes to be possible — ways things might be, for all
it knows. `B(i) p` says:

> **p holds at every world i takes to be possible.**

That is belief. Not *a record with p in it*, but *p is true however things turn out to be, as far as
this agent can tell*. It is what makes an agent able to be **wrong without being broken**: its
possible worlds simply did not include the actual one.

And it is what gives realism content. `I(i) p ⇒ D(i) p` — *what an agent intends, it desires* — is a
real claim under this reading, and a naming convention under the field-lookup one.

## The two readings

**And then the engine runs something else.**

A modality in a rule **body** does not compile. The error is `HS0052`, and the reason is structural:
the modality is a quantifier over worlds, and a Horn body cannot carry one. So this line:

```
intend Collect for Enough when B(self) gathered(seen) ∧ seen < 5
```

lowers to the **ground reading** — `gathered(id, seen)`, at the actual world. Not at every possible
world. Just the one.

So the same line means two things in two places:

| | What it says |
|---|---|
| As written, read modally | `gathered(seen)` holds at every world this agent takes to be possible |
| As lowered, what runs | `gathered(id, seen)` holds *here* |

**This is not a defect to hide.** It is the honest shape of every BDI implementation ever built — no
production agent evaluates a modality over possible worlds at run time. The difference is that here
**both readings are written down**, one of them is checked by a proof, and this page says which is
which.

`Agency.hp` proves things about the modal reading. The engine runs the ground one. A reader who is
not told this will eventually be surprised, and being surprised by your own agent framework is the
thing worth preventing.

---

## Commitment: a bet about the world

`Collector` differs from `Thermostat` in one keyword:

```
commit single_minded
```

| | |
|---|---|
| `Thermostat` | `open_minded` — reconsiders as beliefs change |
| `Collector` | `single_minded` — keeps the intention until achieved or impossible |

Run the Collector sample and watch the intention **survive every step short of the goal**:

```
  gathered   intends      goal met     credit
  ────────   ──────────   ──────────   ──────
         0   Collect      —            —
         3   Collect      —            —
         4   Collect      —            —
         5   —            Enough       Collect positive
```

The count changed three times and the intention did not waver. That is what single-mindedness buys:
**an agent that finishes things.**

It is also a bet. Kinny and Georgeff's result is the one to know:

> A **bold** agent, which rarely reconsiders, wins in a world that changes slowly — it wastes no
> time deliberating. In a world that changes fast it loses badly, because it pursues goals that
> stopped making sense.

So commitment is not a virtue, and there is no best answer. It is a claim about how fast your world
changes, and the language **forces you to state it** — `HS0060` if you do not. A framework that
defaulted this would be hiding the bet.

## `value` and `reward`

```
value remaining ≔ 5 − seen
reward Collect positive when B(self) gathered(5)
```

`value` is a derived quantity the conditions may use, so the arithmetic is written once.

`reward` is **the seam to reinforcement learning**. The same feedback that marks a plan good here is
the signal a learner maximises in `MLambda.AI.Learning`. The engine reports it as a `CreditRow` —
the plan, and the sign — and the test asserts both that credit is earned at the goal and that it is
*not* earned before.

---

## A plan library, and choosing nothing

`Cleaner` has two plans for one goal:

```
intend Sweep for Spotless when B(self) dirt(r) ∧ B(self) reachable(r)
intend Mop   for Spotless when B(self) dirt(r) ∧ B(self) reachable(r)
```

Given reachable dirt, the engine offers **both**:

```
  cleaner 'open'
    intends    Mop, Sweep
```

**The agent proposes and does not choose.** Choosing is the host's job — it knows which mop is
broken and how much time is left. An engine that chose would be hiding the decision somewhere
nobody can read it.

## Attention: what the agent is about

```
attention Cleaning initially
attention Blocked

when B(self) blocked(r) attend Blocked
```

Attention is what the agent is currently *minding*. When the way is shut it moves to `Blocked` —
and notice that **the goal has not changed**. It still wants a spotless house. What changed is what
it is about, which is a different thing, and the reason attention is a keyword rather than just
another belief.

Every focus must be declared (`HS0063`), so the set of things an agent can be about is fixed and
readable at the top of the file.

## Honest failure

```
impossible Spotless when B(self) dirt(r) ∧ B(self) blocked(r) because "there is dirt in a room I cannot get to"
```

An agent that cannot reach its goal has two options: keep trying forever, or stop and say why. Every
other BDI framework makes the second a log line somebody remembered to write. Here it is **part of
the agent**, and the compiler will not let you omit the reason:

```
error HS0001: expected 'because' and the reason a person will read, found the end of the line
```

That is the language taking a position, and it is the right one.

**One limit today.** The reason is required by the parser and then **discarded** — the generated
`UnreachableFact` carries the subject and the goal, and the sentence lives only in `Cleaner.ha`. So
a test can assert *that* a goal was abandoned and cannot yet assert *why*. The keyword still earns
its place: it makes the reason a required part of the agent rather than a comment somebody might
omit. See [diagnostics](../hilbert/06-diagnostics.md).

## Run them

```bash
dotnet run --project src/MLambda.AI.Agent -- Collector
dotnet run --project src/MLambda.AI.Agent -- Cleaner
```

## What the tests assert

[`CollectorTests.cs`](../../test/MLambda.AI.Agent.Tests/CollectorTests.cs) ·
[`CleanerTests.cs`](../../test/MLambda.AI.Agent.Tests/CleanerTests.cs)

- The intention survives all the way up, and stops exactly at the goal.
- Credit is earned at the goal — **and not before**.
- The whole cycle runs end to end driven by nothing but the count going up.
- A plan library offers every route and chooses none.
- Dirt it cannot reach licenses nothing, and the goal is reported abandoned.
- Attention moves when the way is shut, and stays put when it is not.
- One cleaner's locked door is not another's.

## Try it yourself

1. Change `Collector` to `commit open_minded` and re-run. The output does not change — and working
   out *why* is the exercise. (Hint: what would have to change for the strategies to differ?)
2. Give `Cleaner` a third plan and a condition only it satisfies.
3. Make the cellar both `blocked` and `reachable` and see what the agent does with a contradiction
   it was never told was one.
