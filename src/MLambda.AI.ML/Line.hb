-- Line.hb — fitting a straight line, and watching the error fall.
--
-- THE FIRST `.hb` IN THIS REPOSITORY. Everything before this was `.hs` and `.hp` — theories and
-- proofs, which reason about relations. This dialect is the other half: mathematics over tensors
-- and reals, where a formula IS a computation.
fn slopeOf(rise, run) ↦ rise / run

fn errorOf(predicted, actual) ↦ (predicted − actual) · (predicted − actual)
