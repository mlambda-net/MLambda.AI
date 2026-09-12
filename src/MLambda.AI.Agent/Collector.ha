-- Collector.ha — the whole BDI cycle in one agent: believe, desire, intend, achieve, learn.
--
-- THE COMPARISON WITH `Thermostat` IS THE POINT OF THIS FILE. That one is open-minded because its
-- world will not hold still. This one is SINGLE-MINDED: it keeps the intention until the goal is
-- achieved or becomes impossible — not until something more attractive turns up. That is the
-- difference between an agent that finishes things and one that does not, and it is one keyword.
--
-- Kinny and Georgeff's result is worth knowing here: a BOLD agent, which rarely reconsiders, wins
-- in a world that changes slowly, and loses badly in one that changes fast. Commitment is not a
-- virtue; it is a bet about the world.
--
-- `value` IS A DERIVED QUANTITY the conditions may use, so the arithmetic is written once.
--
-- `reward` IS THE SEAM TO REINFORCEMENT LEARNING: the same feedback that marks a plan good here is
-- the signal a learner maximises in MLambda.AI.Learning.
agent Collector(id: String) : Agency
{
  belief gathered(count: Int)

  desire Enough reachable by [Collect]

  commit single_minded
  attention Gathering initially

  value remaining ≔ 5 − seen

  intend Collect for Enough when B(self) gathered(seen) ∧ seen < 5

  achieved Enough when B(self) gathered(5)

  reward Collect positive when B(self) gathered(5)

  plan Collect ↦ World.PickUp
}
