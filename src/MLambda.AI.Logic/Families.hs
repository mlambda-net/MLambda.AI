-- Families.hs — the expert system every rule language is first shown as.
--
-- WHAT IS NEW SINCE `Animals`. `ancestor` composes `parent` with ITSELF rather than climbing a
-- second relation, so the recursion is visible in one line. And `sibling` needs something no rule
-- so far has needed: a way to say two things are NOT the same one.
--
-- `person` and `parent` are PRIMITIVE — nothing derives them, so a host asserts them. Everything
-- else is concluded.
theory Families (Person)
{
  def person(id: Person)
  def parent(of: Person, child: Person)

  -- A parent is an ancestor, and an ancestor's ancestor is one too. The second law is recursive
  -- through `ancestor` positively, which is an ordinary fixpoint.
  law direct   = ∀ a c,   parent(a, c) ⇒ ancestor(a, c)
  law indirect = ∀ a b c, parent(a, b) ∧ ancestor(b, c) ⇒ ancestor(a, c)

  -- TWO CHILDREN OF ONE PARENT, AND NOT THE SAME CHILD. `x ≠ y` is the guard, and without it
  -- everybody is their own sibling.
  law siblings = ∀ p x y, parent(p, x) ∧ parent(p, y) ∧ x ≠ y ⇒ sibling(x, y)

  -- INPUTS FIRST, THE `?` OUTPUT LAST, AND THE BODY ATOM IN THE SAME ORDER. Getting that backwards
  -- gives a query that compiles and answers nothing -- see docs/hilbert/06-diagnostics.md.
  query ancestors(of: Person, descendant?: Person) :- ancestor(of, descendant)
  query siblings_of(who: Person, other?: Person)   :- sibling(who, other)
}
