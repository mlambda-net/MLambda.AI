# Adjudicator — a veto, and a reserve that may refuse to exist

**Source:** [`Adjudicator.hs`](../../src/MLambda.AI.Actuarial/Adjudicator.hs) ·
[`Adjudicator.hp`](../../src/MLambda.AI.Actuarial/Adjudicator.hp) ·
[`Reserve.hb`](../../src/MLambda.AI.Actuarial/Reserve.hb) ·
[`AdjudicatorTests.cs`](../../test/MLambda.AI.Actuarial.Tests/AdjudicatorTests.cs) ·
[`ReserveTests.cs`](../../test/MLambda.AI.Actuarial.Tests/ReserveTests.cs)

## Disagreement is the design

On the capstone's house, valuation says buy, condition says negotiate, credit says walk. Each is right
about its own question. The adjudicator's job is not to average them — it is to decide what their
disagreement means.

## A veto, not a vote

```
any agent walks      → walk
else any negotiates  → negotiate
else someone voted   → buy
```

**Because the costs are not symmetric.** A wrong *walk* costs a missed house — bounded. A wrong *buy* can
cost a default, a repair bill the buyer cannot meet, or both — unbounded. So one agent seeing ruin
outranks two seeing a bargain, and a majority vote would be the wrong shape entirely.

`one_walk_is_enough` is a theorem in `Adjudicator.hp`: a single walk vote marks the purchase walked,
whatever anyone else said.

## "Everyone said buy" has no Horn form

A Horn rule has no universal in its body, so *"every agent said buy"* cannot be written directly. It is
written as what it amounts to when every agent votes exactly once:

```
def verdict_buy(purchase: Case) :- voted(purchase) ∧ ¬ walked(purchase) ∧ ¬ haggled(purchase)
```

Somebody voted, nobody walked, nobody haggled. The two negations are **stratified** — `walked` and
`haggled` depend on nothing that depends on them — the same shape as `only_child` in the Kinship theory.

And **silence is not agreement**: without `voted(purchase)`, an empty panel would buy everything.

Every verdict test asserts that **exactly one** verdict fires. Dropping `¬ walked` from the negotiate rule
lets a purchase be walked and negotiated at once — and that was checked to fail a test.

---

## The reserve, had it gone ahead

The buyer walks. But suppose they bought anyway: what would it take to carry it?

Treat the predicted shortfalls — repair exposure plus default loss — as **claims** against a fund, and the
buyer's yearly set-aside as its **premium**. Cramér and Lundberg worked out when such a fund survives:

```
safety loading   θ = c / (λ · m) − 1
adjustment       R = max(1/m − λ/c, 0)
Lundberg bound   ψ(u) ≤ exp(−R · u)
reserve          u = ln(1/ε) / R
```

### A simulation can only say "not yet"

Run the fund forward a thousand years and it survives. That proves nothing about year one thousand and
one. **The Lundberg bound answers the real question — the chance of *ever* running dry** — and the reserve
is sized against that. A test checks it both ways: the reserve for a 5% chance of ruin, fed back into the
bound, gives exactly 5% again.

### A thin loading makes the reserve explode

On the capstone's house — claims averaging $51 323, arriving about once every two years (a rate of 0.5),
against a set-aside of $30 000 a year:

```
  loading 16.9%
    reserve $1,063,225 keeps the chance of EVER running dry at or below 5%
```

**A million-dollar reserve for a $172 150 house.** That is not a bug. A loading of 16.9% makes the
adjustment coefficient tiny, and the reserve is inversely proportional to it. The honest reading: this
purchase is barely fundable, and ruin theory says so in a number too large to ignore.

### And below the loading, no reserve at all

At a $20 000 set-aside, expected claims beat the premium:

```
  loading -22.1%
    NO RESERVE IS REPORTED. The set-aside does not beat the expected claims, so ruin is certain
    however much is held back — and any figure printed here would be a comfortable lie.
```

When the loading is at or below zero, ruin is **certain** and `R = 0`. The reserve formula then divides by
zero and returns **positive infinity** — a number, which would print as "a very large reserve" when the
truth is "no reserve is enough". A test pins that infinity, so the refusal has something concrete to
refuse. **The safety loading is a precondition, not a diagnostic.**
