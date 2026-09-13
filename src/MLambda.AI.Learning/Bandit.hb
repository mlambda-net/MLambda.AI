-- Bandit.hb — the one-state MDP, and the oldest exploration trick there is.
--
-- A BANDIT IS REINFORCEMENT LEARNING WITH THE MACHINERY TAKEN AWAY. There are no states and no
-- transitions — only arms, rewards, and the choice between pulling the arm you believe is best and
-- pulling one you know less about. Everything else in the field is that choice with more structure.
--
-- OPTIMISM NEEDS NO ε. A table that starts ABOVE anything the arms can pay makes every unpulled arm
-- look best until it has been tried — which is exploration without a coin flip, and why `start` is
-- an argument rather than a hard-coded zero.
--
-- TWO WEIGHTS, NOT ONE. `total` and `counts` are kept apart so an estimate is a division at question
-- time rather than a running update — which means a reader can see exactly what an estimate IS.
--
-- AND EXPLORATION IS ARITHMETIC, NEVER A BRANCH. `choose` answers a DISTRIBUTION over arms, not an
-- arm: ε-greedy is a weighted sum of two policies. The consequence every implementation that
-- branches forgets is that the exploring half may ALSO pick the greedy arm — so its real chance is
-- 1 − ε + ε/A, not 1 − ε. Written as a sum, that falls out of the algebra.
open Prelude.Policies

model Bandit(arms, start) : Learner {

  weight total  : Tensor[1, arms] from 0
  weight counts : Tensor[1, arms] from 0

  def train(a, rw) {
    total  ≔ total + einsum("i,ia->a", rw, marked(actionIds(total), a))
    counts ≔ counts + einsum("i,ia->a", ones(rw), marked(actionIds(counts), a))
  }

  -- AN UNPULLED ARM IS WORTH `start`; a pulled one is worth what it has paid on average.
  def means ≔ (total + start · (counts = 0)) / safe(counts)

  def choose(eps) ≔ exploring(means, eps)
  def ucb(c)      ≔ confident(means, c, counts)
}
