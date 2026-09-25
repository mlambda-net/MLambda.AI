-- ValuationAgent.ha — judges OVERPAYMENT, from how many spreads the offer sits above fair value.
--
-- ITS BELIEFS ARE INTEGERS because guard arithmetic in `.ha` is integer: the excess is held in
-- HUNDREDTHS of a spread, so 1.23 spreads is believed as 123. The host scales Valuation.hb's answer.
--
-- IT IS SINGLE-MINDED: a purchase is decided once, not re-litigated every time a belief is refreshed.
agent ValuationAgent(id: String) : Risk
{
  belief excess(hundredths: Int)
  belief tolerance(hundredths: Int)
  belief ceiling(hundredths: Int)

  desire Decided reachable by [Buy, Negotiate, Walk]

  commit single_minded
  attention Appraising initially

  intend Buy       for Decided when B(self) excess(e) ∧ B(self) tolerance(t) ∧ e ≤ t
  intend Negotiate for Decided when B(self) excess(e) ∧ B(self) tolerance(t) ∧ B(self) ceiling(c) ∧ e > t ∧ e ≤ c
  intend Walk      for Decided when B(self) excess(e) ∧ B(self) ceiling(c) ∧ e > c

  -- A DESIRE MUST BE REACHABLE (HS0062: "an agent may not want what it can never reach"). Deciding is
  -- met once the host has acted on the intention and told the agent so -- the agent does not decide
  -- that it has decided.
  --
  -- `settled` IS THE AGENCY VOCABULARY'S, NOT A BELIEF OF THIS AGENT (HS0082): `settled(child, how)`,
  -- asserted by the host. Here the child is the verdict the host carried out and `how` is "acted".
  achieved Decided when B(self) settled(_verdict, "acted")

  plan Buy       ↦ Purchase.Proceed
  plan Negotiate ↦ Purchase.Counter
  plan Walk      ↦ Purchase.Decline
}
