-- Thermostat.ha — the smallest agent there is: one belief, one goal, two plans.
--
-- WHAT MAKES THIS AN AGENT AND NOT AN `if`. The `if` version tests a temperature and switches a
-- heater. This one holds a GOAL — `Comfortable` — and an intention is licensed only while that goal
-- is unmet. Nothing here performs anything: the agent deliberates, and a host reads what it decided
-- and acts. `plan Warm ↦ Room.Heat` names a LABEL, not a method; there is no `Room.Heat` anywhere
-- in this repository and there does not need to be.
--
-- AND WHAT THE ENGINE ACTUALLY RUNS IS THE GROUND READING. `B(self) temperature(t)` is a claim
-- about every world this thermostat takes to be possible; a modality in a rule BODY does not
-- compile (HS0052), so the engine runs `temperature(id, t)` at the actual world. That is not a
-- shortcut — it is the only thing that compiles, and it is why the same line lowers two ways.
agent Thermostat(id: String) : Agency
{
  belief temperature(degrees: Int)

  desire Comfortable reachable by [Warm, Cool]

  -- COMMITMENT IS NOT OPTIONAL, AND THAT IS A DESIGN STATEMENT. The compiler refuses an agent that
  -- does not say how long its intentions survive contact with the world (HS0060), and the three
  -- answers are the three in the literature:
  --
  --   blind          keep the intention until you believe it achieved, come what may
  --   single_minded  keep it until achieved OR believed impossible
  --   open_minded    keep it only while you still hold the goal, reconsidering as beliefs change
  --
  -- A THERMOSTAT IS OPEN-MINDED, because its world will not hold still. Somebody opens a window and
  -- the right thing to do changes; an agent that blindly finished heating would be a worse
  -- thermostat than a bimetallic strip. `Collector.ha` is single-minded, and the pair is the
  -- comparison worth making.
  commit open_minded

  attention Watching initially

  intend Warm for Comfortable when B(self) temperature(t) ∧ t < 18
  intend Cool for Comfortable when B(self) temperature(t) ∧ t > 24

  achieved Comfortable when B(self) temperature(t) ∧ t ≥ 18 ∧ t ≤ 24

  plan Warm ↦ Room.Heat
  plan Cool ↦ Room.Chill
}
