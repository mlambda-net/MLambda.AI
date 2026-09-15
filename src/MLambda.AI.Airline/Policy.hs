-- Policy.hs — the airline's policies as expert rules: what each party OUGHT to do, and WHEN.
--
-- THE CASE THIS SAMPLE EXISTS FOR. In Moffatt v. Air Canada (2024 BCCRT 149) a customer whose
-- grandmother had died asked the airline's chatbot about bereavement fares. The chatbot told him he could
-- fly at the full price and claim the bereavement discount back within 90 days. The written policy said
-- the opposite: a bereavement fare is not applied to travel already taken. The tribunal held the airline
-- to what its chatbot said. A language model had committed a company to a rule the company never wrote.
--
-- SO THE POLICY BOOK IS DEONTIC LOGIC, the logic of Duty.hs in MLambda.AI.Minds. Every policy is a NORM
-- with a moment, a party, an act and its source: `obliged(m, "airline", "refund", "REF-1")` is
-- O(airline) refund at moment m, because REF-1 says so; `forbidden(m, "airline", "refund", "REF-4")` is
-- F(airline) refund. What is not forbidden is permitted, and a forbidden act done anyway is a VIOLATION.
--
-- AND IT IS TEMPORAL LOGIC, the logic of Traffic.hs, because the Moffatt mistake is a mistake about TIME.
-- "Fly now, claim it back later" asks for a remedy at a moment after flying, and every norm that mattered
-- changed when the plane landed. So a customer's question is a finite trace: `now`, and one moment after
-- each step they say they will take before asking. Flying is an event; `flown` is a fluent that starts at
-- the next moment and never stops; the fare, a cancellation and a bereavement persist. The norms are read
-- at every moment of the trace, and three temporal verdicts are derived from them:
--
--   lostBy(m, a, e)   O a holds at m, the event e happens, and F a holds next — "a refund, until you fly".
--   eventuallyOwed    F O a — some moment from here on owes it.
--   neverOwed         G ¬O a — no moment from here on owes it: whatever the customer does, not this.
--
-- AND THE ASSISTANT IS A PARTY TOO. For the assistant, an act is a PROMISE. CHAT-1 is the norm the Moffatt
-- chatbot lacked — the assistant must not promise what the airline is not obliged to give AT THE MOMENT THE
-- CUSTOMER WOULD ASK — so "fly, and we will refund you" is a violation even though a refund is owed today.
--
-- A FRAME THAT PROVES. `ideal(i, w, u)` — u is a situation where party i does everything it ought to at w.
-- `O(i)` is the modality over it; axiom D is seeded (HS0022). Policy.hp proves D, the reading of O at an
-- ideal, that flying makes a trip flown and keeps it flown, and the norms the Moffatt case turns on.
--
-- THE POLICIES ARE MODELLED, NOT QUOTED: simplified from the bereavement policy as the tribunal describes
-- it and from common refund rules. Their ids are the ones in data/policies.json, where their wording lives.
sort Party
sort Act
sort Event
sort Moment
sort Policy
sort Kind

