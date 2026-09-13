# Reinforcement learning, L2 — QTable and Sarsa

**Source:** [`QTable.hb`](../../src/MLambda.AI.Learning/QTable.hb) ·
[`Sarsa.hb`](../../src/MLambda.AI.Learning/Sarsa.hb)

Two learners, one world, and **one line of source** between them. That line is the whole page.

## The world

```
  row 0   .  .  .  .  .  .  .
  row 1   .  .  .  .  .  .  .
  row 2   S  C  C  C  C  C  G
```

Start at `S`, reach `G`. Every step costs 1. Stepping onto the cliff `C` costs 100 and sends you back
to the start. The shortest path is up one, along row 1, and down — eight steps, right beside the edge.

This is Sutton & Barto's cliff walk (§6.5), made smaller for a measured reason — see the end of the page.

## The temporal-difference step

Both learners keep a table `q`: for every state and action, how good that action is believed to be.
After each step they nudge one entry toward a **target**:

```
target  =  reward  +  γ · (value of where you landed)
```

and move the entry part of the way there. At rate 0.5 it moves exactly halfway — which
[`Identities.hp`](../../src/MLambda.AI.Learning/Identities.hp) proves, and a test checks against a
single step from an all-zero table: one entry becomes −0.5, and all 83 others stay exactly zero.

`QTable(21, 4)` is this cliff; `QTable(500, 6)` would be an inventory problem. **One declaration,
both sizes** — the constructor takes the shape, `train` takes the experience.

## The one line

Everything hangs on *"value of where you landed"*. There are two honest answers.

**Q-learning** — `qTarget` — uses the **best** action available in the next state:

```
qTarget(q, marked(stateIds(q), s2), rw, gamma, done)
```

**Sarsa** — `sarsaTarget` — uses the action the learner **actually took** there:

```
sarsaTarget(q, marked(stateIds(q), s2), marked(actionIds(q), a2), rw, gamma, done)
```

That is it. One takes a maximum; the other takes the action that really happened.

A test shows it as one number each, with no episodes at all. Teach both tables that some action is
worth 10, then step into that state telling Sarsa the next action taken was a worthless one:

```
Q-learning aims at  0 + max(next) = 10   → the entry moves halfway, to 5
Sarsa aims at       0 + taken     = 0    → the entry stays at 0
```

Swap Sarsa's target for Q-learning's and that test fails — which was checked.

## What the difference does on a cliff

```
  Q-learning              Sarsa
  . . . . . . .           * * * * * . .
  * * * * * * *           * . . . * * *
  S C C C C C G           S C C C C C G
```

**Q-learning walks the edge.** Its target always assumes the best next action, so it learns the value
of a perfect greedy policy — and the perfect policy takes the shortest path. But the learner is not
following that policy. It explores with ε = 0.1, and one random step down from row 1 is a fall.
Q-learning's table never counts that risk, because it never counts its own exploration.

**Sarsa goes round.** Its target uses what it actually did, exploration included. So its table *has*
counted the occasional fall off row 1, and learned that two extra steps along the far row are cheaper
than a 1-in-40 chance of −100 per step.

**Neither is wrong.** Q-learning answers *"what is the best policy?"* Sarsa answers *"what is the best
policy for an agent that explores the way I do?"* On most worlds those are close enough not to
matter. On a cliff, they are the difference between walking the edge and not.

## What was measured, and what was not claimed

Everything above was run on four seeds before a word of it was written, and only what held on all four
is a test:

| Seed | Q-learning greedy path | Sarsa greedy path | Return, last 20 episodes (Q / Sarsa) |
|---|---|---|---|
| 1 | 8 steps, beside the cliff | 10 steps, via the far row | −29.5 / −11.2 |
| 2 | 8 steps, beside the cliff | 10 steps, via the far row | −30.4 / −16.4 |
| 3 | 8 steps, beside the cliff | **loops — never reaches the goal** | **−13.9** / −33.6 |
| 4 | 8 steps, beside the cliff | 10 steps, via the far row | −29.9 / −26.4 |

**Held on all four, so tested:** both learners improve; Q-learning never leaves the row beside the
cliff; Sarsa's greedy policy goes to the far row.

**Did not hold on all four, so not tested:** that Sarsa *earns more reward while learning*. That is
the textbook's famous claim, and on seeds 1, 2 and 4 it is true. On seed 3 it reverses — Sarsa's
policy has not settled after 120 episodes and still loops. The textbook result is an average over
many runs. From four runs of 120 episodes, this project cannot show it, so it does not say it.

## Why the world is small

Every `Train` call through the generated code costs about 9 milliseconds, and every `Act` about 3,
because each call interprets a graph. `Prelude/Networks.hb` warns about this directly: a fit driven
step by step from the host is correct and not fast.

A first attempt at the textbook's 4 × 12 cliff, 500 episodes, three seeds, was still running after ten
minutes. This 3 × 7 world keeps exactly what the lesson needs — a safe row and a row beside the cliff —
and trains both learners in about ten seconds.

## Run it

```bash
dotnet run --project src/MLambda.AI.Learning -- Cliff
```

## What the tests assert

[`GridworldTests.cs`](../../test/MLambda.AI.Learning.Tests/GridworldTests.cs)

- A single step moves the entry it should, and no other.
- Off-policy aims at the best next action; on-policy at the one taken.
- Both learners collect more reward as they learn.
- Q-learning walks the optimal path along the edge, and never visits the far row.
- Sarsa steps back from the edge.

## Try it yourself

1. Set ε to 0 for both learners. Do their paths still differ? Why not?
2. Raise the cliff penalty to −1000. Does Q-learning's path change?
3. Run seed 3 for 400 episodes instead of 120 and see whether Sarsa settles.
