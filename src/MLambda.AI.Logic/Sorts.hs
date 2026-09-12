-- Sorts.hs — classification, disjointness, and sort-relative identity.
--
-- A SORTAL DOES TWO JOBS AT ONCE, and the subject comes from taking both seriously. It CLASSIFIES
-- — this is a horse — and it INDIVIDUATES: it says what makes this horse the same horse as that
-- one. The second job is the one adjectives cannot do ("red" tells you nothing about how to count
-- reds), and it is why identity here is written `same(x, y, s)`, a THREE-place relation. Two things
-- may be the same F and different Gs, and nothing in the logic collapses that.
--
-- `absurd` IS HOW THIS THEORY SAYS "NO". There is no `¬` goal to introduce, so a claim of emptiness
-- is written as a derivation OF absurdity from the instance that would witness it — which is what
-- "S and T are disjoint" means anyway: nothing can be both.
--
-- EVERY LAW HERE BINDS ITS HEAD VARIABLES IN ITS BODY, which is why this theory both proves and
-- runs. `sub_refl` would like to be `∀ s, ⊤ ⇒ sub(s, s)` and cannot be, for the reason
-- `Worlds.hs` sets out at length; `kind` is its generator relation.
theory Sorts (Kind, Thing)
{
  def kind(id: Kind)
  def sub(lower: Kind, upper: Kind)
  def inst(what: Thing, sort: Kind)
  def disjoint(left: Kind, right: Kind)
  def same(left: Thing, right: Thing, under: Kind)
  def absurd(witness: Thing)

  -- ── subsumption is a pre-order ────────────────────────────────────────────────────────────
  law sub_refl  = ∀ s,     kind(s) ⇒ sub(s, s)
  law sub_trans = ∀ s t v, sub(s, t) ∧ sub(t, v) ⇒ sub(s, v)

  -- ── classification climbs ─────────────────────────────────────────────────────────────────
  --
  -- The one law connecting the two primitives: an instance of a sort is an instance of anything
  -- that sort falls under. Everything about classification is this plus transitivity.
  law inst_sub = ∀ x s t, inst(x, s) ∧ sub(s, t) ⇒ inst(x, t)

  -- ── disjointness ──────────────────────────────────────────────────────────────────────────
  law disjoint_symm = ∀ s t,   disjoint(s, t) ⇒ disjoint(t, s)
  law disjoint_down = ∀ s t u, disjoint(s, t) ∧ sub(u, s) ⇒ disjoint(u, t)

  -- WHAT DISJOINTNESS MEANS: a thing in both is a contradiction.
  law disjoint_excludes = ∀ x s t, disjoint(s, t) ∧ inst(x, s) ∧ inst(x, t) ⇒ absurd(x)

  -- ── individuation: identity under a sort ──────────────────────────────────────────────────
  --
  -- An equivalence on the instances of each sort, and it CLIMBS but does not descend. `same_up` is
  -- the climbing. The absence of a descending law is the whole of Wiggins's point, and `Sorts.hp`
  -- records that absence as a non-theorem rather than as a comment.
  law same_refl  = ∀ x s,     inst(x, s) ⇒ same(x, x, s)
  law same_symm  = ∀ x y s,   same(x, y, s) ⇒ same(y, x, s)
  law same_trans = ∀ x y z s, same(x, y, s) ∧ same(y, z, s) ⇒ same(x, z, s)
  law same_up    = ∀ x y s t, same(x, y, s) ∧ sub(s, t) ⇒ same(x, y, t)

  query sorts_of(what: Thing, sort?: Kind)     :- inst(what, sort)
  query impossible(witness?: Thing)            :- absurd(witness)
  -- UNDER WHICH SORTS ARE THESE TWO THE SAME? This is the question the whole subject is about, and
  -- it is also the only shape the lowering allows over a three-place relation today: inputs first,
  -- the `?` output last, and the body atom in that same order. Asking it the other way round --
  -- "who is the same as whom, under this sort?" -- would put the output first and silently answer
  -- nothing. See docs/hilbert/06-diagnostics.md.
  query sorts_sharing(left: Thing, right: Thing, under?: Kind) :- same(left, right, under)
}
