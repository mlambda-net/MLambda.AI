# 2. The four dialects

One compiler, one token set, four file extensions. Each says a different *kind* of thing.

| Extension | What it holds | Compiled by |
|---|---|---|
| `.hb` | formulas, `model`s, `process`es, `fn`s, `law`s over tensors and reals | `MLambda.Hilbert.targets` |
| `.hs` | a `theory`: `sort`s, relations, Horn `law`s, `modality` declarations | `MLambda.Hilbert.Shin.targets` |
| `.ha` | an `agent`: beliefs, desires, intentions, commitment, attention, plans | `MLambda.Hilbert.Shin.targets` |
| `.hp` | `theorem`s over a theory, discharged by `proof … qed` | `MLambda.Hilbert.Proof.targets` |

A project imports only the targets for the dialects it uses.

## `.hs` — a theory

What relations exist, and what follows from what. This is
[`Animals.hs`](../../src/MLambda.AI.Logic/Animals.hs), the sample in this repository:

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

It becomes an **engine**: something you assert facts into and ask questions of.

## `.hp` — proofs about a theory

The engine answers about the facts you gave it. A `.hp` proves things that hold whatever facts
anybody gives it. From [`Animals.hp`](../../src/MLambda.AI.Logic/Animals.hp):

```
theorem a_cat_is_an_animal : ∀ x c a, known(x, c) ⇒ kind_of(c, a) ⇒ is_a(x, a)
proof
  intro x c a said wider
  have surely : is_a(x, c) by apply directly [said]
  apply climbing [surely, wider]
qed
```

One theory, two dialects reading it, and neither holds a copy of the other's declarations.
See [reading a proof](05-reading-a-proof.md).

## `.hb` — formulas and models

Mathematics: a function, a neural layer, a stochastic process. From
[`Perceptron.hb`](../../src/MLambda.AI.ML/Perceptron.hb):

```
model Perceptron(inputs, hidden, seed) : Learner {
  weight first  : Tensor[inputs, hidden] from gaussian(seed) · 0.5
  weight second : Tensor[hidden, 1]      from gaussian(seed) · 0.5

  def train(x, y, rate) {
    first  ≔ stepFirst(rate, x, y, first, second)
    second ≔ stepSecond(rate, x, y, first, second)
  }

  def eval(x) ≔ outputs(x, first, second)
}
```

**One `fn` becomes many methods.** The compiler emits each formula once for `double`, once for
complex numbers, once for every tensor rank with scalar broadcasting, and once more as an **ONNX
graph** an executor can run. You write the formula once.

Hilbert's own sample project states the rule worth adopting: *"if a line here looks like
mathematics, it is in the wrong file"* — meaning the mathematics belongs in `.hb`, not in the C#
that calls it.

## `.ha` — an agent

Belief, desire and intention as **first-class keywords**, not something hand-rolled in `.hs`.
Arriving with `MLambda.AI.Agent`. The shape:

```
agent Counter(id: String) : Agency {
  belief count(value: Int)
  desire Three reachable by [Bump]
  commit single_minded
  attention Counting initially
  intend Bump for Three when B(self) count(seen) ∧ seen < 3
  achieved Three when B(self) count(3)
  plan Bump ↦ Sample.Bump
}
```

`plan Bump ↦ Sample.Bump` names a C# method — a plan has to *act*, so that is the one place in an
agent project where ordinary code belongs.

Next: [how the build works](03-how-the-build-works.md).
