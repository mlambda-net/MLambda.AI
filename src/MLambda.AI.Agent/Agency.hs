-- Agency.hs — belief, desire and intention, as one theory with three accessibility relations.
--
-- THE ATTITUDES ARE MODALITIES, NOT RECORDS. `B(i) p` is not "agent i has a belief field"; it is
-- "p holds at every world i takes to be possible". That is what makes realism a claim with content
-- rather than a naming convention. The standard translation turns each modality into a first-order
-- formula over an agent-indexed relation, so nothing downstream knows one was ever written.
--
-- THE AGENT COMES FIRST in every relation, which is the epistemic literature's convention: `K_a p`
-- reads `knows(a, w, u)`. An unindexed relation could not tell two agents' beliefs apart.
--
-- Sources: Rao & Georgeff, "Modeling Rational Agents within a BDI-Architecture" (KR 1991);
-- Chellas, Modal Logic: An Introduction (CUP 1980) §1.2 for the frame conditions.
sort Prop
sort World

theory Agency (Agent, World, Prop)
{
  def believes(who: Agent, from: World, to: World)
  def desires (who: Agent, from: World, to: World)
  def intends (who: Agent, from: World, to: World)

  -- Satisfaction: which propositions hold at which world. The relation every modality is
  -- evaluated against.
  def holds(at: World, prop: Prop)

  modality B(i) over believes at holds
  modality D(i) over desires  at holds
  modality I(i) over intends  at holds

  -- ── seriality, by a named successor ───────────────────────────────────────────────────────
  --
  -- AXIOM D FOR ALL THREE: an agent may not believe both p and ¬p, may not desire both, and may
  -- not intend both — and consistency of intention is what makes a plan library coherent at all.
  --
  -- IT IS SEEDED RATHER THAN INFERRED, because axiom D is `∀ w, ∃ u, R(w, u)` and a Horn rule
  -- cannot conclude that something EXISTS (HS0022). So each attitude carries a successor relation
  -- naming the world, and the frame condition follows by a rule binding `u` in its BODY.
  --
  -- THE CONSEQUENCE, PLAINLY: seriality holds by construction of the SEEDING, not by inference. A
  -- world nobody seeds a successor for has none, and nothing complains. Axiom D is a precondition
  -- on the runtime.
  def beliefSucc   (who: Agent, from: World, to: World)
  def desireSucc   (who: Agent, from: World, to: World)
  def intentionSucc(who: Agent, from: World, to: World)

  law serialB = ∀ i w u, beliefSucc(i, w, u)    ⇒ believes(i, w, u)
  law serialD = ∀ i w u, desireSucc(i, w, u)    ⇒ desires(i, w, u)
  law serialI = ∀ i w u, intentionSucc(i, w, u) ⇒ intends(i, w, u)

  -- ── introspection, for belief only ────────────────────────────────────────────────────────
  --
  -- KD45 IS BELIEF'S LOGIC AND KD IS THE OTHER TWO'S. An agent knows what it believes (4) and
  -- knows what it does not believe (5); nothing of the kind is true of WANTING. Desire and
  -- intention get consistency and no more, which is Rao and Georgeff's own assignment.
  law transB  = ∀ i w u v, believes(i, w, u) ∧ believes(i, u, v) ⇒ believes(i, w, v)
  law euclidB = ∀ i w u v, believes(i, w, u) ∧ believes(i, w, v) ⇒ believes(i, u, v)

  -- ── realism, as relation inclusion ────────────────────────────────────────────────────────
  --
  -- `I(i) p ⇒ D(i) p` IS NOT WRITABLE AS A LAW. Translated it is
  --   (∀u, intends(i,w,u) ⇒ holds(u,p)) ⇒ (∀u, desires(i,w,u) ⇒ holds(u,p))
  -- — an implication under a universal, IN A HEAD, which Horn cannot express (HS0020) for exactly
  -- the reason an existential cannot.
  --
  -- CORRESPONDENCE THEORY GIVES THE EXPRESSIBLE FORM: `□_I p ⇒ □_D p` holds precisely when
  -- R_D ⊆ R_I. So "what an agent intends, it desires" is one inclusion between relations — the
  -- same shape reflexivity and transitivity already use. The modal reading is unchanged; only the
  -- spelling is first-order.
  law realism = ∀ i w u, desires(i, w, u) ⇒ intends(i, w, u)
}
