-- Risk.hs — what an agent judging a purchase may believe, and what it may not believe at once.
--
-- THE THREE ATTITUDES, as in MLambda.AI.Agent's Agency.hs: belief, desire and intention are modalities
-- over three accessibility relations, belief is KD45 and the others KD, seriality is a generator
-- relation (HS0022) and realism is relation inclusion (HS0020). An agent here extends this theory, so
-- every `B(self)` in the three `.ha` files is read against it.
--
-- AND ONE THING SPECIFIC TO RISK: a price cannot be both acceptable and ruinous to the same agent. That
-- is written the way Sorts.hs writes disjointness -- a derivation OF absurdity from the pair that would
-- witness it -- because there is no negated goal to introduce.
sort Prop
sort World

theory Risk (Agent, World, Prop, Offer)
{
  def believes(who: Agent, from: World, to: World)
  def desires (who: Agent, from: World, to: World)
  def intends (who: Agent, from: World, to: World)
  def holds(at: World, prop: Prop)

  modality B(i) over believes at holds
  modality D(i) over desires  at holds
  modality I(i) over intends  at holds

  def beliefSucc   (who: Agent, from: World, to: World)
  def desireSucc   (who: Agent, from: World, to: World)
  def intentionSucc(who: Agent, from: World, to: World)

  law serialB = ∀ i w u, beliefSucc(i, w, u)    ⇒ believes(i, w, u)
  law serialD = ∀ i w u, desireSucc(i, w, u)    ⇒ desires(i, w, u)
  law serialI = ∀ i w u, intentionSucc(i, w, u) ⇒ intends(i, w, u)

  law transB  = ∀ i w u v, believes(i, w, u) ∧ believes(i, u, v) ⇒ believes(i, w, v)
  law euclidB = ∀ i w u v, believes(i, w, u) ∧ believes(i, w, v) ⇒ believes(i, u, v)

  law realism = ∀ i w u, desires(i, w, u) ⇒ intends(i, w, u)

  -- ── judging one offer ─────────────────────────────────────────────────────────────────────
  def assessed(who: Agent, offer: Offer)
  def within(who: Agent, offer: Offer)
  def ruinous(who: Agent, offer: Offer)
  def absurd(who: Agent)

  -- AN ASSESSED OFFER WITHIN TOLERANCE IS ACCEPTABLE.
  law clears = ∀ i o, assessed(i, o) ∧ within(i, o) ⇒ acceptable(i, o)

  -- AND ACCEPTABLE AND RUINOUS AT ONCE IS A CONTRADICTION -- the property an adjudicator relies on when
  -- it trusts an agent that says "buy".
  law exclusive = ∀ i o, acceptable(i, o) ∧ ruinous(i, o) ⇒ absurd(i)

  query acceptable_offers(who: Agent, offer?: Offer) :- acceptable(who, offer)
  query contradicted(who?: Agent)                     :- absurd(who)
}
