# Minds, L1 — Traffic and Duty

Everything so far has asked *whether* something is true. Fluffy is an animal; dave is alice's
descendant. A **modal logic** asks *how* something is true. Is it true **always** or only
**eventually**? Is it true, or only **supposed** to be? Does somebody **know** it, or merely
**believe** it?

This page takes the first two questions: time, then obligation. Both turn out to be ordinary rules
over one extra relation, and both come with a surprise that is worth more than the notation.

> **Why any of this matters** (why a machine that confuses *ought* with *is* can cost a company in
> court, and why possible worlds are the right way to put a thought into software) is the subject of
> [Putting a thought into a machine](../ideas/thinking-machines.md) and
> [When a chatbot makes a promise](../ideas/semantics-and-accountability.md).

| Sample | The new idea |
|---|---|
| [Traffic](#traffic) | *always*, *eventually* and *until* are questions you can ask of a run |
| [Duty](#duty) | an obligation says what ought to be, and does not make it so |

---

## Traffic

**Source:** [`src/MLambda.AI.Minds/Traffic.hs`](../../src/MLambda.AI.Minds/Traffic.hs) ·
[`src/MLambda.AI.Minds/Traffic.hp`](../../src/MLambda.AI.Minds/Traffic.hp)

### The idea

Watch a traffic light for a minute and write down what it shows at each moment:

| moment | m0 | m1 | m2 | m3 | m4 | m5 |
|---|---|---|---|---|---|---|
| colour | red | red | green | green | amber | red |

That list is a **trace**, one run of a system. Now ask questions about it that are not about any
single moment:

- From m0, will it **eventually** be green? Yes: at m2.
- From m0, is it **always** red? No: m2 is green.
- From m5 on, is it **always** red? Yes. Nothing after m5 is anything else.
- From m0, is it red **until** it is green? Yes. It stays red, and then turns green.

Temporal logic names these three questions **F** (*finally*, or eventually), **G** (*globally*, or
always) and **U** (*until*). Engineers use them to say what a system must never do and what it must
eventually do: a lift eventually arrives, and a light is never green both ways. Here they are rules.

### The theory

```
theory Traffic (Moment, Colour)
{
  def moment(id: Moment)
  def colour(id: Colour)
  def next(now: Moment, then: Moment)
  def shows(at: Moment, c: Colour)

  law nowIsLater  = ∀ m,     moment(m) ⇒ later(m, m)
  law stepIsLater = ∀ m n,   next(m, n) ⇒ later(m, n)
  law onwards     = ∀ m n o, later(m, n) ∧ later(n, o) ⇒ later(m, o)

  law eventuallyIs = ∀ m n c, later(m, n) ∧ shows(n, c) ⇒ eventually(m, c)

  law breaksAt = ∀ m n c, later(m, n) ∧ colour(c) ∧ ¬ shows(n, c) ⇒ breaks(m, c)
  law alwaysIs = ∀ m c,   moment(m) ∧ colour(c) ∧ ¬ breaks(m, c) ⇒ always(m, c)

  law untilNow  = ∀ m a b,   shows(m, b) ∧ colour(a) ⇒ until(m, a, b)
  law untilStep = ∀ m n a b, shows(m, a) ∧ next(m, n) ∧ until(n, a, b) ⇒ until(m, a, b)

  law clashes = ∀ m a b, shows(m, a) ∧ shows(m, b) ∧ a ≠ b ⇒ clash(m)
  ...
}
```

Read it in three parts.

**Time.** `later(m, n)` means *n is m or comes after it*. A step is later, and later composes. It
includes *now* on purpose, because "always, from here" should include here.

**F is one witness.** `eventually(m, c)` holds if *some* later moment shows `c`. One is enough.

**G is the absence of a counter-example.** "Always green from m" means *there is no later moment
that is not green*. That is two steps. `breaks(m, c)` finds a later moment that fails to show `c`,
and `always(m, c)` holds where **no** `breaks` was found. The `¬` is *negation as failure*: true when
the search for `breaks` comes back empty.

> That second `¬` is legal only because `breaks` never depends on `always`. The engine can settle
> every `breaks` first and then ask. A rule that negated itself would have no answer at all, and
> Hilbert refuses it with `HS0031`. See [diagnostics](../hilbert/06-diagnostics.md).

**U unfolds one moment at a time.** `a until b` holds *now* if `b` shows now. Otherwise it holds if
`a` shows now **and** `a until b` holds next. Two rules, and the engine follows them to the end of
the trace.

### What it answers

```
A light's run, m0 to m5: red, red, green, green, amber, red

  F  from m0, eventually shows:  amber, green, red
  F  from m4, eventually shows:  amber, red
  G  from m0, always shows:      (nothing)
  G  from m5, always shows:      red
  U  from m0, red until green:   True
  U  from m2, green until red:   False
```

Two of those are worth a second look.

- **From m4, green is gone.** F looks forward only. Green happened, but not *from here*.
- **Green until red is False from m2**, although red does come. Amber comes between, so green does
  not *last* until red, and until asks for both halves.

### The proof

The engine answered about one run. The proofs are about **every** run anybody could assert:

```
theorem two_steps_are_later : ∀ m n o, next(m, n) ⇒ next(n, o) ⇒ later(m, o)
proof
  intro m n o first second
  have mn : later(m, n) by apply stepIsLater [first]
  have no : later(n, o) by apply stepIsLater [second]
  apply onwards [mn, no]
qed
```

Two steps forward is later. It sounds too obvious to prove, and that is the point: the proof uses
nothing but the three laws of time, so it holds of every trace, including ones with a thousand
moments. The file also proves that a step is later, that later composes, and both halves of how
until unfolds.

### Run it

```bash
dotnet run --project src/MLambda.AI.Minds -- Traffic
```

### What the tests assert

[`TrafficTests.cs`](../../test/MLambda.AI.Minds.Tests/TrafficTests.cs)

- Eventually sees every colour still to come, and not one that has already gone.
- Always holds only where nothing later breaks it: red from m5, and nothing from m0.
- Red waits until green; green does not wait until red when amber comes between.
- The light never shows two colours at once, and a broken light that does is caught.
- Every theorem in `Traffic.hp` is proved by the kernel while the test runs.

### Try it yourself

1. Change m5 from red to green in `Program.cs`. Predict what G from m5 answers, then run it.
2. Add a seventh moment, `m6`, showing amber. Does *red* stay always-on from m5?
3. Delete `nowIsLater` from `Traffic.hs`. Predict which queries change. (Hint: what does G ask about
   the current moment?)

---

## Duty

**Source:** [`src/MLambda.AI.Minds/Duty.hs`](../../src/MLambda.AI.Minds/Duty.hs) ·
[`src/MLambda.AI.Minds/Duty.hp`](../../src/MLambda.AI.Minds/Duty.hp)

### The idea

A small office has a rulebook:

| Role | Must | May not |
|---|---|---|
| keyholder | lock up | — |
| staff | — | smoke |

ana is the keyholder *and* staff. ben is staff. Three words do all the work:

- **Obligatory:** you must. ana must lock up.
- **Forbidden:** you must not. Neither may smoke.
- **Permitted:** anything not forbidden. ben may lock up, and ben may sweep.

That branch of logic is **deontic** logic, from the Greek for *what is binding*. And it holds the
most important sentence on this page:

> **An obligation says what ought to be. It does not make it so.**

That sounds obvious, until you notice how often software gets it wrong: a system that treats "the
invoice must be paid" as "the invoice is paid" has confused the rulebook with the world.

### The theory

```
law obligedBy   = ∀ p r a, plays(p, r) ∧ obliges(r, a) ⇒ obliged(p, a)
law forbiddenBy = ∀ p r a, plays(p, r) ∧ forbids(r, a) ⇒ forbidden(p, a)
law permittedIf = ∀ p a,   person(p) ∧ act(a) ∧ ¬ forbidden(p, a) ⇒ permitted(p, a)
law violates    = ∀ p a,   obliged(p, a) ∧ ¬ did(p, a) ⇒ violation(p, a)
law clash       = ∀ p a,   obliged(p, a) ∧ forbidden(p, a) ⇒ conflict(p, a)
```

A role passes its duties to whoever plays it. What is not forbidden is permitted. A **violation** is
an obligation with no deed to meet it. A **conflict** is a rulebook that obliges and forbids the same
act, which is a bug in the rules, not in the people.

### What it answers

```
An office: ana is the keyholder and staff, ben is staff.
Keyholders must lock up. Staff may not smoke.

  ana must:      lock_up
  ben may:       lock_up, sweep
  unmet:         ana has not done lock_up
  An obligation says what ought to be. It does not make it so.
```

Nobody told the engine that ana locked up, so the obligation stands **unmet**. That is the engine
keeping *ought* and *is* apart.

### The frame behind the rulebook

The rulebook runs. Behind it sits a small piece of logic that proves: **ideal situations**.
`ideal(w, u)` says *u is a situation where everything that ought to be, is*.

Two laws decide what "ought" can mean:

- **D: every situation has an ideal.** That is what makes a rulebook *consistent*: if nothing ideal
  exists, "ought" can demand anything, including p and not p at once. Duty seeds the ideal, as
  seriality is seeded everywhere in this repository, because a rule cannot conclude that something
  exists.
- **And not T.** `ideal(w, w)`, *this situation is ideal because it is this one*, is **not** a law.
  If it were, everything that ought to be would already be true.

`Duty.hp` proves D and the inheritance of duty through roles. The absence of T is tested in the one
way a missing theorem can be: the test asks the kernel to prove `ideal(w, w)` and watches it refuse.

```
But_what_ought_to_be_is_not_thereby_what_is      Rejected
```

### Run it

```bash
dotnet run --project src/MLambda.AI.Minds -- Duty
```

### What the tests assert

[`DutyTests.cs`](../../test/MLambda.AI.Minds.Tests/DutyTests.cs) ·
[`RefusalTests.cs`](../../test/MLambda.AI.Minds.Tests/RefusalTests.cs)

- A role passes its obligations to whoever plays it, and only to them.
- What is not forbidden is permitted.
- An obligation does not make itself true, and doing it discharges it.
- A rulebook that obliges and forbids the same act is caught, for everyone it binds.
- The kernel proves D, and refuses "what ought to be is".
- **One limit, pinned:** a deed reported in a *later* batch of facts does not yet withdraw a
  violation the engine already drew. That is a limit of today's engine, not of the logic, and it is
  written up in [diagnostics](../hilbert/06-diagnostics.md#a-conclusion-drawn-from-an-absence-is-not-revised-when-the-absence-ends-open).

### Try it yourself

1. Add `new Rules.DidFact("ana", "lock_up")` to the office in `Program.cs`, in the same `AssertAll`.
   Predict *unmet*.
2. Add `new Rules.ObligesFact("staff", "smoke")`. Now the rulebook contradicts itself. Add a line to
   print `office.Conflicts()` and see who it catches.
3. Give ben the keyholder role too. What does `ben must` answer, and does it change `ben may`?

Next: [Minds, L2 — Knowledge and Mind](../L2-practitioner/minds.md).
