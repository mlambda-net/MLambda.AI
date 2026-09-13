# The actuarial capstone

**Read this after L3.** It is not a fourth level. It is what the three levels were for — everything the
other projects taught, spent on one decision, and it introduces no new Hilbert.

**Source:** [`src/MLambda.AI.Actuarial/`](../../src/MLambda.AI.Actuarial/)

## The decision

A buyer is looking at a real house from the Ames sales — quality 6, 1 478 square feet, 52 years old,
rated 3 for condition, which sold for $156 500. It is on offer at 10% more: $172 150. Should they buy?

Three agents look at it, each with its own question and its own evidence:

```
  ValuationAgent   fair value $169,249, spread $39,602
                   asking is +0.07 spreads over                     intends Buy
  ConditionAgent   rated 3, 2 points below typical × $8,762 a point
                   repair exposure $17,523                            intends Negotiate
  CreditAgent      loan-to-value 94.6%, rate 6.00%, default chance 30.40%
                   loss if it defaults $33,800                          intends Walk
                   ⚠ THIS LOAN IS SYNTHESISED. ...

  Adjudicator      Walk — one agent seeing ruin outranks two seeing a bargain.
```

**They disagree, and that is the design.** Valuation is right that the price is fair. Condition is right
that the house needs work. Credit — on an invented loan — is right that the borrowing is stretched. An
architecture that averaged those into one score would have thrown away exactly the information that
makes the decision hard.

This house was not staged. It was found by searching all 2 930 sales for ones the three agents disagree
about; there are 188.

## Where every piece came from

| Piece | Dialect | What it spends |
|---|---|---|
| [`House.hb`](../../src/MLambda.AI.Actuarial/House.hb) | `.hb` frame | reading data as a schema — from [ML](../L1-novice/ml.md) |
| [`Valuation.hb`](../../src/MLambda.AI.Actuarial/Valuation.hb) | `.hb` | least squares and a spread — from [ML L1](../L1-novice/ml.md) |
| [`Condition.hb`](../../src/MLambda.AI.Actuarial/Condition.hb) | `.hb` | the same fit, and a confound found by measuring first |
| [`Credit.hb`](../../src/MLambda.AI.Actuarial/Credit.hb) | `.hb` | proportional hazards from `Prelude.Sequences` |
| [`Reserve.hb`](../../src/MLambda.AI.Actuarial/Reserve.hb) | `.hb` | Cramér–Lundberg from `Prelude.Fields` |
| three `*Agent.ha` | `.ha` | BDI agents and commitment — from [Agents](../L2-practitioner/agent.md) |
| [`Risk.hs`](../../src/MLambda.AI.Actuarial/Risk.hs) · [`.hp`](../../src/MLambda.AI.Actuarial/Risk.hp) | `.hs` `.hp` | the attitudes, and a derivation of absurdity — from [Logic L3](../L3-advanced/logic.md) |
| [`Adjudicator.hs`](../../src/MLambda.AI.Actuarial/Adjudicator.hs) · [`.hp`](../../src/MLambda.AI.Actuarial/Adjudicator.hp) | `.hs` `.hp` | stratified negation — from [Logic L1](../L1-novice/logic.md) |

**All four dialects in one project** — which no other project here does.

## The pages

- [valuation.md](valuation.md) — fair value, and why a premium is measured in spreads
- [condition.md](condition.md) — why this model does *not* use age, found by measuring the data
- [credit.md](credit.md) — the honesty page: a method on inputs that do not exist
- [adjudicator.md](adjudicator.md) — a veto not a vote, and a reserve that may refuse to exist

## Run it

```bash
dotnet run --project src/MLambda.AI.Actuarial
```

## What this is not

**A worked system for teaching, not an underwriting product.** And the reserve it computes is real
ruin theory over one real sale and one invented loan — a correct method on partly fictional inputs.
Both facts are printed with the output, not only written here.
