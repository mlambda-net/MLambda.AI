-- Policy.hs — the airline's policies as expert rules: what each party OUGHT to do, and what it MUST NOT.
--
-- THE CASE THIS SAMPLE EXISTS FOR. In Moffatt v. Air Canada (2024 BCCRT 149) a customer whose
-- grandmother had died asked the airline's chatbot about bereavement fares. The chatbot told him he could
-- fly at the full price and claim the bereavement discount back within 90 days. The written policy said
-- the opposite: a bereavement fare is not applied to travel already taken. The tribunal held the airline
-- to what its chatbot said. A language model had committed a company to a rule the company never wrote.
--
-- SO THE POLICY BOOK IS DEONTIC LOGIC, the logic of Duty.hs in MLambda.AI.Minds. Every policy is a NORM
-- with a party, an act and its source: `obliged(c, "airline", "refund", "REF-1")` is O(airline) refund in
-- situation c, because REF-1 says so; `forbidden(c, "airline", "bereavement_fare", "BRV-2")` is
-- F(airline) bereavement_fare. What is not forbidden is permitted. An obligation does not make itself
-- true, and a forbidden act that is done anyway is a VIOLATION.
--
-- AND THE ASSISTANT IS A PARTY TOO. For the assistant, an act is a PROMISE: "refund" is "promise a refund".
-- CHAT-1 is the norm the Moffatt chatbot lacked — the assistant must not promise what the airline is not
-- obliged to give — so a model's draft that promises more is an act the book forbids, and `violation`
-- says so before the customer reads a word.
--
-- A FRAME THAT PROVES. `ideal(i, w, u)` — u is a situation where party i does everything it ought to at w.
-- `O(i)` is the modality over it. Axiom D (every situation has an ideal, so nothing is both obligatory and
-- forbidden in it) is seeded, as seriality is everywhere in this repository, because a rule cannot conclude
-- that a situation exists (HS0022). Policy.hp proves D, the reading of O at an ideal, and the norms the
-- Moffatt case turns on.
--
-- THE POLICIES ARE MODELLED, NOT QUOTED: simplified from the bereavement policy as the tribunal describes
-- it and from common refund rules. Their ids are the ones in data/policies.json, where their wording lives.
sort Party
sort Act
sort Situation
sort Policy
sort Kind

