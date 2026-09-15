# Use case: adding temporal logic

This page is a worked use case. It starts from a real wrong answer, finds the missing idea, and adds it
one law at a time. Follow the same steps when your own rules turn out to depend on *when*.

**Source:** [`Policy.hs`](../../src/MLambda.AI.Airline/Policy.hs) ·
[`Expert/PolicyExpert.cs`](../../src/MLambda.AI.Airline/Expert/PolicyExpert.cs) ·
[`PolicyTests.cs`](../../test/MLambda.AI.Airline.Tests/PolicyTests.cs)

## 1. The wrong answer

The deontic rules of [rules.md](rules.md) were in place, with obligations, prohibitions and CHAT-1, and
every test passed. Then a customer asked:

```
You: so I can fly on RFD512 and ask for a refund?
Assistant: … Because your fare is refundable and the flight has not been flown, we will refund it to
  your original form of payment. You don't need to do anything further on your end.
  policy  REF-1   O(airline) refund   Refundable fares are refunded
  review  The model's draft promised a refund, which the airline is obliged to give.
```

Every individual step was "correct":

- the booking record says RFD512 is refundable and **not flown**,
- so REF-1 derives `O(airline) refund`,
- so a promised refund is backed, and CHAT-1 allows it.

And the answer is wrong. It is the same shape of wrong as Moffatt: *fly now, claim later.*

## 2. The missing idea: the rules were evaluated at the wrong moment

The customer did not ask "can I have a refund?" They asked "if I **fly first**, can I have a refund
**afterwards**?" The question describes a **plan**, a sequence of steps, and the request comes at the end
of it. The rules were evaluated at the only moment they knew about, *now*, when nothing has been flown.

Written as logic, the question is a claim about a later moment:

```
now:            ¬ flown,  refundable      ⇒  O(airline) refund     (REF-1)
customer flies
after flying:   flown,    refundable      ⇒  F(airline) refund     (REF-4)
customer asks for the refund     ← here
```

**Deontic logic says what ought to be. It does not say when.** A norm with no moment is silently a norm
about now. So the fix is not a new policy. It is time.

## 3. Step one: a trace of moments

