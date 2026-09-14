-- Knowledge.hs — knowing is not believing.
--
-- ONE FRAME, TWO ATTITUDES. Worlds are ways things might be; `holds(w, p)` says p is so at w. For
-- each agent, `knows(i, w, u)` says that at w the agent cannot rule u out, and `believes(i, w, u)`
-- says that at w the agent takes u to be how things are.
--
-- KNOWLEDGE IS S5: reflexive, transitive, euclidean. Reflexive is the one that matters — the actual
-- world is never ruled out, so whatever is known is TRUE. BELIEF IS KD45: serial, transitive,
-- euclidean, and NOT reflexive — the actual world may be ruled out, so a belief can be FALSE.
-- That single missing law is the whole difference, and `KnowledgeTests` shows it running while
-- `RefusalTests` shows the kernel refusing to prove it away.
--
-- `within` IS R_B ⊆ R_K: an agent believes at least what it knows. Every world belief takes seriously
-- is one knowledge has not ruled out.
--
-- "BELIEVES THAT p" IS A UNIVERSAL — p at every believed world — written as the absence of a
-- counter-example, `¬ doubts`. Legal because `doubts` never depends back on `believes_that`.
sort World
sort Agent
sort Prop

theory Knowledge (World, Agent, Prop)
{
  def world(id: World)
  def agent(id: Agent)
  def prop(id: Prop)
  def holds(at: World, p: Prop)

  -- What the host says the agent could not tell apart, and what it settled on.
  def glimpse(who: Agent, from: World, to: World)
  def guess(who: Agent, from: World, to: World)

  modality K(i) over knows at holds
  modality B(i) over believes at holds

  law reflK   = ∀ i w,     agent(i) ∧ world(w) ⇒ knows(i, w, w)                    -- T
  law seenK   = ∀ i w u,   glimpse(i, w, u) ⇒ knows(i, w, u)
  law transK  = ∀ i w u v, knows(i, w, u) ∧ knows(i, u, v) ⇒ knows(i, w, v)      -- 4
  law euclidK = ∀ i w u v, knows(i, w, u) ∧ knows(i, w, v) ⇒ knows(i, u, v)      -- 5

  law serialB = ∀ i w u,   guess(i, w, u) ⇒ believes(i, w, u)                    -- D
  law transB  = ∀ i w u v, believes(i, w, u) ∧ believes(i, u, v) ⇒ believes(i, w, v)
  law euclidB = ∀ i w u v, believes(i, w, u) ∧ believes(i, w, v) ⇒ believes(i, u, v)

  law within  = ∀ i w u,   believes(i, w, u) ⇒ knows(i, w, u)                    -- K p ⇒ B p

  law doubtB  = ∀ i w u p, believes(i, w, u) ∧ prop(p) ∧ ¬ holds(u, p) ⇒ doubtsB(i, w, p)
  law doubtK  = ∀ i w u p, knows(i, w, u) ∧ prop(p) ∧ ¬ holds(u, p) ⇒ doubtsK(i, w, p)
  law believesThat = ∀ i w p, agent(i) ∧ world(w) ∧ prop(p) ∧ ¬ doubtsB(i, w, p) ⇒ believes_that(i, w, p)
  law knowsThat    = ∀ i w p, agent(i) ∧ world(w) ∧ prop(p) ∧ ¬ doubtsK(i, w, p) ⇒ knows_that(i, w, p)
  law falseBelief  = ∀ i w p, believes_that(i, w, p) ∧ ¬ holds(w, p) ⇒ false_belief(i, w, p)

  query believed(who: Agent, at: World, p?: Prop) :- believes_that(who, at, p)
  query known(who: Agent, at: World, p?: Prop)    :- knows_that(who, at, p)
  query mistaken(who: Agent, at: World, p?: Prop) :- false_belief(who, at, p)
}