theory Policy (Party, Act, Event, Moment, Policy, Kind)
{
  -- ── the trace: moments, and what the customer does between them ─────────────────────────────────
  --
  -- SEEDED BY THE HOST, as time is in Traffic.hs: a rule cannot conclude that a moment exists (HS0022).
  def moment(id: Moment)
  def next(now: Moment, then: Moment)
  def happens(at: Moment, what: Event)

  law nowIsLater  = ∀ m,     moment(m) ⇒ later(m, m)
  law stepIsLater = ∀ m n,   next(m, n) ⇒ later(m, n)
  law onwards     = ∀ m n o, later(m, n) ∧ later(n, o) ⇒ later(m, o)

  -- ── the situation at each moment ────────────────────────────────────────────────────────────────
  --
  -- THE BOOKING IS THE SYSTEM OF RECORD, asserted at the first moment: `travelled`, `fare` and
  -- `cancelledByAirline` come from data/bookings.json, never from a model. The request is the customer's
  -- word, and the host asks it at every moment of the trace so the book can say how the answer changes.
  def party(id: Party)
  def act(id: Act)
  def request(at: Moment, what: Act)
  def bereavement(at: Moment)
  def travelled(at: Moment)
  def fare(at: Moment, kind: Kind)
  def cancelledByAirline(at: Moment)
  def did(at: Moment, who: Party, what: Act)

  -- FLOWN IS A FLUENT: true if the record says so, true the moment after flying, and true ever after.
  law flownOnRecord   = ∀ m,   travelled(m) ⇒ flown(m)
  law flyingIsFlown   = ∀ m n, happens(m, "fly") ∧ next(m, n) ⇒ flown(n)
  law flownStaysFlown = ∀ m n, flown(m) ∧ next(m, n) ⇒ flown(n)

  -- AND WHAT THE RECORD SAYS ABOUT THE FARE DOES NOT CHANGE BECAUSE TIME PASSES.
  law fareStays        = ∀ m n k, fare(m, k) ∧ next(m, n) ⇒ fare(n, k)
  law cancelledStays   = ∀ m n,   cancelledByAirline(m) ∧ next(m, n) ⇒ cancelledByAirline(n)
  law bereavementStays = ∀ m n,   bereavement(m) ∧ next(m, n) ⇒ bereavement(n)

  -- ── the deontic frame ──────────────────────────────────────────────────────────────────────────
  def holds(at: Moment, what: Act)
  def idealSeed(who: Party, from: Moment, to: Moment)

  modality O(i) over ideal at holds

  law serialO = ∀ i w u, idealSeed(i, w, u) ⇒ ideal(i, w, u)                                        -- D

  -- O(i) a AT w MEANS a HOLDS WHEREVER i DOES WHAT IT OUGHT: an obligation is read at every ideal.
  law atIdeal = ∀ i a w u, ought(w, i, a) ∧ ideal(i, w, u) ⇒ holds(u, a)

  -- ── BRV: bereavement fares ─────────────────────────────────────────────────────────────────────
  --
  -- BRV-1: asked for before flying, a bereavement OBLIGES the airline to offer the bereavement fare;
  -- without one, the fare is FORBIDDEN.
  law bereavementBefore = ∀ m, request(m, "bereavement_fare") ∧ bereavement(m) ∧ ¬ flown(m) ⇒ obliged(m, "airline", "bereavement_fare", "BRV-1")
  law notABereavement   = ∀ m, request(m, "bereavement_fare") ∧ ¬ bereavement(m) ∧ ¬ flown(m) ⇒ forbidden(m, "airline", "bereavement_fare", "BRV-1")

  -- BRV-2: AND ONCE FLOWN IT IS FORBIDDEN. This is the norm the chatbot contradicted.
  law bereavementAfter  = ∀ m, request(m, "bereavement_fare") ∧ flown(m) ⇒ forbidden(m, "airline", "bereavement_fare", "BRV-2")

  -- ── REF: refunds ───────────────────────────────────────────────────────────────────────────────
  --
  -- REF-1: a refundable fare, not yet flown, OBLIGES a refund.
  law refundable        = ∀ m, request(m, "refund") ∧ fare(m, "refundable") ∧ ¬ flown(m) ⇒ obliged(m, "airline", "refund", "REF-1")

  -- REF-2: a non-refundable fare FORBIDS a refund — and OBLIGES keeping its value as a travel credit.
  law nonRefundable     = ∀ m, request(m, "refund") ∧ fare(m, "nonrefundable") ∧ ¬ flown(m) ∧ ¬ cancelledByAirline(m) ⇒ forbidden(m, "airline", "refund", "REF-2")
  law keptAsCredit      = ∀ m, request(m, "refund") ∧ fare(m, "nonrefundable") ∧ ¬ flown(m) ∧ ¬ cancelledByAirline(m) ⇒ obliged(m, "airline", "travel_credit", "REF-2")

  -- REF-3: when the airline cancels, it is OBLIGED to refund, whatever kind of fare it was.
  law airlineCancelled  = ∀ m, request(m, "refund") ∧ cancelledByAirline(m) ⇒ obliged(m, "airline", "refund", "REF-3")

  -- REF-4: a trip already flown FORBIDS a refund.
  law alreadyFlown      = ∀ m, request(m, "refund") ∧ flown(m) ∧ ¬ cancelledByAirline(m) ⇒ forbidden(m, "airline", "refund", "REF-4")

  -- ── CHAT: what the assistant may promise ───────────────────────────────────────────────────────
  --
  -- CHAT-1: THE ASSISTANT MUST NOT PROMISE WHAT THE AIRLINE DOES NOT OWE, moment by moment. `act` is every
  -- remedy there is a word for, including one a model invented and the host asserted to check it.
  law promiseOnlyWhatIsOwed = ∀ m a, moment(m) ∧ act(a) ∧ ¬ ought(m, "airline", a) ⇒ forbidden(m, "assistant", a, "CHAT-1")

  -- ── what the norms add up to ───────────────────────────────────────────────────────────────────
  law owes        = ∀ m i a _source, obliged(m, i, a, _source) ⇒ ought(m, i, a)
  law mustNot     = ∀ m i a _source, forbidden(m, i, a, _source) ⇒ oughtNot(m, i, a)
  law permittedIf = ∀ m i a, moment(m) ∧ party(i) ∧ act(a) ∧ ¬ oughtNot(m, i, a) ⇒ permitted(m, i, a)
  law violates    = ∀ m i a, oughtNot(m, i, a) ∧ did(m, i, a) ⇒ violation(m, i, a)

  -- AND A BOOK THAT OBLIGES AND FORBIDS THE SAME ACT AT ONE MOMENT IS A DEFECT IN THE BOOK: axiom D says no
  -- ideal situation could satisfy it. `PolicyTests` asks for a conflict on every trace and expects none.
  law clash       = ∀ m i a, ought(m, i, a) ∧ oughtNot(m, i, a) ⇒ conflict(m, i, a)

  -- ── time and duty together ─────────────────────────────────────────────────────────────────────
  --
  -- LOST: owed now, and the customer's own step makes it forbidden next. "A refund, until you fly."
  law lostByAStep     = ∀ m n a e, ought(m, "airline", a) ∧ happens(m, e) ∧ next(m, n) ∧ oughtNot(n, "airline", a) ⇒ lostBy(m, a, e)

  -- F O a: some moment from here on owes it.
  law owedSometime    = ∀ m n a, later(m, n) ∧ ought(n, "airline", a) ⇒ eventuallyOwed(m, a)

  -- G ¬O a: no moment from here on owes what was asked — written, as Traffic.hs writes G, as the absence
  -- of a counter-example.
  law owedAtNoMoment  = ∀ m a, request(m, a) ∧ ¬ eventuallyOwed(m, a) ⇒ neverOwed(m, a)

  query obligations(at: Moment, who: Party, what?: Act, source?: Policy)  :- obliged(at, who, what, source)
  query prohibitions(at: Moment, who: Party, what?: Act, source?: Policy) :- forbidden(at, who, what, source)
  query permissions(at: Moment, who: Party, what?: Act)                   :- permitted(at, who, what)
  query violations(at: Moment, who?: Party, what?: Act)                   :- violation(at, who, what)
  query conflicts(at?: Moment, who?: Party, what?: Act)                   :- conflict(at, who, what)
  query ideally(at: Moment, what?: Act)                                   :- holds(at, what)
  query lapses(at?: Moment, what?: Act, by?: Event)                       :- lostBy(at, what, by)
  query eventually(from: Moment, what?: Act)                              :- eventuallyOwed(from, what)
  query never(from: Moment, what?: Act)                                   :- neverOwed(from, what)
  query flownAt(at?: Moment)                                              :- flown(at)
}
