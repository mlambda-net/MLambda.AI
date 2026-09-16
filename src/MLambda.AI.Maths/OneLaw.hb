-- OneLaw.hb — a computer algebra system, in three lines.
--
-- THIS FILE OPENS NOTHING. Not `Prelude.Arithmetic`, not anything. What its `Compute` can do is
-- exactly what is written below and not one step more, because a file's CAS fires the laws that
-- file declares or opens:
--
--     R(F) = own(F) ∪ ⋃ R(o) for every o the file opens
--
-- SO THE SCOPE IS PER FILE, NOT PER PROJECT. `Algebra.hb` sits beside this one and knows ten
-- arithmetic laws and five trigonometric ones. This file knows one. They are different computer
-- algebra systems in the same assembly, and neither can borrow from the other.
model OneLaw {
  law only_unit = x · 1 = x
}
