# Minds, L2 — Knowledge and Mind

**Knowing is not believing.** That sentence is the whole of this page, and by the end of it you will
have seen the difference three ways: as one missing law, as an engine that catches somebody holding a
false belief, and as a kernel that refuses to prove the difference away.

> The names on this page (T, D, 4, 5, S5, KD45) are each explained, with what they mean for
> knowledge, belief, obligation and time and when you would reject them, in
> [The modal systems, explained](../ideas/modal-systems.md).

| Sample | The new idea |
|---|---|
| [Knowledge](#knowledge) | knowledge is S5, belief is KD45, and the gap between them is one law |
| [Mind](#mind) | a mental state is four attitudes over one set of possible worlds |

---

## Knowledge

**Source:** [`src/MLambda.AI.Minds/Knowledge.hs`](../../src/MLambda.AI.Minds/Knowledge.hs) ·
[`src/MLambda.AI.Minds/Knowledge.hp`](../../src/MLambda.AI.Minds/Knowledge.hp)

### Possible worlds, in one paragraph

You don't know everything, so for you several ways the world could be are still open. Each one is a
**world**. At [Logic L2](logic.md) a world could `see` others. Here each agent gets its own relation,
and there are two:

- `knows(i, w, u)`: at w, agent i **cannot rule u out**. Nothing i knows excludes it.
- `believes(i, w, u)`: at w, agent i **takes u to be how things are**.

"i knows p" then means *p is true in every world i cannot rule out*. "i believes p" means *p is true
in every world i takes seriously*. The two attitudes use the same data. They differ only in the
**laws** their relations obey.

### The one missing law

| | T: reflexive | D: serial | 4: transitive | 5: euclidean | the logic |
|---|---|---|---|---|---|
| **knowledge** | ✔ | (follows) | ✔ | ✔ | **S5** |
| **belief** | **✘** | ✔ | ✔ | ✔ | **KD45** |

Read the T column. For knowledge, **the actual world is never ruled out**: whatever you know is true
where you actually are, because the actual world is one of the worlds it must hold in. For belief,
the actual world **can** be ruled out. You can take yourself to be somewhere you are not. Belief gets
D instead: you can't believe everything, including p and not p at once.

That single ✘ is the whole difference between knowing and believing. Both have 4 and 5, which is
*introspection*: if you know, you know that you know; if you don't know, you know that you don't.

### The scene

At `home`, the door is **unlocked**. A second world, `locked_home`, is the same except the door is
locked.

- **alice** glanced at the door. From where she stands she cannot tell the two worlds apart, and she
  settled on *locked*.
- **bob** tried the handle. Nothing about the door is hidden from him.

```
law reflK   = ∀ i w,     agent(i) ∧ world(w) ⇒ knows(i, w, w)        -- T
law seenK   = ∀ i w u,   glimpse(i, w, u) ⇒ knows(i, w, u)
law transK  = ∀ i w u v, knows(i, w, u) ∧ knows(i, u, v) ⇒ knows(i, w, v)
law euclidK = ∀ i w u v, knows(i, w, u) ∧ knows(i, w, v) ⇒ knows(i, u, v)

law serialB = ∀ i w u,   guess(i, w, u) ⇒ believes(i, w, u)          -- D
law transB  = ∀ i w u v, believes(i, w, u) ∧ believes(i, u, v) ⇒ believes(i, w, v)
law euclidB = ∀ i w u v, believes(i, w, u) ∧ believes(i, w, v) ⇒ believes(i, u, v)

law within  = ∀ i w u,   believes(i, w, u) ⇒ knows(i, w, u)
```

`within` says every world belief takes seriously is one knowledge has not ruled out. In formula
terms, **what you know, you believe**. You can believe more than you know, but never less.

### "Believes that" is a universal

"alice believes the door is locked" means *locked holds in **every** world she believes in*. A rule
engine doesn't have "every", but it has "no counter-example":

```
law doubtB       = ∀ i w u p, believes(i, w, u) ∧ prop(p) ∧ ¬ holds(u, p) ⇒ doubtsB(i, w, p)
law believesThat = ∀ i w p,   agent(i) ∧ world(w) ∧ prop(p) ∧ ¬ doubtsB(i, w, p) ⇒ believes_that(i, w, p)
law falseBelief  = ∀ i w p,   believes_that(i, w, p) ∧ ¬ holds(w, p) ⇒ false_belief(i, w, p)
```

First find a believed world where p fails. If there is none, p is believed. And a belief is **false**
when it doesn't hold where the agent actually is.

### What it answers

```
At home the door is unlocked. alice glanced and settled on 'locked';
bob tried the handle.

  agent   believes    knows       mistaken about
  alice   locked      (nothing)   locked
  bob     unlocked    unlocked    (nothing)
```

| | believes | knows | mistaken about |
|---|---|---|---|
| **alice** | locked | nothing | locked |
| **bob** | unlocked | unlocked | nothing |

alice **believes** the door is locked and does **not know** it, because knowledge would have to hold
at `home` too, and at home the door is unlocked. She doesn't even know it is *un*locked: she can't
rule out `locked_home`. bob's belief and knowledge coincide, because he looked.

Notice what the engine did *not* need to be told: that alice is wrong. It worked that out from what
she believes and where she actually is.

### The proof, and the refusal

`Knowledge.hp` proves the frame conditions that make these attitudes what they are:

| theorem | what it says |
|---|---|
| `knowledge_is_factive` | the actual world is never ruled out: T |
| `knowing_you_know` | positive introspection: 4 |
| `knowing_what_you_do_not_know` | negative introspection: 5 |
| `knowledge_is_symmetric` | derived from T and 5, and nobody gave you that one |
| `a_guess_is_believed`, `believing_you_believe` | D and 4 for belief |
| `what_is_believed_is_within_what_is_known` | what you know, you believe |

A proof file can only say what **proves**. That belief is **not** factive is a statement about a
proof that does not exist, so [`RefusalTests.cs`](../../test/MLambda.AI.Minds.Tests/RefusalTests.cs)
asks the kernel for it, under exactly the laws belief is allowed, and requires a refusal:

| attempt | axioms allowed | verdict |
|---|---|---|
| `∀ i w, agent(i) ⇒ world(w) ⇒ knows(i, w, w)` | `reflK` | **Proved** (the control) |
| `∀ i w, agent(i) ⇒ world(w) ⇒ believes(i, w, w)` | `serialB, transB, euclidB` | **Rejected** |
| the same, borrowing knowledge's `reflK` | `serialB, transB, euclidB, reflK` | **Rejected**: `reflK` concludes `knows`, not `believes` |

**Why the control matters.** A refusal test that can only fail proves nothing: a broken harness
refuses everything. The first row goes through the same harness and must prove, so a refusal in the
second row means *the logic* said no.

> **Why the theorem is about `knows(i, w, w)` and not `K p ⇒ p`.** On exactly the frames where
> `knows(i, w, w)` holds, `K p ⇒ p` is true. That is *correspondence theory*. A proof file can't
> write `K p ⇒ p` directly, because a quantifier inside a premise doesn't parse (`HP0003`), so it
> states the frame condition. The engine carries the formula instead: `Known` never answers a false
> proposition.

### Run it

```bash
dotnet run --project src/MLambda.AI.Minds -- Knowledge
```

### What the tests assert

[`KnowledgeTests.cs`](../../test/MLambda.AI.Minds.Tests/KnowledgeTests.cs)

- alice believes the door is locked, does not know it, and the engine names it a false belief.
- bob, who checked, believes and knows the same thing, and is not mistaken.
- Looking properly is what turns a belief into knowledge.
- All eight theorems proved by the kernel; knowledge's T proves and belief's is refused.

### Try it yourself

1. Give bob a `glimpse` of `locked_home` as well. Predict what bob **knows** now, then what he
   **believes**.
2. Remove `within` from the axioms line in `Knowledge.hp` and build. Which theorem stops checking?
3. Make alice's guess point at `home` instead. Predict all three of her columns.

---

## Mind

**Source:** [`src/MLambda.AI.Minds/Mind.hs`](../../src/MLambda.AI.Minds/Mind.hs) ·
[`src/MLambda.AI.Minds/Mind.hp`](../../src/MLambda.AI.Minds/Mind.hp)

### The idea

An agent does more than know and believe. It **wants** things and it **commits** to things. The BDI
model (belief, desire, intention) is how most agent software is built, and
[Agents, L3](../L3-advanced/agent.md) proves its laws. This sample adds **knowledge** and asks all
four attitudes of one agent at once. The result is a **state of mind**.

| attitude | laws | what they buy |
|---|---|---|
| knows | T, 4, 5 (S5) | what is known is true |
| believes | D, within knowledge | consistent, but may be false |
| desires | D | consistent, and need not be true: that is what wanting is |
| intends | D, and includes desire | **realism**: an agent intends at least what it desires |

Look at *desires*. It has no T, and it mustn't. If desire were factive, you could only want what is
already so. Wanting tea is interesting precisely because tea isn't made.

### The scene

A kitchen robot. **Now** the kettle is on and the cup is dirty.

- It couldn't see the cup, so it can't rule out a clean one, and it **assumed** the cup is clean.
- It **wants** tea: a world with a clean cup and tea made.
- It has **planned** for a clean cup.

### What it answers

```
A kitchen robot. The kettle is on; the cup is dirty, but the robot could not see it.
It wants tea and has planned for a clean cup.

  proposition   knows  believes  desires  intends
  kettle_on     yes    yes       yes      yes
  cup_clean     -      yes       yes      yes
  tea_made      -      -         yes      -
```

Read it row by row.

- **kettle_on** is true in every world the robot can't rule out, so it is **known**, and therefore
  also believed, and part of what it wants and intends.
- **cup_clean** is **believed but not known**, and it is false: the cup is dirty. This is the door
  from the Knowledge sample again, inside a single agent.
- **tea_made** is **desired and nothing else**. Desire isn't factive, and tea isn't made.
- **Intends** holds what is true in *every* world the robot is committed to. Realism commits it to
  the tea world, and the plan commits it to the clean-cup world. A clean cup is in both, and tea is
  only in one. So the robot intends a clean cup and doesn't yet intend the tea itself. Committing to
  more worlds means intending *fewer* things for certain.

### The proof

```
theorem what_you_wish_for_you_intend : ∀ i w u, wish(i, w, u) ⇒ intends(i, w, u)
proof
  intro i w u w1
  have wanted : desires(i, w, u) by apply serialD [w1]
  apply realism [wanted]
qed
```

A wish is desired, and what is desired is intended. The kernel checks both steps. The refusal for
desire works like belief's: `∀ i w, agent(i) ⇒ world(w) ⇒ desires(i, w, w)` under desire's own laws
comes back **Rejected**.

### Run it

```bash
dotnet run --project src/MLambda.AI.Minds -- Mind
```

### What the tests assert

[`MindTests.cs`](../../test/MLambda.AI.Minds.Tests/MindTests.cs)

- The robot knows only what holds in every world it can't rule out.
- It believes more than it knows, including something false.
- It desires what is not yet so.
- It intends only what holds in every world it is committed to, so a clean cup and not yet tea.
- Five theorems proved; desire's T refused.

### Try it yourself

1. Add a plan for the `tea` world too. Predict the *intends* column, then run it.
2. Remove the wish. What does the robot desire, and does *intends* change? Why?
3. Let the robot see the cup: drop the glimpse and the guess, and guess `now` instead. Which row
   changes, and in how many columns?

Next: [Minds, L3 — Deadlines, and what these logics cannot say](../L3-advanced/minds.md).
