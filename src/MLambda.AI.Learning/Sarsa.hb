-- Sarsa.hb — the same step, on-policy, and one line different from QTable.
--
-- ON-POLICY: the target uses the action the learner ACTUALLY TOOK at the next state, not the best
-- one available there. So Sarsa learns the value of the policy it is following — exploration
-- included — where Q-learning learns the value of a greedy policy it is not following.
--
-- ON A CLIFF THAT DIFFERENCE IS VISIBLE. Q-learning finds the optimal path along the edge and falls
-- off whenever exploration nudges it; Sarsa accounts for its own exploration and walks further from
-- the edge. Neither is wrong. They answer different questions, and this is the world where that
-- stops being a sentence in a textbook.
open Prelude.Temporal

model Sarsa(states, actions) : Learner {

  weight q : Tensor[states, actions] from 0

  def train(s, a, rw, s2, a2, done, rate, gamma) {
    q ≔ nudged(q, marked(stateIds(q), s), marked(actionIds(q), a), sarsaTarget(q, marked(stateIds(q), s2), marked(actionIds(q), a2), rw, gamma, done), rate)
  }

  def worth ≔ rowMost(q)
  def plan  ≔ greedyOf(q)
  def act(s, eps) ≔ einsum("is,sa->a", marked(stateIds(q), s), exploring(q, eps))
}
