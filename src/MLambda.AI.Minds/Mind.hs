-- Mind.hs — four attitudes, one agent, one frame.
--
-- WHAT A MENTAL STATE IS, in the terms of this subject: for each attitude, the set of worlds the
-- agent's attitude takes seriously. The attitudes differ in their LAWS, not in their data:
--
--   knows     S5 (T, 4, 5)   what is known is true
--   believes  KD, within K   consistent, may be false
--   desires   KD             consistent, need not be true — that is what wanting is
--   intends   KD, ⊇ desires  realism: an agent intends at least the worlds it desires (R_D ⊆ R_I)
--
-- `Agency.hs` in MLambda.AI.Agent is the agent-programming view of B, D and I; this file adds
-- knowledge and asks all four of one agent at once.
sort Agent
sort World
sort Prop

theory Mind (Agent, World, Prop)
{
  def agent(id: Agent)
  def world(id: World)
  def prop(id: Prop)
  def holds(at: World, p: Prop)

  -- What the host says: worlds the agent cannot rule out, settled on, wishes for, and planned for.
  def glimpse(who: Agent, from: World, to: World)
  def guess(who: Agent, from: World, to: World)
  def wish(who: Agent, from: World, to: World)
  def plan(who: Agent, from: World, to: World)

  modality K(i) over knows at holds
  modality B(i) over believes at holds
  modality D(i) over desires at holds
  modality I(i) over intends at holds

  law reflK   = ∀ i w,     agent(i) ∧ world(w) ⇒ knows(i, w, w)
  law seenK   = ∀ i w u,   glimpse(i, w, u) ⇒ knows(i, w, u)
  law transK  = ∀ i w u v, knows(i, w, u) ∧ knows(i, u, v) ⇒ knows(i, w, v)
  law euclidK = ∀ i w u v, knows(i, w, u) ∧ knows(i, w, v) ⇒ knows(i, u, v)

  law serialB = ∀ i w u,   guess(i, w, u) ⇒ believes(i, w, u)
  law within  = ∀ i w u,   believes(i, w, u) ⇒ knows(i, w, u)

  law serialD = ∀ i w u,   wish(i, w, u) ⇒ desires(i, w, u)
  law serialI = ∀ i w u,   plan(i, w, u) ⇒ intends(i, w, u)
  law realism = ∀ i w u,   desires(i, w, u) ⇒ intends(i, w, u)

  -- "i <attitude> that p" holds when no world the attitude takes seriously lacks p.
  law doubtK = ∀ i w u p, knows(i, w, u)    ∧ prop(p) ∧ ¬ holds(u, p) ⇒ unknown(i, w, p)
  law doubtB = ∀ i w u p, believes(i, w, u) ∧ prop(p) ∧ ¬ holds(u, p) ⇒ unbelieved(i, w, p)
  law doubtD = ∀ i w u p, desires(i, w, u)  ∧ prop(p) ∧ ¬ holds(u, p) ⇒ undesired(i, w, p)
  law doubtI = ∀ i w u p, intends(i, w, u)  ∧ prop(p) ∧ ¬ holds(u, p) ⇒ unintended(i, w, p)

  law knowsThat    = ∀ i w p, agent(i) ∧ world(w) ∧ prop(p) ∧ ¬ unknown(i, w, p)    ⇒ knows_that(i, w, p)
  law believesThat = ∀ i w p, agent(i) ∧ world(w) ∧ prop(p) ∧ ¬ unbelieved(i, w, p) ⇒ believes_that(i, w, p)
  law desiresThat  = ∀ i w p, agent(i) ∧ world(w) ∧ prop(p) ∧ ¬ undesired(i, w, p)  ⇒ desires_that(i, w, p)
  law intendsThat  = ∀ i w p, agent(i) ∧ world(w) ∧ prop(p) ∧ ¬ unintended(i, w, p) ⇒ intends_that(i, w, p)

  query known(who: Agent, at: World, p?: Prop)    :- knows_that(who, at, p)
  query believed(who: Agent, at: World, p?: Prop) :- believes_that(who, at, p)
  query desired(who: Agent, at: World, p?: Prop)  :- desires_that(who, at, p)
  query intended(who: Agent, at: World, p?: Prop) :- intends_that(who, at, p)
}
