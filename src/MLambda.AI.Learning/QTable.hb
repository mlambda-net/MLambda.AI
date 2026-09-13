-- QTable.hb — Q-learning, one batch of experience per call.
--
-- THE CONSTRUCTOR TAKES THE SHAPE AND `train` TAKES THE EXPERIENCE. QTable(48, 4) is a cliff walk
-- and QTable(500, 6) an inventory problem; one declaration, both sizes.
--
-- `q` IS NAMED FOUR TIMES IN ONE ASSIGNMENT AND EVERY ONE IS THE ENTRY VALUE — the generated method
-- opens `var q = this.Q;` and the body names the local. So the target is computed from the table
-- this call was ENTERED with, which is what a temporal-difference step means.
--
-- OFF-POLICY: `qTarget` takes the BEST action at the next state, whatever the learner actually did
-- there. That is the one line that separates this file from `Sarsa.hb`.
open Prelude.Temporal

model QTable(states, actions) : Learner {

  weight q : Tensor[states, actions] from 0

  def train(s, a, rw, s2, done, rate, gamma) {
    q ≔ nudged(q, marked(stateIds(q), s), marked(actionIds(q), a), qTarget(q, marked(stateIds(q), s2), rw, gamma, done), rate)
  }

  def worth ≔ rowMost(q)
  def plan  ≔ greedyOf(q)
  def act(s, eps) ≔ einsum("is,sa->a", marked(stateIds(q), s), exploring(q, eps))
}
