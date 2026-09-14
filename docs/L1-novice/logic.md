# Logic, L1 — Animals, Families and Chains

Three samples. Each one adds exactly one idea to the last.

| Sample | The new idea |
|---|---|
| [Animals](#animals) | a rule works out things nobody wrote down |
| [Families](#families) | a rule can use its own conclusions, and can say *not the same one* |
| [Chains](#chains) | the subject can be logic itself |

---

## Animals

**Source:** [`src/MLambda.AI.Logic/Animals.hs`](../../src/MLambda.AI.Logic/Animals.hs) ·
[`src/MLambda.AI.Logic/Animals.hp`](../../src/MLambda.AI.Logic/Animals.hp)

### The idea

Fluffy is a cat. A cat is a kind of animal. So Fluffy is an animal — and notice that nobody said
that last part. You said two things, and a third came out.

That third thing is the whole subject. A program that classifies Fluffy with `if` statements has to
be told about animals somewhere. A theory is told the *rules*, and works out the rest for every
creature anybody mentions afterwards, including ones that did not exist when the rules were written.

### The theory

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

### What the build makes of it

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

### The proof

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

### Run it

```bash
dotnet run --project src/MLambda.AI.Logic -- Animals
```

```
What is fluffy?                            animal, cat
  'animal' is in that list and nobody put it there.
```

### What the tests assert

[`test/MLambda.AI.Logic.Tests/AnimalsTests.cs`](../../test/MLambda.AI.Logic.Tests/AnimalsTests.cs)

- Fluffy is a cat — what you said survives.
- Fluffy is an animal — what you did not say is concluded.
- Fluffy is *exactly* a cat and an animal, and nothing else.
- Tweety is a bird and therefore an animal — but **not** a cat, though a cat is a kind of animal
  too. The rule climbs and never descends.
- A chain of three is walked: fluffy → cat → animal → living thing.
- A fact asserted later is derived over: the engine learns and answers again.
- A creature nobody classified has no kinds at all.
- Every theorem in `Animals.hp` came back `Proved`.

The tweety one is worth a moment. "A cat is a kind of animal" does not make every animal a cat, and
a rule engine that got this wrong would be useless. Nothing in the theory says "do not run
backwards" — it simply never says you may.

**And a word on how that test is written**, because it is a trap worth seeing once. The obvious
version asks whether `"cat"` appears among the kinds of `"animal"`. It does not — but only because
*nothing* appears there: nobody said what an animal is, so the answer is the empty list, and the
test would pass just as happily against an engine that had stopped working entirely. A test that
only looks for an absence proves nothing unless you know the answer was not empty to begin with.
So this one asks about tweety, who *has* kinds, and pins the whole answer set instead of probing
for one gap.

### Try it yourself

1. Add `new KindOfFact("animal", "living_thing")` to the zoo in `Program.cs` and run it again. You
   did not touch the rules, and Fluffy is now a living thing too.
2. Add a second creature — a bird called `tweety` — and ask what it is.
3. Delete the `climbing` law and run the tests. Watch which ones fail, and why.

### The same relation, asked from the other end

`Animals.hs` has a second query, *who are the animals?*:

```
query members(kind: Thing, who?: Thing) :- is_a(who, kind)
```

It reads the same `is_a` as `kinds`, from the other end. Its parameters are in the order you ask the
question, `kind` first, while `is_a` stores the creature first. That is fine: the engine matches a
query's arguments by name. `Members("animal")` answers fluffy and tweety. (It once answered nothing;
see [diagnostics](../hilbert/06-diagnostics.md#a-query-whose-parameters-are-in-a-different-order-than-its-body-fixed).)

---

## Families

**Source:** [`src/MLambda.AI.Logic/Families.hs`](../../src/MLambda.AI.Logic/Families.hs) ·
[`Families.hp`](../../src/MLambda.AI.Logic/Families.hp)

### The idea

```
alice ─┬─ bob ── dave
       └─ carol
```

Three facts are asserted: alice is bob's parent, alice is carol's parent, bob is dave's parent. Ask
who descends from alice and the answer includes **dave** — her grandchild, whom nothing mentioned.

`Animals` climbed a hierarchy. This does something new: a rule that uses **its own conclusions**.

```
law direct   = ∀ a c,   parent(a, c) ⇒ ancestor(a, c)
law indirect = ∀ a b c, parent(a, b) ∧ ancestor(b, c) ⇒ ancestor(a, c)
```

`indirect` needs an `ancestor` in order to conclude an `ancestor`. Having concluded one, it applies
again — and keeps applying until it runs out of parents. **Nothing says how far to go.** There is no
rule about grandchildren, and none about great-grandchildren either: there are two rules, and a
family of any depth.

### And a rule that says *not the same one*

```
law siblings = ∀ p x y, parent(p, x) ∧ parent(p, y) ∧ x ≠ y ⇒ sibling(x, y)
```

Two children of one parent are siblings. Without `x ≠ y` the rule matches bob twice as a child of
alice and concludes that bob is his own sibling. The guard is the whole of the difference, and it is
the first thing in this curriculum a rule can say that a plain hierarchy cannot.

### Run it

```bash
dotnet run --project src/MLambda.AI.Logic -- Families
```

### What the tests assert

[`FamiliesTests.cs`](../../test/MLambda.AI.Logic.Tests/FamiliesTests.cs)

- A parent is an ancestor, and recursion reaches the grandchild.
- Ancestry does not run backwards — dave has no ancestors.
- Two children of one parent are siblings, each way round.
- **And nobody is their own sibling.** Verified by mutation: dropping the guard fails three tests,
  including the one about dave, who becomes his own sibling without it.

### Try it yourself

Add a fourth generation and ask again. You will not need a new rule.

---

## Chains

**Source:** [`src/MLambda.AI.Logic/Chains.hs`](../../src/MLambda.AI.Logic/Chains.hs) ·
[`Chains.hp`](../../src/MLambda.AI.Logic/Chains.hp)

### The idea

`Animals` reasoned about cats and `Families` about people. This reasons about **statements**, which
means the subject is now logic itself.

```
def says(if_this: Claim, then_that: Claim)
def holds(what: Claim)

law ponens = ∀ p q, holds(p) ∧ says(p, q) ⇒ holds(q)
```

`says(p, q)` is the claim *if p then q*. `holds(p)` is *p is true*. The one law is the oldest rule
in logic: if p is true, and p says q, then q is true.

Assert that it is raining, plus three implications:

```
raining → wet_ground → slippery → be_careful
```

**One claim was asserted true. All four come back true.**

### The thing most worth understanding here

`holds` is both **asserted** and **concluded** — unlike `is_a` in `Animals`, which was purely
derived. A host says which claims are true to begin with; the law says which others follow; and both
kinds of fact live in the same relation. That is exactly what makes chaining work, because the
conclusion of one step is the premise of the next.

### An implication is not an assertion

Give the engine all three implications and assert **nothing** true. It concludes nothing. The
implications are all still there — they simply never fire, because none has a true premise to start
from.

That is the confusion this sample exists to prevent, and it has a test of its own.

### Run it

```bash
dotnet run --project src/MLambda.AI.Logic -- Chains
```

### What the tests assert

[`ChainsTests.cs`](../../test/MLambda.AI.Logic.Tests/ChainsTests.cs)

- One assertion, and the rule reaches the end of the chain.
- A chain with no true start concludes nothing.
- The chain does not run backwards — being careful does not make it rain.
- A claim added later is chained from.
- And the proof holds for **any** p, q, r, s — not just for the weather.

That last one is the difference between the two dialects in a single assertion. The engine chained
one particular set of claims; `Chains.hp` proves the pattern for all of them.

### Try it yourself

Assert `slippery` true directly, without asserting `raining`. Which claims follow now, and which do
not — and why?