Time here is the same as in [Traffic](../L1-novice/minds.md#traffic): a finite **trace**. There is one moment
now, and one moment after each step the customer says they will take before asking.

```hilbert
def moment(id: Moment)
def next(now: Moment, then: Moment)
def happens(at: Moment, what: Event)

law nowIsLater  = ∀ m,     moment(m) ⇒ later(m, m)
law stepIsLater = ∀ m n,   next(m, n) ⇒ later(m, n)
law onwards     = ∀ m n o, later(m, n) ∧ later(n, o) ⇒ later(m, o)
```

For "fly on RFD512, then ask", the host seeds two moments:

```
turn-1@0  ──happens fly──▶  turn-1@1
 (now)                      (the customer asks here)
```

The moments are **seeded by the host** because a rule cannot conclude that a moment exists (HS0022). The
steps come from the model. The draft prompt asks it for `"before": ["fly"]`, the steps the customer says
they will take before asking. Only steps the book has laws for are kept.

## 4. Step two: separate events from fluents

This is the idea people most often miss when adding time. **Flying is an event**: it happens between two
moments. **Flown is a fluent**: a state that is true or false *at* a moment. The policies read fluents
(`¬ flown(m)`), and customers talk about events ("I will fly").

```hilbert
law flownOnRecord   = ∀ m,   travelled(m) ⇒ flown(m)
law flyingIsFlown   = ∀ m n, happens(m, "fly") ∧ next(m, n) ⇒ flown(n)
law flownStaysFlown = ∀ m n, flown(m) ∧ next(m, n) ⇒ flown(n)
```

Three laws, three jobs:

1. **The record** can say a trip was already flown, as for MOF240.
2. **An event starts a fluent** at the *next* moment, not the same one.
3. **Persistence**: once flown, always flown. Without this law, flown would be true for exactly one
   moment and a customer could "wait a step" and ask again.

## 5. Step three: carry the unchanging facts forward

The booking record is asserted at the first moment. A fact asserted at `@0` is not automatically true at
`@1`, and a law reading `fare(m, "refundable")` at `@1` would find nothing. So each fact that does not
change with time gets an **inertia** law (in the AI literature, the *frame axiom*):

```hilbert
law fareStays        = ∀ m n k, fare(m, k) ∧ next(m, n) ⇒ fare(n, k)
law cancelledStays   = ∀ m n,   cancelledByAirline(m) ∧ next(m, n) ⇒ cancelledByAirline(n)
law bereavementStays = ∀ m n,   bereavement(m) ∧ next(m, n) ⇒ bereavement(n)
```

Without these laws, the moment after flying would have no fare at all. Neither REF-1 nor REF-4 would fire,
and the request would look "uncovered". That is safe, but wrong.

## 6. Step four: make every norm take a moment

The policies do not change their meaning. They only read the fluent `flown` at a moment `m` instead of the
record's `travelled`:

```hilbert
law refundable   = ∀ m, request(m, "refund") ∧ fare(m, "refundable") ∧ ¬ flown(m) ⇒ obliged(m, "airline", "refund", "REF-1")
law alreadyFlown = ∀ m, request(m, "refund") ∧ flown(m) ∧ ¬ cancelledByAirline(m) ⇒ forbidden(m, "airline", "refund", "REF-4")
```

The host asks the request at **every** moment of the trace, so the engine says what the answer would be at
each one:

| Moment | `flown` | Norm |
|---|---|---|
| `@0` now | no | `O(airline) refund` REF-1 |
| `@1` after flying | yes | `F(airline) refund` REF-4 |

## 7. Step five: say what time does to a duty

Two temporal verdicts turn that table into an answer a person can act on. They combine `O`/`F` with the
temporal operators *until* and *always* from Traffic:

```hilbert
-- LOST: owed now, and the customer's own step makes it forbidden next.   O a U e
law lostByAStep    = ∀ m n a e, ought(m, "airline", a) ∧ happens(m, e) ∧ next(m, n) ∧ oughtNot(n, "airline", a) ⇒ lostBy(m, a, e)

-- F O a: some moment from here on owes it.
law owedSometime   = ∀ m n a, later(m, n) ∧ ought(n, "airline", a) ⇒ eventuallyOwed(m, a)

-- G ¬O a: no moment from here on owes what was asked.
law owedAtNoMoment = ∀ m a, request(m, a) ∧ ¬ eventuallyOwed(m, a) ⇒ neverOwed(m, a)
```

`neverOwed` is *always not owed*, written as Traffic writes *always*: as the absence of a counter-example.
Negation-as-failure is legal here because `eventuallyOwed` never depends back on `neverOwed`.

## 8. Step six: judge promises at the moment the customer would ask

This is the step that actually protects the company. CHAT-1 already forbids promising what is not owed.
The host now asserts the model's promise **at the asked moment**, the last moment of the trace:

```csharp
engine.Assert(new Rules.DidFact(decision.Asked, Assistant, promise));   // decision.Asked = "turn-1@1"
```

A refund is owed at `@0`. That does not license "fly, and we will refund you", because the promise is
about `@1`, where the refund is forbidden.

## 9. The answer now

```
You: so I can fly on RFD512 and ask for a refund?
Assistant: I understand you'd like to fly first and then ask for a refund, but that isn't possible: once
  a trip has been flown, no refund is owed. … you would need to request it before you travel.
  policy  REF-4   F(airline) refund   after you fly   Completed travel is not refunded
  policy  REF-1   O(airline) refund   until you fly   Refundable fares are refunded
  time    O(airline) refund holds until you fly (REF-1): O refund U fly.
  time    after you fly, a refund is never owed again: G ¬O refund.
```

And the question with no plan is unchanged: "I want a refund for RFD512" still gets `O(airline) refund`
now.

## 10. Pin it with tests and a proof

Each step above has a test in
[`PolicyTests.cs`](../../test/MLambda.AI.Airline.Tests/PolicyTests.cs), and the ones that matter most are
the pairs. A temporal fix that breaks the plain question is not a fix.

```csharp
[Fact]
public async Task A_refund_owed_today_does_not_license_promising_one_after_flying()
{
    var today = await Decide("RFD512", "refund");
    var afterFlying = await Decide("RFD512", "refund", false, "fly");

    Assert.Null(await Expert.ViolatedByAsync(today, "refund"));

    var broken = await Expert.ViolatedByAsync(afterFlying, "refund");
    Assert.Equal("F(assistant) refund", broken!.Written);
    Assert.Equal("after you fly", broken.When);
}
```

The Moffatt case itself is proved over time, not just tested:

```hilbert
theorem a_bereavement_fare_asked_for_after_flying_is_forbidden : ∀ m n, happens(m, "fly") ⇒ next(m, n) ⇒ request(n, "bereavement_fare") ⇒ forbidden(n, "airline", "bereavement_fare", "BRV-2")
proof
  intro m n flying step asked
  have landed : flown(n) by apply flyingIsFlown [flying, step]
  apply bereavementAfter [asked, landed]
qed
```

## The recipe

When a rule depends on *when*:

1. **Find the wrong answer's hidden moment.** Which moment did the rules assume? Usually "now".
2. **Seed a trace**: one moment now, one after each step the question describes.
3. **Split events from fluents.** Customers say events; policies read fluents.
4. **Write persistence** for fluents that do not undo (flown), and **inertia** for facts that do not
   change (fare).
5. **Give every norm a moment**, and evaluate the request at every moment.
6. **Derive the temporal verdicts** people need: *until* (what a step ends) and *never* (what no step
   restores).
7. **Judge commitments at the moment they would be used**, not at the moment they are made.
8. **Test in pairs**: the new temporal question, and the old plain question unchanged.

[hacking.md](hacking.md) shows what happened when someone tried to get around exactly this.
