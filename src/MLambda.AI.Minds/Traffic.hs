-- Traffic.hs — time as a trace: what a light shows now, next, eventually and always.
--
-- A RUN IS A FINITE TRACE. Moments are asserted and `next` links them; `later` is the order they
-- make. Time is SEEDED rather than built from `start | tick(t)` because a rule cannot conclude that
-- a moment exists (HS0022), and "every moment is later than itself" needs `moment(m)` to bind `m`
-- (HS0020) — the same trick `Worlds.hs` plays with `place`.
--
-- F, G AND U ARE QUERIES YOU RUN. "Eventually" is one later moment that shows it. "Always" is the
-- absence of a later moment that does not — a universal written as negation-as-failure, which is
-- legal because `breaks` never depends back on `always`.
sort Moment
sort Colour

theory Traffic (Moment, Colour)
{
  def moment(id: Moment)
  def colour(id: Colour)
  def next(now: Moment, then: Moment)
  def shows(at: Moment, c: Colour)

  -- The order. Reflexive, because "always" in temporal logic includes now.
  law nowIsLater  = ∀ m,     moment(m) ⇒ later(m, m)
  law stepIsLater = ∀ m n,   next(m, n) ⇒ later(m, n)
  law onwards     = ∀ m n o, later(m, n) ∧ later(n, o) ⇒ later(m, o)

  -- F: some moment from here on shows it.
  law eventuallyIs = ∀ m n c, later(m, n) ∧ shows(n, c) ⇒ eventually(m, c)

  -- G: no moment from here on fails to show it.
  law breaksAt = ∀ m n c, later(m, n) ∧ colour(c) ∧ ¬ shows(n, c) ⇒ breaks(m, c)
  law alwaysIs = ∀ m c,   moment(m) ∧ colour(c) ∧ ¬ breaks(m, c) ⇒ always(m, c)

  -- U: `a until b` holds now if b shows now, or a shows now and `a until b` holds next.
  law untilNow  = ∀ m a b,   shows(m, b) ∧ colour(a) ⇒ until(m, a, b)
  law untilStep = ∀ m n a b, shows(m, a) ∧ next(m, n) ∧ until(n, a, b) ⇒ until(m, a, b)

  -- A light shows one colour at a time.
  law clashes = ∀ m a b, shows(m, a) ∧ shows(m, b) ∧ a ≠ b ⇒ clash(m)

  query eventually_shows(from: Moment, c?: Colour)       :- eventually(from, c)
  query always_shows(from: Moment, c?: Colour)           :- always(from, c)
  query waits_until(from: Moment, a: Colour, b: Colour)  :- until(from, a, b)
  query clashing(at?: Moment)                            :- clash(at)
  query moments_after(from: Moment, to?: Moment)         :- later(from, to)
}
