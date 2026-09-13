-- Identities.hs — almost nothing, for the same reason `Counting.hs` is almost nothing.
--
-- `ring` DECIDES AN IDENTITY FROM THE POLYNOMIAL ALGEBRA ALONE and `linarith` reads the hypotheses
-- in scope; neither consults a law. So this declares the sort the corpus quantifies over and one
-- predicate to hang a name on, and every claim in `Identities.hp` carries its own certificate.
--
-- NO ENGINE COMES FROM THIS, AND NONE IS WANTED. This project imports the Proof targets and not the
-- Shin ones, so the theory is parsed for its proofs and never lowered -- which is why `Double` is
-- fine here and needed a `HilbertShinFile Remove` in MLambda.AI.Logic.
theory Identities (Double)
{
  def valued(q: Double)
}
