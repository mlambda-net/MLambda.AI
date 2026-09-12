-- Cleaner.ha — a gridworld agent with a plan library, shifting attention, and an honest failure.
--
-- `impossible … because "…"` IS THE KEYWORD WORTH THE WHOLE FILE. An agent that cannot reach its
-- goal has two options: keep trying forever, or stop and say why. Every other BDI framework makes
-- the second a log line somebody remembered to write. Here it is part of the agent, in words a
-- person reads, and a test can assert the sentence.
--
-- ATTENTION IS WHAT THE AGENT IS CURRENTLY MINDING. It starts on Cleaning and moves to Blocked when
-- the way is shut, which changes what the agent is about without changing what it wants.
--
-- A PLAN LIBRARY IS SEVERAL ROUTES TO ONE GOAL. The agent proposes all of them and chooses none:
-- choosing is the host's job, and an engine that chose would be hiding the decision.
agent Cleaner(id: String) : Agency
{
  belief dirt(room: String)
  belief reachable(room: String)
  belief blocked(room: String)
  belief cleaned(room: String)

  desire Spotless reachable by [Sweep, Mop]

  commit single_minded

  -- EVERY FOCUS IS DECLARED. `attend` may only name an attention this agent has (HS0063), so the
  -- set of things it can be about is fixed and readable, rather than arriving from wherever the
  -- rules happen to point.
  attention Cleaning initially
  attention Blocked

  when B(self) blocked(r) attend Blocked

  intend Sweep for Spotless when B(self) dirt(r) ∧ B(self) reachable(r)
  intend Mop   for Spotless when B(self) dirt(r) ∧ B(self) reachable(r)

  achieved Spotless when B(self) cleaned(r)

  -- AND THE REASON IS NOT OPTIONAL. The parser says so in as many words: "expected 'because' and
  -- the reason a person will read". An agent may not declare its goal impossible without saying
  -- why, in a sentence somebody can read. That is the language taking a position, and it is the
  -- right one.
  impossible Spotless when B(self) dirt(r) ∧ B(self) blocked(r) because "there is dirt in a room I cannot get to"

  reward Sweep positive when B(self) cleaned(r)

  plan Sweep ↦ House.Sweep
  plan Mop   ↦ House.Mop
}
