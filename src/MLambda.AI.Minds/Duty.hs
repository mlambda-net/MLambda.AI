-- Duty.hs — what ought to be, read two ways.
--
-- A RULEBOOK THAT RUNS. Roles carry obligations and prohibitions; whoever plays a role inherits
-- them. What is not forbidden is permitted. And an obligation DOES NOT MAKE ITSELF TRUE: somebody
-- obliged to lock up may not have, and `violation` is how the engine says so.
--
-- A FRAME THAT PROVES. `ideal(w, u)` — u is a situation where everything that ought to be, is. Axiom
-- D says every situation has one, so nothing can be both obligatory and forbidden in it. It is
-- seeded, like seriality everywhere in this repository, because a rule cannot conclude that a
-- situation exists (HS0022). What is NOT a law, on purpose, is `ideal(w, w)`: this situation is
-- not ideal just because it is this one. That is the deontic T, and it must not hold.
sort Person
sort Role
sort Act
sort Situation

theory Duty (Person, Role, Act, Situation)
{
  def person(id: Person)
  def act(id: Act)
  def plays(who: Person, role: Role)
  def obliges(role: Role, what: Act)
  def forbids(role: Role, what: Act)
  def did(who: Person, what: Act)

  law obligedBy   = ∀ p r a, plays(p, r) ∧ obliges(r, a) ⇒ obliged(p, a)
  law forbiddenBy = ∀ p r a, plays(p, r) ∧ forbids(r, a) ⇒ forbidden(p, a)
  law permittedIf = ∀ p a,   person(p) ∧ act(a) ∧ ¬ forbidden(p, a) ⇒ permitted(p, a)
  law violates    = ∀ p a,   obliged(p, a) ∧ ¬ did(p, a) ⇒ violation(p, a)
  law clash       = ∀ p a,   obliged(p, a) ∧ forbidden(p, a) ⇒ conflict(p, a)

  def situation(id: Situation)
  def idealSeed(from: Situation, to: Situation)

  law serialO = ∀ w u, idealSeed(w, u) ⇒ ideal(w, u)                         -- D

  query obligations(who: Person, what?: Act) :- obliged(who, what)
  query permissions(who: Person, what?: Act) :- permitted(who, what)
  query violations(who?: Person, what?: Act) :- violation(who, what)
  query conflicts(who?: Person, what?: Act)  :- conflict(who, what)
}