theory Policy (Party, Act, Situation, Policy, Kind)
{
  -- ── the situation: what the customer asks, and what the booking record says ───────────────────
  --
  -- THE BOOKING IS THE SYSTEM OF RECORD, the request is the customer's word. `travelled`, `fare` and
  -- `cancelledByAirline` come from data/bookings.json, never from a model: a customer who types "I have
  -- not flown yet" does not make it so.
  def situation(id: Situation)
  def party(id: Party)
  def act(id: Act)
  def request(at: Situation, what: Act)
  def bereavement(at: Situation)
  def travelled(at: Situation)
  def fare(at: Situation, kind: Kind)
  def cancelledByAirline(at: Situation)
  def did(at: Situation, who: Party, what: Act)

  -- ── the deontic frame ──────────────────────────────────────────────────────────────────────────
  def holds(at: Situation, what: Act)
  def idealSeed(who: Party, from: Situation, to: Situation)

  modality O(i) over ideal at holds

  law serialO = ∀ i w u, idealSeed(i, w, u) ⇒ ideal(i, w, u)                                        -- D

  -- O(i) a AT w MEANS a HOLDS WHEREVER i DOES WHAT IT OUGHT: an obligation is read at every ideal.
  law atIdeal = ∀ i a w u, ought(w, i, a) ∧ ideal(i, w, u) ⇒ holds(u, a)

  -- ── BRV: bereavement fares ─────────────────────────────────────────────────────────────────────
  --
  -- BRV-1: asked for before the trip, a bereavement OBLIGES the airline to offer the bereavement fare;
  -- without one, the fare is FORBIDDEN.
  law bereavementBefore = ∀ w, request(w, "bereavement_fare") ∧ bereavement(w) ∧ ¬ travelled(w) ⇒ obliged(w, "airline", "bereavement_fare", "BRV-1")
  law notABereavement   = ∀ w, request(w, "bereavement_fare") ∧ ¬ bereavement(w) ∧ ¬ travelled(w) ⇒ forbidden(w, "airline", "bereavement_fare", "BRV-1")

  -- BRV-2: AND AFTER THE TRIP IT IS FORBIDDEN. This is the norm the chatbot contradicted.
  law bereavementAfter  = ∀ w, request(w, "bereavement_fare") ∧ travelled(w) ⇒ forbidden(w, "airline", "bereavement_fare", "BRV-2")

  -- ── REF: refunds ───────────────────────────────────────────────────────────────────────────────
  --
  -- REF-1: a refundable fare, not yet flown, OBLIGES a refund.
  law refundable        = ∀ w, request(w, "refund") ∧ fare(w, "refundable") ∧ ¬ travelled(w) ⇒ obliged(w, "airline", "refund", "REF-1")

  -- REF-2: a non-refundable fare FORBIDS a refund — and OBLIGES keeping its value as a travel credit.
  law nonRefundable     = ∀ w, request(w, "refund") ∧ fare(w, "nonrefundable") ∧ ¬ travelled(w) ∧ ¬ cancelledByAirline(w) ⇒ forbidden(w, "airline", "refund", "REF-2")
  law keptAsCredit      = ∀ w, request(w, "refund") ∧ fare(w, "nonrefundable") ∧ ¬ travelled(w) ∧ ¬ cancelledByAirline(w) ⇒ obliged(w, "airline", "travel_credit", "REF-2")

  -- REF-3: when the airline cancels, it is OBLIGED to refund, whatever kind of fare it was.
  law airlineCancelled  = ∀ w, request(w, "refund") ∧ cancelledByAirline(w) ⇒ obliged(w, "airline", "refund", "REF-3")

  -- REF-4: a trip already flown FORBIDS a refund.
  law alreadyFlown      = ∀ w, request(w, "refund") ∧ travelled(w) ∧ ¬ cancelledByAirline(w) ⇒ forbidden(w, "airline", "refund", "REF-4")

  -- ── CHAT: what the assistant may promise ───────────────────────────────────────────────────────
  --
  -- CHAT-1: THE ASSISTANT MUST NOT PROMISE WHAT THE AIRLINE DOES NOT OWE. `act` is every remedy there is
  -- a word for, including one a model invented and the host asserted to check it; for each one the airline
  -- is not obliged to give, promising it is forbidden.
  law promiseOnlyWhatIsOwed = ∀ w a, situation(w) ∧ act(a) ∧ ¬ ought(w, "airline", a) ⇒ forbidden(w, "assistant", a, "CHAT-1")

  -- ── what the norms add up to ───────────────────────────────────────────────────────────────────
  law owes        = ∀ w i a _source, obliged(w, i, a, _source) ⇒ ought(w, i, a)
  law mustNot     = ∀ w i a _source, forbidden(w, i, a, _source) ⇒ oughtNot(w, i, a)
  law permittedIf = ∀ w i a, situation(w) ∧ party(i) ∧ act(a) ∧ ¬ oughtNot(w, i, a) ⇒ permitted(w, i, a)
  law violates    = ∀ w i a, oughtNot(w, i, a) ∧ did(w, i, a) ⇒ violation(w, i, a)

  -- AND A BOOK THAT OBLIGES AND FORBIDS THE SAME ACT IS A DEFECT IN THE BOOK: axiom D says no ideal
  -- situation could satisfy it. `PolicyTests` asks for a conflict on every booking and expects none.
  law clash       = ∀ w i a, ought(w, i, a) ∧ oughtNot(w, i, a) ⇒ conflict(w, i, a)

  query obligations(at: Situation, who: Party, what?: Act, source?: Policy)  :- obliged(at, who, what, source)
  query prohibitions(at: Situation, who: Party, what?: Act, source?: Policy) :- forbidden(at, who, what, source)
  query permissions(at: Situation, who: Party, what?: Act)                   :- permitted(at, who, what)
  query violations(at: Situation, who?: Party, what?: Act)                   :- violation(at, who, what)
  query conflicts(at?: Situation, who?: Party, what?: Act)                   :- conflict(at, who, what)
  query ideally(at: Situation, what?: Act)                                   :- holds(at, what)
}
