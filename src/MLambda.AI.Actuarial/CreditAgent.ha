-- CreditAgent.ha — judges DEFAULT, from a hazard over SYNTHESISED loan data.
--
-- THE LOAN THIS AGENT JUDGES DOES NOT EXIST. Its loan-to-value and rate are invented from the sale price
-- and a seed, and its hazard coefficients are declared, not fitted -- see the header of Credit.hb. So
-- this agent's verdict demonstrates how a credit view enters a decision, and measures nothing.
--
-- ITS BELIEFS ARE BASIS POINTS of default chance, because guard arithmetic in `.ha` is integer: a 13.24%
-- chance is believed as 1324.
agent CreditAgent(id: String) : Risk
{
  belief chance(basisPoints: Int)
  belief tolerance(basisPoints: Int)
  belief ceiling(basisPoints: Int)

  desire Decided reachable by [Buy, Negotiate, Walk]

  commit single_minded
  attention Underwriting initially

  intend Buy       for Decided when B(self) chance(p) ∧ B(self) tolerance(t) ∧ p ≤ t
  intend Negotiate for Decided when B(self) chance(p) ∧ B(self) tolerance(t) ∧ B(self) ceiling(c) ∧ p > t ∧ p ≤ c
  intend Walk      for Decided when B(self) chance(p) ∧ B(self) ceiling(c) ∧ p > c

  -- MET ONCE THE HOST SAYS IT ACTED: the vocabulary's `settled(child, how)`, the verdict it carried out
  -- and "acted" -- see ValuationAgent.ha.
  achieved Decided when B(self) settled(_verdict, "acted")

  plan Buy       ↦ Purchase.Proceed
  plan Negotiate ↦ Purchase.Counter
  plan Walk      ↦ Purchase.Decline
}
