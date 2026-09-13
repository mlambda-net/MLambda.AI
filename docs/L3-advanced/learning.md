# Reinforcement learning, L3 — Identities

**Source:** [`src/MLambda.AI.Learning/Identities.hp`](../../src/MLambda.AI.Learning/Identities.hp)

This is where Logic and Learning meet, and it is the argument for why this repository has both.

## The algebra nobody checks

A reinforcement-learning textbook does a lot of algebra on a whiteboard. The temporal-difference error
is a backup less an estimate. A step size of one replaces the estimate; a step size of zero changes
nothing. A two-step return unrolls into rewards weighted by powers of γ. ε-greedy's chances add up to
one.

Every implementation silently assumes all of it. `Identities.hp` **proves** it, and the build refuses to
finish if one stops holding:

```
theorem a_half_step_lands_halfway : c + (1 / 2) * (t - c) = (c + t) / 2
proof
  ring
qed

theorem a_two_step_return_unrolls : r1 + g * (r2 + g * v) = r1 + g * r2 + g * g * v
proof
  ring
qed

theorem an_epsilon_greedy_row_sums_to_one : (1 - e + e / 4) + e / 4 + e / 4 + e / 4 = 1
proof
  ring
qed
```

`ring` normalises both sides to the same polynomial. If they agree, the identity holds for every value
of every variable — no examples, no tolerance, no floating point.

And `axioms []`: **nothing is assumed.** Each theorem stands on its own certificate, and a test asserts
the axiom list of every one is empty.

The step-size theorems are not decoration. `GridworldTests` trains at rate 0.5 and checks that one step
moves an entry halfway. `a_half_step_lands_halfway` is what turns that from an observation into a fact.

One honest note: the first identity, *the TD error is the backup less the estimate*, prints as
`r + g · v − c = r + g · v − c`. Once the parentheses are parsed away it very nearly is `X = X`. Upstream,
its point is that two independently written definitions in `Prelude/Temporal.hb` agree; here, on its
own, it is the weakest theorem in the file, and worth knowing as such.

## What is not here, and why

The more important half of the page. A corpus that quietly stopped short would read like one that was
complete — which is worse than one that says where it stopped.

**The convergence of value iteration.** The Bellman backup is a γ-contraction in the sup norm, so
Banach's theorem gives a unique fixed point. The one-coordinate pieces are provable here; the sup-norm
step needs a maximum over a finite index *and a limit*. This language's tactics are propositional,
modal, inductive, linear-arithmetic and algebraic. Claiming the theorem would be claiming a proof
nobody wrote.

**Q-learning's almost-sure convergence** (Robbins–Monro) is further still, and not attempted.

**The geometric sum at every n.** It wants induction over a sum indexed by a natural number. The
two-step return is one instance of that family, and it is *decided*; the family is not.

**A division by a variable.** Outside a polynomial ring, so `1/(1 − γ)` appears only at literal
denominators, and ε-greedy is proved over exactly four actions. The arithmetic is the same at any n;
the certificate is only available at one.

## Two refusals that look the same and are not

Proving that *a step toward a larger target never overshoots it* took three attempts, and taught more
than the theorem did.

**First, at a symbolic step size `a`:**

```
∀ c t a, c ≤ t ⇒ 0 ≤ a ⇒ a ≤ 1 ⇒ c + a · (t − c) ≤ t
```

Refused: *"`linarith` found no linear combination that refutes c + a · (t - c) ≤ t."*

**That refusal is correct.** `linarith` works by finding non-negative multiples of the hypotheses that
add up to a contradiction. But this claim needs the **product** of two hypotheses —
`(1 − a) · (t − c) ≥ 0` — and adding can never produce a product. It is genuinely beyond a linear
certificate, and there is no `nlinarith` to reach for (`HP0003`). So the theorem is stated at a literal
step size, where the product disappears.

**Second, at the literal step size one half:**

```
∀ c t, c ≤ t ⇒ c + (1 / 2) * (t − c) ≤ t
```

Refused again — and **this one is linear**. It reduces to `c ≤ t`. A certificate exists.

So the next step was to change one thing at a time:

| Written as | `linarith` |
|---|---|
| `c / 2 ≤ t / 2` | proves |
| `0.5 * c ≤ 0.5 * t` | proves |
| `c + (t − c) / 2 ≤ t` | proves |
| `(1 / 2) * c ≤ (1 / 2) * t` | **refused** |

`linarith` does not fold `1 / 2` into a constant when it *multiplies* something. That is a gap in
Hilbert, not in the mathematics — `ring` accepts the same expression, and `0.5` goes straight through.

The two refusals print almost identical messages. **One is a true limit of the method and one is an
implementation gap**, and the only way to tell them apart was to vary the spelling until the boundary
showed.

The shipped theorem is written `(t − c) / 2`, with a comment saying why. And a test pins the gap: it
proves the working spellings through `Prover.Prove` on inline source — a control, so the test cannot
pass for a broken harness — then asserts the `(1 / 2) *` spelling is still refused. **The day Hilbert
fixes it, that test fails and says what to delete.**

## Run it

```bash
dotnet run --project src/MLambda.AI.Learning -- Identities
```

Every theorem is proved again as it prints, not read back from the build.

## What the tests assert

[`IdentitiesTests.cs`](../../test/MLambda.AI.Learning.Tests/IdentitiesTests.cs)

- Every theorem in `Identities.hp` proved by the kernel while the test runs, one test each, with a
  guard that fails if a theorem is added without one.
- None of them assumed anything.
- The step size the gridworld uses is the one proved safe.
- `linarith` proves a half written as a divisor or a decimal — **and today refuses it written as
  `(1 / 2) *`**, pinned so the fix is noticed.

## Try it yourself

1. State `a_two_step_return_unrolls` for three steps and prove it. Then try to state it for *n* steps,
   and find out exactly where the language stops you.
2. Write `an_epsilon_greedy_row_sums_to_one` for three actions. Then try `n` actions.
3. Write a *false* identity — `c + (t − c) / 2 = t` — and read what the kernel says.
