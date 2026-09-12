-- Counting.hs — almost nothing, on purpose.
--
-- `ring` DECIDES AN IDENTITY FROM THE POLYNOMIAL ALGEBRA ALONE and `linarith` reads the hypotheses
-- in scope; neither consults a law. So this declares the sort the proofs quantify over and one
-- predicate to hang a name on, and every claim in `Counting.hp` is carried by its own certificate.
--
-- A THEORY WITH NO LAWS IS A LEGITIMATE THEORY. It says what there is and nothing about what
-- follows -- which here is exactly right, because what follows is DECIDED rather than assumed.
theory Counting (Double)
{
  def counted(value: Double)
}
