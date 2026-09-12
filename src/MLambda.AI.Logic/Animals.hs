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

  -- ONE QUERY, AND THE REVERSE ONE IS ABSENT ON PURPOSE. `members(kind, who?)` -- "who are the
  -- animals?" -- is the obvious companion, and it does not work today: the lowering takes the
  -- generated method's parameters in DECLARATION order but builds the goal in BODY-ATOM order, so
  -- `:- is_a(who, kind)` under a `(kind, who?)` declaration silently inverts and answers nothing.
  -- Writing round it needs a reversed relation, which is a workaround, and a first sample is the
  -- wrong place to teach one.
  query kinds(of: Thing, kind?: Thing) :- is_a(of, kind)
}
