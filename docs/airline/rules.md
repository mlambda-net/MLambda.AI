# The rules: policies as obligations and prohibitions

A policy written for people reads like this: *"A refundable fare that has not been flown is refunded to
the original form of payment."* A language model can repeat that sentence and still promise something it
does not say. This page turns each sentence into a **logical statement**: a law the engine runs and the
kernel can check. After that, the only thing that can commit the airline is a norm derived from those
laws.

**Source:** [`Policy.hs`](../../src/MLambda.AI.Airline/Policy.hs) ·
[`Policy.hp`](../../src/MLambda.AI.Airline/Policy.hp) ·
[`data/policies.json`](../../src/MLambda.AI.Airline/data/policies.json)

## Reading a norm

Deontic logic has three words, which you met in [Duty](../L1-novice/minds.md#duty):

| Symbol | Reads | In `Policy.hs` |
|---|---|---|
| `O(i) a` | party *i* is **obliged** to do *a* | `obliged(m, i, a, source)` |
| `F(i) a` | party *i* is **forbidden** to do *a* | `forbidden(m, i, a, source)` |
| `P(i) a` | party *i* is **permitted** to do *a*: nothing forbids it | `permitted(m, i, a)` |

Every norm carries four things, and each one closes a door a tricky question could use:

- **`m`, a moment.** A norm holds *at a time*. Why that matters is the whole of [time.md](time.md).
- **`i`, a party.** The airline and the assistant are different parties with different norms.
- **`a`, an act.** A closed vocabulary: `refund`, `bereavement_fare`, `travel_credit`.
- **`source`, a policy id.** No norm exists without the policy that creates it, so every reply can say
  which policy it rests on.

## The policies, one law each

The facts a law reads come from two places. The **booking record**
([`bookings.json`](../../src/MLambda.AI.Airline/data/bookings.json)) says whether the trip was flown, the
kind of fare, and whether the airline cancelled. The **customer's message** says what they ask for and
whether a death in the family is the reason. The model never supplies a booking fact.

### Bereavement fares

> **BRV-1** A reduced bereavement fare is available for travel because of the death of an immediate
> family member. It must be requested before the trip.
>
> **BRV-2** A bereavement fare cannot be applied to travel that has already been completed.

```hilbert
law bereavementBefore = ∀ m, request(m, "bereavement_fare") ∧ bereavement(m) ∧ ¬ flown(m) ⇒ obliged(m, "airline", "bereavement_fare", "BRV-1")
law notABereavement   = ∀ m, request(m, "bereavement_fare") ∧ ¬ bereavement(m) ∧ ¬ flown(m) ⇒ forbidden(m, "airline", "bereavement_fare", "BRV-1")
law bereavementAfter  = ∀ m, request(m, "bereavement_fare") ∧ flown(m) ⇒ forbidden(m, "airline", "bereavement_fare", "BRV-2")
```

Read the first law aloud: *at any moment m, if a bereavement fare is requested, there is a bereavement,
and the trip is not flown, then the airline is obliged to give the bereavement fare, because of BRV-1.*
BRV-2 is the norm the Moffatt chatbot contradicted.

Note what a single policy sentence became. "Must be requested before the trip" turned into **two** laws:
an obligation when the conditions hold, and a prohibition when there is no bereavement. A policy written
for people leaves the "otherwise" implicit. A law has to say it, or the otherwise is simply unknown.

### Refunds

```hilbert
law refundable       = ∀ m, request(m, "refund") ∧ fare(m, "refundable") ∧ ¬ flown(m) ⇒ obliged(m, "airline", "refund", "REF-1")
law nonRefundable    = ∀ m, request(m, "refund") ∧ fare(m, "nonrefundable") ∧ ¬ flown(m) ∧ ¬ cancelledByAirline(m) ⇒ forbidden(m, "airline", "refund", "REF-2")
law keptAsCredit     = ∀ m, request(m, "refund") ∧ fare(m, "nonrefundable") ∧ ¬ flown(m) ∧ ¬ cancelledByAirline(m) ⇒ obliged(m, "airline", "travel_credit", "REF-2")
law airlineCancelled = ∀ m, request(m, "refund") ∧ cancelledByAirline(m) ⇒ obliged(m, "airline", "refund", "REF-3")
law alreadyFlown     = ∀ m, request(m, "refund") ∧ flown(m) ∧ ¬ cancelledByAirline(m) ⇒ forbidden(m, "airline", "refund", "REF-4")
```

| Booking | Asked | Norms derived |
|---|---|---|
| `RFD512` refundable, not flown | refund | `O(airline) refund` REF-1 |
| `FUT315` non-refundable, not flown | refund | `F(airline) refund` REF-2 and `O(airline) travel_credit` REF-2 |
| `CXL777` cancelled by the airline | refund | `O(airline) refund` REF-3 |
| `MOF240` flown | refund | `F(airline) refund` REF-4 |
| `MOF240` flown | bereavement fare | `F(airline) bereavement_fare` BRV-2 |

REF-2 shows why a norm is better than a yes/no answer. The same policy **forbids** one act and
**obliges** another, and the reply says both: no refund, but you keep a travel credit.

## CHAT-1: the rule the Moffatt chatbot lacked

The policies above say what the airline owes. None of them says anything about what the chatbot may
*say*. That gap is the Moffatt case. So the assistant is a party too, and its acts are **promises**:

```hilbert
law promiseOnlyWhatIsOwed = ∀ m a, moment(m) ∧ act(a) ∧ ¬ ought(m, "airline", a) ⇒ forbidden(m, "assistant", a, "CHAT-1")

law owes     = ∀ m i a _source, obliged(m, i, a, _source) ⇒ ought(m, i, a)
law mustNot  = ∀ m i a _source, forbidden(m, i, a, _source) ⇒ oughtNot(m, i, a)
law violates = ∀ m i a, oughtNot(m, i, a) ∧ did(m, i, a) ⇒ violation(m, i, a)
```

*The assistant is forbidden to promise any act the airline is not obliged to perform.* Promising is
therefore **forbidden by default** and permitted only where an obligation exists. That default is the
most important design choice in the file. The general rule "what is not forbidden is permitted" is fine
for what a customer may do. It is dangerous for what a company commits to. For commitments the default is
reversed.

Each model call returns a `promise` field saying what its text commits the airline to. The host asserts
that as an act, `did(m, "assistant", promise)`, and asks the engine for violations:

```
review  The model's draft promised a bereavement fare, which the airline is not obliged to give for
        booking MOF240 now; promising it violates CHAT-1. The draft was withheld.
```

A word the model invents, such as `"full_refund_and_voucher"`, is not tidied into "nothing". It is
asserted like any other promise. No policy obliges it, so it is a violation.

## The agent: choosing the kind of reply

Before any norm is asked for, [`Assistant.ha`](../../src/MLambda.AI.Airline/Assistant.ha) decides what
kind of reply the message gets:

```hilbert
intend Answer  for Answered when B(self) topic("in") ∧ B(self) booking("yes")
intend Clarify for Answered when B(self) topic("in") ∧ B(self) booking("no")

impossible Answered when B(self) topic("out") because "That is not about a refund or a bereavement fare, and no written policy here covers it — please contact an agent"
```

A question the book does not cover (baggage, seats, anything else) is not answered from the model's
general knowledge. It is `impossible`, and the reason in the source is what the customer reads. **An honest
refusal is a correct reply.** Answering from general knowledge is how the Moffatt chatbot got into trouble.

The order of a reply is a plan, [`Reply.hk`](../../src/MLambda.AI.Airline/Reply.hk). It has no path to
`print` that goes around `review`:

```hilbert
plan AnswerSteps(act: chan Word, done: chan Word) : Assistant ≔ "review" → act ; v ← done ; match v { … } ; "format" → act ; f ← done ; match f { when "formatted" ↦ SKIP  other ↦ "fallback" → act ; g ← done } ; "print" → act ; p ← done ; believe replied(1)
```

## The frame, and what the kernel proves

`O` is a real modality, not a label. `ideal(i, m, u)` means *u is a situation in which party i does
everything it ought to at m*:

```hilbert
modality O(i) over ideal at holds

law serialO = ∀ i w u, idealSeed(i, w, u) ⇒ ideal(i, w, u)                 -- axiom D
law atIdeal = ∀ i a w u, ought(w, i, a) ∧ ideal(i, w, u) ⇒ holds(u, a)
```

Axiom **D** says every situation has an ideal. So nothing can be obliged and forbidden at once, since no
ideal situation could satisfy both. The engine checks that too: `conflict(m, i, a)` must come back empty
for every booking, and `PolicyTests` asks.

[`Policy.hp`](../../src/MLambda.AI.Airline/Policy.hp) proves, and the build refuses to compile if any of
these stop checking:

| Theorem | Says |
|---|---|
| `every_seeded_situation_has_an_ideal` | axiom D |
| `what_a_policy_obliges_holds_wherever_things_are_ideal` | the meaning of `O` |
| `doing_what_a_policy_forbids_is_a_violation` | a forbidden act done is a violation, whoever says otherwise |
| `two_steps_are_later`, `flying_leaves_the_trip_flown_two_moments_on` | time: see [time.md](time.md) |
| `a_bereavement_fare_asked_for_after_flying_is_forbidden` | the Moffatt case, as a theorem |
| `so_granting_it_anyway_is_a_violation` | and what granting it would be |
| `a_cancellation_by_the_airline_obliges_a_refund` | REF-3 |

The laws with `¬` run in the engine and are tested rather than proved. That is how negation is handled
throughout this repository, as on the [Logic pages](../L1-novice/logic.md).

## What every reply shows

```
policy  REF-4   F(airline) refund     after you fly   Completed travel is not refunded
policy  REF-1   O(airline) refund     until you fly   Refundable fares are refunded
```

The customer sees the norm, the moment it holds, and the policy it comes from. A reply that cannot show a
norm cannot commit the airline. That is the guarantee the Moffatt ruling says a company needs.
