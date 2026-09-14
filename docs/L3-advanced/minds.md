# Minds, L3 — Deadlines, and what these logics cannot say

At L1 time and duty were separate samples; at L2 knowledge and belief were. Real questions mix them.
*Did she breach the contract?* is deontic, because there was a duty. It is temporal, because the
deadline has passed. And it is epistemic, because it matters whether she knew. This page puts all
three in one rule, then spends its second half on the part a practitioner needs most: **where these
logics stop**, and how you can tell you've reached the edge.

| Section | The new idea |
|---|---|
| [Deadlines](#deadlines) | one verdict can depend on duty, time and knowledge at once |
| [Introspection](#introspection-and-the-theorem-nobody-gave-you) | what S5 gives you for free |
| [What cannot be said](#what-these-logics-cannot-say-and-how-you-find-out) | six edges, each with its tell-tale |

---

## Deadlines

**Source:** [`src/MLambda.AI.Minds/Deadlines.hs`](../../src/MLambda.AI.Minds/Deadlines.hs) ·
[`src/MLambda.AI.Minds/Deadlines.hp`](../../src/MLambda.AI.Minds/Deadlines.hp)

### The scene

Today is **Thursday**. Three people each owed a report.

| person | due | told about it | filed |
|---|---|---|---|
| ana | Tuesday | — | Monday |
| ben | Tuesday | Monday | never |
| cy | Wednesday | never | Thursday |

ben and cy both missed their deadline. Should they get the same verdict?

### The theory

```
law remembers = ∀ p a m n,  informed(p, a, m) ∧ later(m, n) ⇒ knows_due(p, a, n)
law fulfils   = ∀ p a m by, due(p, a, by) ∧ did(m, p, a) ∧ later(m, by) ⇒ fulfilled(p, a, by)
law passes    = ∀ by n,     today(n) ∧ later(by, n) ∧ by ≠ n ⇒ passed(by)
law breaches  = ∀ p a by,   due(p, a, by) ∧ passed(by) ∧ ¬ fulfilled(p, a, by) ⇒ breached(p, a, by)
law knowingly = ∀ p a by,   breached(p, a, by) ∧ knows_due(p, a, by) ⇒ knowing_breach(p, a)
law excuses   = ∀ p a by,   breached(p, a, by) ∧ ¬ knows_due(p, a, by) ⇒ excused(p, a)
```

Three logics, three kinds of line:

- **Time.** `later` is the order from [Traffic](../L1-novice/minds.md#traffic). A deadline has
  *passed* when today is strictly after it, and a duty is *fulfilled* by a deed on or before it.
- **Duty.** A *breach* is a duty that is due, past, and not fulfilled. That is the violation from
  [Duty](../L1-novice/minds.md#duty), now with a date.
- **Knowledge.** `remembers` carries what you were told forward in time: once told, you still know
  later. The breach then splits by what the person *knew on the deadline*.

`remembers` is an **assumption about these agents**, stated as a law: nobody forgets a duty. A
theory of forgetful agents would drop it, and a proof that needed it would stop checking. That is
the point of writing assumptions as laws.

### What it answers

```
Today is Thursday. ana's report was due Tuesday and she filed it Monday.
ben's was due Tuesday; he was told Monday and never filed.
cy's was due Wednesday; nobody told him, and he filed Thursday.

  kept             ana (report, due tue)
  knowing breach   ben (report) -- he had been told
  excused          cy (report) -- nobody told him

  ben and cy both missed their deadline. What separates the verdicts is what each knew.
```

cy did file, on Thursday. Filing late doesn't keep a duty, so cy breached it as surely as ben did.
What separates them is **what each knew on the deadline**, which is exactly the distinction a court,
a manager or a contract draws. The engine draws it from one extra rule.

### The proof

```
theorem what_you_were_told_you_still_know_later :
  ∀ p a m n o, informed(p, a, m) ⇒ later(m, n) ⇒ later(n, o) ⇒ knows_due(p, a, o)
proof
  intro p a m n o told mn no
  have mo : later(m, o) by apply onwards [mn, no]
  apply remembers [told, mo]
qed
```

Knowledge persists across any stretch of time, not only one day. The file also proves that being
told the day before means knowing on the day, and that doing it in time fulfils it.

---

## Introspection, and the theorem nobody gave you

From [Knowledge](../L2-practitioner/minds.md#knowledge), three theorems are worth reading at this
level.

| theorem | frame condition | in words |
|---|---|---|
| `knowing_you_know` | 4: transitive | if you know p, you know that you know p |
| `knowing_what_you_do_not_know` | 5: euclidean | if you don't know p, you know that you don't |
| `knowledge_is_symmetric` | B: symmetric | **derived** from T and 5; it isn't an axiom |

```
theorem knowledge_is_symmetric : ∀ i w u, agent(i) ⇒ world(w) ⇒ knows(i, w, u) ⇒ knows(i, u, w)
proof
  intro i w u a x seen
  have home : knows(i, w, w) by apply reflK [a, x]
  apply euclidK [seen, home]
qed
```

Nobody wrote a symmetry law. T says every world sees itself, 5 says two worlds you see can see each
other, and together they give symmetry. That is why S5's worlds fall into **clusters** of mutual
indistinguishability, and why "S5 knowledge" means an agent partitions the worlds into ones it can
tell apart and ones it can't.

Negative introspection is also the law people argue about. Do you really know everything you don't
know? For an idealised reasoner, yes. For a person, often not. S4 drops 5 for exactly that reason,
and in this repository dropping it means one word removed from an `axioms` line.

---

## What these logics cannot say, and how you find out

Every language has edges. The useful skill is recognising them, because the symptom is rarely a
sentence saying "you have reached an edge".

### 1. "Something exists": an existential in a rule head

Axiom D is `∀ w, ∃ u, ideal(w, u)`. A rule can't conclude that something **exists**: Hilbert
refuses it with `HS0022`. So every serial relation here (belief, desire, intention, ideal
situations) is **seeded**: the host names the witness, and D holds *exactly where something seeded
it*. A world nobody seeded has no ideal alternative, and nothing complains.

**Tell-tale:** `HS0022`, or a relation that is empty for one world and not the others.

### 2. □ over nothing is true of everything

"Believes p" means *p in every believed world*. If there are **no** believed worlds, every p is
believed, including p and not p. That's why D matters: it keeps an attitude from being vacuous.

**Tell-tale:** an agent that suddenly believes, desires or intends *everything*. Try removing the
robot's wish in [Mind](../L2-practitioner/minds.md#mind) and watch *desires* fill up.

### 3. A quantifier inside a premise

`K p ⇒ p` unfolds to `(∀ u, knows(i, w, u) ⇒ holds(u, p)) ⇒ holds(w, p)`, a universal *inside* the
thing being assumed. A `.hp` file can't parse that (`HP0003`). So factivity is stated on the frame,
`knows(i, w, w)`, which is equivalent on every frame by correspondence theory, and the engine carries
the formula.

**Tell-tale:** `HP0003: expected a predicate, a constructor or a variable`.

### 4. A proof file cannot state a non-theorem

"Belief is not factive" says a proof does **not** exist. `.hp` can only state what proves, so every
non-theorem here is a **test** that attempts the claim and requires `Rejected`, beside a control
that proves through the same harness.

**Tell-tale:** you want to write "cannot be proved", and there is nowhere in the proof file to
write it.

### 5. A negation fact is tested, not proved

"A fulfilled duty is never breached" rests on the `¬ fulfilled` in `breaches`. A Horn proof derives
what follows. It doesn't reason about what fails to follow. So that sentence is
`DeadlinesTests.A_kept_duty_is_never_breached`, not a theorem.

### 6. An engine that learns later does not yet unlearn

Negation as failure concludes from an **absence**: no deed, so a violation; nobody told him, so
excused. Today's Shin engine recomputes what new facts **add**, not what they **defeat**. A deed
reported in a later `AssertAll` doesn't withdraw the violation already drawn. Every scene in Minds
is therefore asserted in one batch, and
`DutyTests.But_today_a_deed_told_later_does_not_withdraw_the_violation_already_drawn` pins the gap
so its fix is noticed.

This one is a limit of the **engine**, not the logic, and it matters: an agent that perceives the
world over time must be able to revise a conclusion it drew from what it hadn't yet seen.
It is written up, with the workaround, in
[diagnostics](../hilbert/06-diagnostics.md#a-conclusion-drawn-from-an-absence-is-not-revised-when-the-absence-ends-open).

> **An edge that turned out to be a bug.** "Always" over a transitive time order negates into a
> recursive relation. Hilbert used to refuse that as `HS0031`, although it is ordinary stratified
> negation. It was fixed while writing this subject; see
> [diagnostics](../hilbert/06-diagnostics.md#hs0031-on-a-negation-that-is-not-in-a-cycle-fixed).
> Not every refusal is an edge of the logic.

---

## Run it

```bash
dotnet run --project src/MLambda.AI.Minds -- Deadlines
```

## What the tests assert

[`DeadlinesTests.cs`](../../test/MLambda.AI.Minds.Tests/DeadlinesTests.cs)

- Doing it before the deadline keeps it.
- A duty you were told of and missed is a knowing breach.
- Doing it late does not keep it, and not knowing excuses it.
- A kept duty is never breached, knowingly or not.
- Telling someone turns an excuse into a breach.
- Three theorems proved by the kernel.

## Try it yourself

1. Tell cy about the report on Tuesday, in the same `AssertAll`. Predict both verdicts, then compare
   with `Telling_someone_turns_an_excuse_into_a_breach`.
2. Delete the `remembers` law. Predict ben's verdict, then read which theorem stops checking.
3. Write `theorem belief_is_true : ∀ i w, agent(i) ⇒ world(w) ⇒ believes(i, w, w)` into
   `Knowledge.hp` with any proof you like. Read the build error: that is edge 4 meeting the
   one missing law.
4. Drop `euclidK` from `Knowledge.hp`'s axioms: you are now in S4. Which theorems go, and does
   `knowledge_is_symmetric` survive?
