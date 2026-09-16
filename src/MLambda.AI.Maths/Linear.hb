-- Linear.hb — one contraction underneath, and two words spelled solve.
--
-- `einsum` IS THE PRIMITIVE, AND THE PRELUDE ADMITS IT. Matrix product, transpose, inner product,
-- outer product and the diagonal are all the same contraction with different indices:
--
--     def mul(a, b)    ≔ einsum("ij,jk->ik", a, b)
--     def transpose(a) ≔ einsum("ij->ji", a)
--     def dot(x, y)    ≔ einsum("i,i->", x, y)
--
-- So `product` and `contracted` below are the same function written twice, and a test says so.
-- Naming a contraction is the whole of what "matrix multiplication" means here.
--
-- AND `solve` HERE IS NUMERIC. `Prelude.LinearAlgebra`'s `solve(a, b)` is conjugate gradient over
-- a real matrix: it takes numbers and returns numbers. The symbolic `Solve` of Solve.hb takes an
-- equation and a variable and returns an expression. Two different operations, one English word,
-- kept apart by which dialect you are in.
open Prelude.LinearAlgebra

-- ── the contraction, named and unnamed ─────────────────────────────────────────────────────────

fn product(a, b)    ↦ mul(a, b)
fn contracted(a, b) ↦ einsum("ij,jk->ik", a, b)

-- ── and the numeric solve ──────────────────────────────────────────────────────────────────────

fn solved(a, b) ↦ solve(a, b)
