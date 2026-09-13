# Condition — the model that refused to use age

**Source:** [`Condition.hb`](../../src/MLambda.AI.Actuarial/Condition.hb) ·
[`ConditionAgent.ha`](../../src/MLambda.AI.Actuarial/ConditionAgent.ha) ·
[`ConditionTests.cs`](../../test/MLambda.AI.Actuarial.Tests/ConditionTests.cs)

This is the page about **measuring before modelling**, because this model's first design was wrong and
only the data said so.

## The plan

Old houses need more repairs. So: let age and years since the last remodel drive a curve of component
failure, and expected repair cost follows. Reasonable, standard, and it would have been confidently
wrong.

## What the data said instead

Before building anything, the corpus's inspector rating — `Overall Cond`, 1 to 9 — was tabulated against
age:

| House age | Mean condition rating | Share rated exactly 5 |
|---|---|---|
| 0–9 years | 5.01 | **98%** |
| 10–29 | 5.37 | 73% |
| 30–59 | 5.74 | 39% |
| 60+ years | **6.15** | 17% |

**Older houses are rated in better condition.** The correlation with age is **+0.37**.

That is not old houses being sounder. Two things are happening at once:

- **New houses get a default.** An inspector rates a house with no history "average" — 98% of houses
  under ten years old are a 5.
- **Survivorship.** The old houses still standing and still being sold are the ones somebody kept up. The
  neglected ones were demolished decades ago and are not in any sales record.

A deterioration-from-age model fitted to this would conclude that old houses need **fewer** repairs.

## And the remodel year is floored

`Year Remod/Add` never goes below 1950, though `Year Built` goes back to 1872. Of the 632 houses built
before 1950, **339 record a remodel in exactly 1950** — the dataset's placeholder, not a building boom. So
"years since work" understates how long an old house has gone untouched, and its correlation with
condition is −0.05: effectively nothing.

## So the model uses the rating as the inspector gave it

```
fn pointsBelowTypical(cond)       ↦ max(5 − cond, 0)
fn repairExposure(cond, perPoint) ↦ max(5 − cond, 0) · perPoint
```

Five is the mode and the median — 1 654 of 2 930 houses — so it is what "typical" means here. A house
rated 3 is two points below.

**No failure data is invented either.** There are no repair costs or failure times in `houses.csv`, so a
survival curve would need a second made-up series — and only `Credit.hb` is allowed to invent its inputs.

## What a point is worth comes from the prices

Regress price on quality, living area, age **and** condition, and the condition coefficient is what the
market paid per point with the rest held fixed.

| Houses fitted | Value of one condition point |
|---|---|
| All 2 930 | **$5 030** |
| Under 30 years (1 267) | $8 614 |
| 30 years and over (1 663) | $8 762 |

The two age bands **agree with each other to within two percent** — and the pooled figure is below
**both**. That cannot be a property of condition. It is the age confound again, averaging two groups rated
on different scales. So the model fits within the house's own age band. For a risk estimate,
understating the cost would be optimism, not caution.

## One house moves the estimate by $630

The first version of the test expected $9 243 for the young band, and Hilbert said $8 614. **Hilbert was
right.** The script behind the expected number had split the band as `0 ≤ age < 30`, which silently
dropped one row:

> built **2008**, sold **2007** — an age of −1. Quality 10, 5 095 square feet, $183 850.

A partial sale of an unfinished new house, and the best-known outlier in the Ames data. With it included,
the exact answer is $8 614, to the cent. It stays in — hiding it would be the worse choice — and a test
pins it as a fact about the data.

**Two lessons in one row:** a single observation out of 1 267 moved an estimate by 7%, and an independent
check is only independent if it asks the same question.
