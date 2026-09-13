# Reinforcement learning, L1 — Bandit

**Source:** [`src/MLambda.AI.Learning/Bandit.hb`](../../src/MLambda.AI.Learning/Bandit.hb)

## You cannot find the best without trying the rest

Three slot machines. One pays 0.1 each pull, one 0.5, one 0.9 — but you are not told which is which.
You have to pull them to find out.

Here is the whole problem. Every pull of a machine you already believe is good earns you something.
Every pull of one you know less about might teach you something. **You cannot learn which arm is
best without pulling arms that turn out not to be.** That trade — use what you know, or find out more
— is called *explore versus exploit*, and every reinforcement-learning method ever written is this
choice with more structure added.

A bandit is that choice with nothing else. No states, no maps, no moves — just arms.

## What the learner remembers

```
weight total  : Tensor[1, arms] from 0
weight counts : Tensor[1, arms] from 0

def means ≔ (total + start · (counts = 0)) / safe(counts)
```

Two things: how much each arm has paid in total, and how many times it was pulled. The **estimate**
of an arm is the first divided by the second — its average payout. Keeping them apart means you can
see exactly what an estimate *is*: nothing clever, just an average.

## Optimism: exploring without a coin flip

That `start · (counts = 0)` is the interesting part. An arm nobody has pulled is believed to be worth
`start`. Set `start` to 10 — more than any arm can pay — and watch:

```
  nothing tried yet          believed  10.00  10.00  10.00
  after five pulls of arm 0  believed   0.10  10.00  10.00
  after five pulls of arm 1  believed   0.10   0.50  10.00
  after five pulls of arm 2  believed   0.10   0.50   0.90
```

Every untried arm looks like the best one until it is tried — so the learner goes and tries it. That is
exploration, and **nobody flipped a coin** to make it happen. The optimism did it.

## Choosing is a set of chances, not a choice

```
def choose(eps) ≔ exploring(means, eps)
```

`choose` does not hand back an arm. It hands back **how likely each arm is to be picked**. Picking one
is the host's job — the program rolls the dice.

This is called ε-greedy: with chance ε, pick an arm at random; otherwise pick the best. And here is a
detail almost everybody gets wrong the first time.

**Picking at random can pick the best arm too.** So with ε = 0.3 and three arms, the best arm is not
chosen 70% of the time — it is chosen 70% of the time *plus* its share of the random picks:

```
0.7  +  0.3 ÷ 3  =  0.8
```

The program prints `0.800`, not `0.700`. Hilbert writes ε-greedy as a sum of two sets of chances
rather than an `if`, so this extra piece falls out of the arithmetic instead of having to be
remembered. A test checks it to nine decimal places.

## Ties split

Before anything is pulled, all three arms are believed worth 10. Which is best? None — they tie. So
the chances are a third each, not "arm 0, because it came first". After arm 0 is pulled, arms 1 and 2
still tie at 10, and they split the greedy share between them: `0.450` each.

Breaking a tie arbitrarily would be a decision nobody made. Splitting it is honest.

## Run it

```bash
dotnet run --project src/MLambda.AI.Learning -- Bandit
```

## What the tests assert

[`BanditTests.cs`](../../test/MLambda.AI.Learning.Tests/BanditTests.cs)

- An untried arm looks best when optimism is high.
- A pulled arm is estimated at what it pays.
- Choosing answers a set of chances that add up to one.
- The best arm's chance is exactly `1 − ε + ε/A` — and at ε = 0, the best arm gets everything.
- Ties split.
- **The estimates rank the arms correctly** — which is a claim about what the learner *believes*.

That last one is worded carefully on purpose. The test does **not** say "the learner always pulls the
best arm". An ε-greedy learner deliberately does not — that is what exploring means — and a test that
demanded it would be asserting that exploration is broken.

## Try it yourself

1. Set `start` to 0 instead of 10. What happens to exploration now?
2. Change ε to 0 and to 1. Which one never learns anything new, and which one never uses what it knows?
3. Add a fourth arm and check that the best arm's chance becomes `1 − ε + ε/4`.
