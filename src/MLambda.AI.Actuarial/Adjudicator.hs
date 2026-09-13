-- Adjudicator.hs — three agents disagree; one verdict comes out, and it is a veto, not a vote.
--
-- THE RULE, IN ORDER OF STRENGTH:
--
--   any agent walks      → walk
--   else any negotiates  → negotiate
--   else (someone voted) → buy
--
-- A VETO BECAUSE THE COSTS ARE NOT SYMMETRIC. A wrong "walk" costs a missed house. A wrong "buy" can cost
-- a default, a repair bill the buyer cannot meet, or both -- unbounded, where the other is bounded. So one
-- agent seeing ruin outranks two seeing a bargain, and a majority vote would be the wrong shape.
--
-- "EVERYONE SAID BUY" CANNOT BE WRITTEN IN HORN, which has no universal in a body. So it is written as
-- what it amounts to when every agent votes exactly once: somebody voted, nobody walked, nobody haggled.
-- The two negations are stratified -- `walked` and `haggled` depend on nothing that depends on them --
-- which is the same shape as `only_child` in the Kinship theory every rule language is first shown as.
theory Adjudicator (Agent, Case)
{
  def walks  (who: Agent, purchase: Case)
  def haggles(who: Agent, purchase: Case)
  def buys   (who: Agent, purchase: Case)

  law vetoed    = ∀ a p, walks(a, p)   ⇒ walked(p)
  law contested = ∀ a p, haggles(a, p) ⇒ haggled(p)
  law heard     = ∀ a p, buys(a, p)    ⇒ voted(p)
  law heard_too = ∀ a p, haggles(a, p) ⇒ voted(p)

  def verdict_walk     (purchase: Case) :- walked(purchase)
  def verdict_negotiate(purchase: Case) :- haggled(purchase) ∧ ¬ walked(purchase)
  def verdict_buy      (purchase: Case) :- voted(purchase) ∧ ¬ walked(purchase) ∧ ¬ haggled(purchase)

  query walk_verdicts(purchase?: Case)      :- verdict_walk(purchase)
  query negotiate_verdicts(purchase?: Case) :- verdict_negotiate(purchase)
  query buy_verdicts(purchase?: Case)       :- verdict_buy(purchase)
}
