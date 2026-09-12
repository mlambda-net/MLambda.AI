-- Worlds.hs — the frame conditions, once, for every normal modal logic.
--
-- A LOGIC IS A CHOICE OF WHICH OF THESE IS OPEN, and that choice is one line in a `.hp` file:
--
--   K     nothing below
--   T     refl                    KD    serial
--   S4    refl, trans             K5    euclid
--   B     refl, symm              S5    refl, trans, euclid
--   KD45  serial, trans, euclid
--
-- The question "did you mean K5 or S5?" is therefore never asked of the language: both are written,
-- both are checked by the same kernel, and a theorem records which set it was proved under.
--
-- NOTHING HERE MENTIONS A MODALITY. Each condition is stated over `sees` alone, which is what makes
-- them composable: a theory may take any subset without the rest.
sort Place

theory Worlds (Place)
{
  def sees(from: Place, to: Place)

  -- EVERY WORLD THE HOST DECLARES. This exists for the same reason `seed` does below, and finding
  -- that out was the lesson of writing this file.
  --
  -- Reflexivity wants to be `∀ w, ⊤ ⇒ sees(w, w)` — "every world sees itself, from nothing". That
  -- is how the upstream corpus writes it, and it does NOT LOWER: `⊤` is not a positive body atom,
  -- so `w` is bound by nothing and the head invents a world (HS0020). The corpus gets away with it
  -- because it only ever PROVES over that theory and never runs it.
  --
  -- This theory does both, so `w` has to come from somewhere. `place` is that somewhere: a host
  -- says which worlds exist, and reflexivity holds of each. Same trick as seriality, one law
  -- earlier -- which is the honest shape of a frame condition in a Horn engine.
  def place(id: Place)

  law refl   = ∀ w,     place(w) ⇒ sees(w, w)                -- T
  law trans  = ∀ w u v, sees(w, u) ∧ sees(u, v) ⇒ sees(w, v) -- 4
  law euclid = ∀ w u v, sees(w, u) ∧ sees(w, v) ⇒ sees(u, v) -- 5
  law symm   = ∀ w u,   sees(w, u) ⇒ sees(u, w)              -- B

  -- SERIALITY IS A GENERATOR RELATION, not a bare law. Axiom D is `∀ w, ∃ u, sees(w, u)`, and a
  -- Horn rule cannot conclude that something EXISTS (HS0022). So the successor is NAMED, and the
  -- frame condition follows from it by an ordinary rule binding `u` in its BODY.
  --
  -- THE CONSEQUENCE IS WORTH SAYING PLAINLY: D holds of a frame exactly insofar as something seeded
  -- it. A world nobody seeds a successor for has none, and nothing complains. Axiom D is a
  -- precondition on the runtime, not an inference.
  def seed(from: Place, to: Place)

  law serial = ∀ w u, seed(w, u) ⇒ sees(w, u)                -- D

  query seen_from(from: Place, to?: Place) :- sees(from, to)
}
