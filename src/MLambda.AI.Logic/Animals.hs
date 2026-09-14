-- Animals.hs — a thing belongs to the kinds its kinds belong to.
--
-- WHAT A READER SHOULD NOTICE. Nothing here is written about Fluffy. The theory says two things —
-- what you are told, you are; and being a cat while a cat is a kind of animal makes you an animal —
-- and the engine works out the rest for any creature anybody mentions later.
--
-- `thing`, `kind_of` and `known` are PRIMITIVE: nothing derives them, so a host asserts them.
-- `is_a` is concluded, and asserting it would be stating as a premise the very thing this is about.
theory Animals (Thing)
{
  def thing(id: Thing)
  def kind_of(narrow: Thing, wide: Thing)
  def known(who: Thing, kind: Thing)

  -- WHAT YOU ARE TOLD, YOU ARE.
  law directly = ∀ x k, known(x, k) ⇒ is_a(x, k)

  -- AND CLASSIFICATION CLIMBS. This law is recursive through `is_a` positively — an ordinary
  -- fixpoint — so it climbs as far as the kinds go without anything saying how far that is.
  law climbing = ∀ x k w, is_a(x, k) ∧ kind_of(k, w) ⇒ is_a(x, w)

  -- ONE RELATION, ASKED FROM BOTH ENDS. "What is fluffy?" and "who are the animals?" read the same
  -- `is_a`, and `members` declares its parameters in the order the question is asked, not the order
  -- `is_a` stores them in. The engine binds a query's arguments by NAME, so the two may differ.
  query kinds(of: Thing, kind?: Thing)     :- is_a(of, kind)
  query members(kind: Thing, who?: Thing)  :- is_a(who, kind)
}
