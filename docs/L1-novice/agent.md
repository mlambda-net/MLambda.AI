# Agents, L1 — Thermostat

**Source:** [`src/MLambda.AI.Agent/Thermostat.ha`](../../src/MLambda.AI.Agent/Thermostat.ha)

## What makes this an agent and not an `if`

Here is a thermostat as ordinary code:

```csharp
if (temperature < 18) { heater.On(); }
```

And here is one as an agent:

```
belief temperature(degrees: Int)

desire Comfortable reachable by [Warm, Cool]

intend Warm for Comfortable when B(self) temperature(t) ∧ t < 18
achieved Comfortable when B(self) temperature(t) ∧ t ≥ 18 ∧ t ≤ 24
```

Three differences, and they are the whole subject.

1. **It holds a goal.** `Comfortable` is a thing the agent wants. The `if` version has no goal
   anywhere — the goal lives in the head of whoever wrote it.
2. **An intention is licensed only while the goal is unmet.** `achieved` says what being comfortable
   *is*, and once that holds the agent proposes nothing. Nothing had to remember to stop it.
3. **It does not act.** `intend Warm` means *I intend Warm, toward Comfortable*. The heater is never
   switched on by this file.

## The agent decides; the host acts

```
plan Warm ↦ Room.Heat
```

`Room.Heat` is a **label**. There is no `Room.Heat` anywhere in this repository and there does not
need to be. What the engine reports is:

> *I intend `Warm`, toward `Comfortable`.*

and `Program.cs` reads that and decides what it means. This seems like a small thing and is not: it
means the deliberation can be tested, printed, and reasoned about without anything happening to a
real heater.

## The five keywords

| Keyword | What it says | In `Thermostat.ha` |
|---|---|---|
| `belief` | what the agent can hold true | `belief temperature(degrees: Int)` |
| `desire … reachable by` | a goal, and the plans that could reach it | `desire Comfortable reachable by [Warm, Cool]` |
| `intend … for … when` | adopt a plan toward a goal when a condition holds | `intend Warm for Comfortable when …` |
| `achieved … when` | the goal is met — stop | `achieved Comfortable when …` |
| `plan … ↦` | the label a host acts on | `plan Warm ↦ Room.Heat` |

## And one the compiler insists on

```
commit open_minded
```

**You cannot write an agent without this.** The compiler refuses one that does not say how long its
intentions survive contact with the world, and the three answers are:

| Strategy | The intention survives until |
|---|---|
| `blind` | you believe the goal achieved — come what may |
| `single_minded` | achieved, **or** believed impossible |
| `open_minded` | achieved, or you stop holding the goal — reconsidered as beliefs change |

A thermostat is **open-minded**, because its world will not hold still. Somebody opens a window and
the right thing to do changes. An agent that blindly finished heating would be a worse thermostat
than a bimetallic strip.

[`Collector`](../L2-practitioner/agent.md) is single-minded, and the pair is the comparison worth
making once you have met both.

## One thing to know now and understand later

`B(self) temperature(t)` does **not** mean *look up this agent's temperature field*. It means
*`temperature(t)` is true in every world this agent takes to be possible* — which is what belief
means in modal logic, and why an agent can be wrong without being broken.

The engine actually runs something slightly different, for a reason that has its own page at
[L2](../L2-practitioner/agent.md#the-two-readings). You do not need that yet. You do need to know
that `B(self)` is a claim about possibility and not a field lookup, because everything above is
built on it.

## Run it

```bash
dotnet run --project src/MLambda.AI.Agent -- Thermostat
```

```
  degrees   intends      goal met
  ───────   ──────────   ────────
       10   Warm         —
       18   —            Comfortable
       24   —            Comfortable
       30   Cool         —
```

One agent, one goal, four temperatures. The goal never changes and what it intends does.

## What the tests assert

[`ThermostatTests.cs`](../../test/MLambda.AI.Agent.Tests/ThermostatTests.cs)

- Cold means it intends to warm; hot, to cool.
- The intention names the **goal it serves**, not just the plan — which is what lets a host ask
  *why*, not only *what*.
- Comfortable means it intends nothing at all, and the goal is met.
- The band is closed at both ends: 18 and 24 are comfortable, 17 and 25 are not. An off-by-one here
  would be invisible in every other test in the file.
- One thermostat's beliefs never become another's.
- A belief that changes changes what it intends.

## Try it yourself

1. Widen the comfortable band to 16–26 and re-run. Which rows change?
2. Add a third plan, `Vent`, for hot weather, and give it a condition of its own.
3. Change `commit open_minded` to `commit blind` and read `Cleaner`'s page to see what you gave up.
