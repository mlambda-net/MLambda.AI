-- ConditionAgent.ha — judges OBSOLESCENCE, from the repair exposure Condition.hb prices.
--
-- ITS BELIEFS ARE WHOLE THOUSANDS OF DOLLARS, because guard arithmetic in `.ha` is integer. The exposure
-- is points below a typical rating times what the market paid per point, within the house's age band.
agent ConditionAgent(id: String) : Risk
{
  belief exposure(thousands: Int)
  belief tolerance(thousands: Int)
  belief ceiling(thousands: Int)
  belief settled(done: Int)

  desire Decided reachable by [Buy, Negotiate, Walk]

  commit single_minded
  attention Inspecting initially

  intend Buy       for Decided when B(self) exposure(x) ∧ B(self) tolerance(t) ∧ x ≤ t
  intend Negotiate for Decided when B(self) exposure(x) ∧ B(self) tolerance(t) ∧ B(self) ceiling(c) ∧ x > t ∧ x ≤ c
  intend Walk      for Decided when B(self) exposure(x) ∧ B(self) ceiling(c) ∧ x > c

  achieved Decided when B(self) settled(1)

  plan Buy       ↦ Purchase.Proceed
  plan Negotiate ↦ Purchase.Counter
  plan Walk      ↦ Purchase.Decline
}
