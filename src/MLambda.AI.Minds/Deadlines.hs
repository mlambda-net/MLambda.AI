-- Deadlines.hs — a duty with a date, and whether you knew about it.
--
-- THREE LOGICS IN ONE RULE. `breached` is deontic (a duty unmet), temporal (its deadline has passed)
-- and — once `knows_due` is asked — epistemic: a breach you were told about is not the same verdict
-- as a breach nobody told you of. The engine separates the two; the page explains why a court would.
--
-- WHAT YOU WERE TOLD, YOU STILL KNOW LATER. `remembers` carries knowledge of a duty forward along
-- `later` — knowledge monotone over time, which is an assumption about these agents, stated as a law.
sort Person
sort Act
sort Moment

theory Deadlines (Person, Act, Moment)
{
  def moment(id: Moment)
  def next(now: Moment, then: Moment)
  def today(at: Moment)
  def due(who: Person, what: Act, by: Moment)
  def did(at: Moment, who: Person, what: Act)
  def informed(who: Person, what: Act, at: Moment)

  law nowIsLater  = ∀ m,     moment(m) ⇒ later(m, m)
  law stepIsLater = ∀ m n,   next(m, n) ⇒ later(m, n)
  law onwards     = ∀ m n o, later(m, n) ∧ later(n, o) ⇒ later(m, o)

  law remembers = ∀ p a m n,  informed(p, a, m) ∧ later(m, n) ⇒ knows_due(p, a, n)
  law fulfils   = ∀ p a m by, due(p, a, by) ∧ did(m, p, a) ∧ later(m, by) ⇒ fulfilled(p, a, by)
  law passes    = ∀ by n,     today(n) ∧ later(by, n) ∧ by ≠ n ⇒ passed(by)
  law breaches  = ∀ p a by,   due(p, a, by) ∧ passed(by) ∧ ¬ fulfilled(p, a, by) ⇒ breached(p, a, by)
  law knowingly = ∀ p a by,   breached(p, a, by) ∧ knows_due(p, a, by) ⇒ knowing_breach(p, a)
  law excuses   = ∀ p a by,   breached(p, a, by) ∧ ¬ knows_due(p, a, by) ⇒ excused(p, a)

  query breaches_of(who?: Person, what?: Act)       :- knowing_breach(who, what)
  query excused_of(who?: Person, what?: Act)        :- excused(who, what)
  query kept(who?: Person, what?: Act, by?: Moment) :- fulfilled(who, what, by)
}
