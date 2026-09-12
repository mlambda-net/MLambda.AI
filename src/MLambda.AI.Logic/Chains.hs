-- Chains.hs — modus ponens, as a rule the engine applies.
--
-- WHAT IS DIFFERENT HERE. `Animals` reasoned about cats and `Families` about people; this reasons
-- about STATEMENTS. `says(p, q)` is the claim "if p then q", and `holds(p)` is "p is true". The one
-- law is the oldest rule in logic, and watching a machine chain it three deep is the point.
--
-- THE STATEMENTS ARE NAMES, not formulas. A statement here is a label like "raining"; nothing
-- inspects its inside. That is enough for the rule, and keeping it that way is what makes this an
-- L1 sample.
--
-- AND `holds` IS BOTH ASSERTED AND CONCLUDED, unlike `is_a` in `Animals` which was purely derived.
-- A host says which claims are true to begin with, and the law says which others follow. Both kinds
-- of fact live in the same relation, which is exactly what makes chaining work.
theory Chains (Claim)
{
  def says(if_this: Claim, then_that: Claim)
  def holds(what: Claim)

  -- MODUS PONENS. If p is true, and p says q, then q is true.
  law ponens = ∀ p q, holds(p) ∧ says(p, q) ⇒ holds(q)

  query follows(from: Claim, reached?: Claim) :- says(from, reached)
  query truths(what?: Claim)                  :- holds(what)
}
