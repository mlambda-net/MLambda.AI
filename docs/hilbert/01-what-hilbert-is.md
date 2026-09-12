# 1. What Hilbert is

Hilbert is a small family of languages for writing down *reasoning* — rules, models, agents and
proofs — and having a compiler check it.

## Why not just write a program?

Suppose you want a program that knows Fluffy is an animal. In an ordinary language you write
something like:

```csharp
if (creature.Species == "cat") { kinds.Add("animal"); }
```

That works, and it is already wrong in a way you cannot see. The knowledge *a cat is a kind of
animal* is now spelled out in a branch, mixed with the knowledge *Fluffy is a cat*, and mixed again
with the decision to look only one step up. Add birds, add mammals, add a hierarchy five deep, and
the rules and the facts are tangled past separating.

In Hilbert you say the two things separately:

```
law directly = ∀ x k, known(x, k) ⇒ is_a(x, k)
law climbing = ∀ x k w, is_a(x, k) ∧ kind_of(k, w) ⇒ is_a(x, w)
```

and then tell it that Fluffy is a cat. Nobody writes the answer down. The engine works it out, for
every creature anybody mentions afterwards — including ones that did not exist when the rules were
written.

## And why a *checked* language?

Because rules that are merely written can quietly stop being true.

Hilbert lets you state a claim about your rules and prove it, and the **build** runs the proof
checker. A theorem that stops checking is a compile error, exactly like a syntax mistake. There is
no way to leave a proof half-finished and forget: `sorry`, the "I will do this later" marker, is
rejected outright in this repository.

So the claim this repository makes is: **if it compiles, the reasoning was checked.** Not reviewed,
not tested on the cases somebody thought of — checked.

## What you write, and what you get

You write Hilbert source files. The build compiles them into ordinary C# beside the source, under
`Generated/`, and your program calls that C# like any other class. You never edit generated code,
and it is not in version control — it is rebuilt every time.

Next: [the four dialects](02-the-four-dialects.md).
