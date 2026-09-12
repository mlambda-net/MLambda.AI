# Logic, L1 — Animals

**Source:** [`src/MLambda.AI.Logic/Animals.hs`](../../src/MLambda.AI.Logic/Animals.hs) ·
[`src/MLambda.AI.Logic/Animals.hp`](../../src/MLambda.AI.Logic/Animals.hp)

## The idea

Fluffy is a cat. A cat is a kind of animal. So Fluffy is an animal — and notice that nobody said
that last part. You said two things, and a third came out.

That third thing is the whole subject. A program that classifies Fluffy with `if` statements has to
be told about animals somewhere. A theory is told the *rules*, and works out the rest for every
creature anybody mentions afterwards, including ones that did not exist when the rules were written.

## The theory

```
theory Animals (Thing)
{
  def thing(id: Thing)
  def kind_of(narrow: Thing, wide: Thing)
  def known(who: Thing, kind: Thing)

  law directly = ∀ x k, known(x, k) ⇒ is_a(x, k)
  law climbing = ∀ x k w, is_a(x, k) ∧ kind_of(k, w) ⇒ is_a(x, w)

  query kinds(of: Thing, kind?: Thing) :- is_a(of, kind)
}
```

Read `∀ x k` as "for any x and any k", `⇒` as "then", and `∧` as "and".

So **`directly`** says: for any x and k, if x is known to be a k, then x is a k. And **`climbing`**
says: if x is a k, and k is a kind of w, then x is a w.

`thing`, `kind_of` and `known` are things you *tell* the engine. `is_a` is what it *works out* —
which is why nothing asserts it. Asserting `is_a` would be stating as a premise the very thing this
sample is about.

**`climbing` mentions `is_a` on both sides.** That is deliberate, and it is the one idea here worth
sitting with. Having climbed one step, the rule applies again to its own conclusion, and keeps
applying until it runs out of kinds. Nothing says how far to go. Three levels or thirty, the rule is
the same two lines.

The `?` in `kind?: Thing` marks an **output**. `kinds("fluffy")` is not a yes-or-no question about a
pair you already know — it hands back the answers, which is why the program reads it with
`await foreach`.

## What the build makes of it

`Animals.hs` becomes an engine. Note the **singular** names — a theory called `Animals` generates
`IAnimalEngine`, not `IAnimalsEngine`:

```csharp
var zoo = AnimalEngineFactory.Create();
zoo.AssertAll([
    new ThingFact("fluffy"), new ThingFact("cat"), new ThingFact("animal"),
    new KindOfFact("cat", "animal"),
    new KnownFact("fluffy", "cat"),
]);
```

One fact record per relation you declared, and the query as a method. You can read the generated
code — it is under `src/MLambda.AI.Logic/Generated/Shin/` after a build.

## The proof

The engine answers about the creatures you mention. `Animals.hp` proves things that hold whatever
anybody mentions:

```
theorem a_cat_is_an_animal : ∀ x c a, known(x, c) ⇒ kind_of(c, a) ⇒ is_a(x, a)
proof
  intro x c a said wider
  have surely : is_a(x, c) by apply directly [said]
  apply climbing [surely, wider]
qed
```

`intro` names the things you are given. `have … by` takes one step and names the result. `apply`
uses a law, handing it its premises in the order the law asks for them. See
[reading a proof](../hilbert/05-reading-a-proof.md).

**The build checks this.** A theorem that stops checking fails the build before any test runs, so
the program can print its verdicts without running a prover. Try it: change `auto` in the last
theorem to `sorry` and build. You get `HP0002`, and no program.

## Run it

```bash
dotnet run --project src/MLambda.AI.Logic
```

```
What is fluffy?      cat animal
  'animal' is in that list and nobody put it there.
```

## What the tests assert

[`test/MLambda.AI.Logic.Tests/AnimalsTests.cs`](../../test/MLambda.AI.Logic.Tests/AnimalsTests.cs)

- Fluffy is a cat — what you said survives.
- Fluffy is an animal — what you did not say is concluded.
- An animal is **not** a cat: the rule climbs, it does not descend.
- A chain of three is walked: fluffy → cat → animal → living thing.
- A fact asserted later is derived over: the engine learns and answers again.
- Every theorem in `Animals.hp` came back `Proved`.

That third one is worth a moment. "Every cat is an animal" does not make every animal a cat, and a
rule engine that got this wrong would be useless. Nothing in the theory says "do not run backwards"
— it simply never says you may.

## Try it yourself

1. Add `new KindOfFact("animal", "living_thing")` to the zoo in `Program.cs` and run it again. You
   did not touch the rules, and Fluffy is now a living thing too.
2. Add a second creature — a bird called `tweety` — and ask what it is.
3. Delete the `climbing` law and run the tests. Watch which ones fail, and why.

## A missing piece, on purpose

The obvious companion query — *who are the animals?* — is commented out in `Animals.hs`. It does not
work today, for a reason in the compiler rather than in the theory: see
[diagnostics](../hilbert/06-diagnostics.md#a-query-that-answers-nothing). Working round it needs a
reversed relation, and a first sample is the wrong place to teach a workaround.
