# Putting a thought into a machine

Most software stores **facts**: a customer's address, a flight's departure time. People don't work
from facts alone. They work from **what they believe, what they know, what they want, what they
intend and what they are obliged to do**, and those things disagree with the facts all the time.

A doctor believes a patient is allergic. A customer was told a refund is possible. A robot assumed a
cup was clean. A contract obliges a payment by Friday. None of these is a fact about the world; each
is a fact about somebody's **mind**, or about a rulebook, and each changes what should happen next.

This page is about why putting that kind of thought into a machine is hard, what goes wrong when it
is done naively, and the idea (possible worlds and modal logic) that makes it tractable. It is not a
how-to. The how-to is [Minds](../L1-novice/minds.md). This is the *why*.

---

## What a mental state is

Philosophers call belief, knowledge, desire and intention **propositional attitudes**: an attitude
someone takes toward a proposition.

| someone | attitude | proposition |
|---|---|---|
| alice | **believes** that | the door is locked |
| bob | **knows** that | the door is unlocked |
| the robot | **wants** that | tea is made |
| ana | **is obliged** to see that | the office is locked |
| the light | will **eventually** show that | it is green |

The proposition is the same kind of thing a database stores. What is new is the **attitude**: a word
that changes how the proposition behaves. That is what a machine has to learn to handle.

---

## Why the obvious representations fail

### A row in a table

The first idea is to store the attitude as data:

```
believes(alice, door_locked)
knows(bob, door_unlocked)
```

This records what somebody said. It supports no **reasoning** about it:

- **Consequence.** If alice believes the door is locked, and believes that a locked door means the
  house is safe, does she believe the house is safe? The table doesn't know. Somebody has to write
  that rule, for every attitude and every rule of inference, by hand.
- **Truth.** Nothing distinguishes `knows` from `believes`. But knowing something *false* is
  impossible, and believing something false happens every day. A table will happily store
  `knows(alice, door_locked)` while the door stands open.
- **Consistency.** Nothing stops `believes(alice, locked)` and `believes(alice, not locked)` from
  sitting side by side.
- **Nesting.** *alice believes that bob knows the door is unlocked.* Now the proposition is itself an
  attitude. Tables of flat facts don't nest.

### A string of text

The second idea, and the fashionable one, is to hand the whole situation to a language model and ask.
A language model is superb at producing text *about* minds. But it keeps no **committed state**.
Ask the same question two ways and you can get two answers; nothing forces the second to agree with
the first, and nothing forces an answer to agree with what the model was told earlier. The model
predicts plausible words; it doesn't maintain a set of beliefs that must hang together. There's more
on this in [Neuro-symbolic AI](neuro-symbolic.md).

### A special case per attitude

The third idea is to write code: a `BeliefStore`, a `KnowledgeStore`, an `ObligationEngine`, each with
its own rules. That works until the attitudes interact. *Knowledge implies belief. You are obliged to
act on what you know. What you intend should be something you believe possible. What was known on
Tuesday is still known on Thursday.* Every interaction is another special case, and the special cases
contradict each other in ways nobody can see.

---

## The idea that works: possible worlds

In 1962 Jaakko Hintikka proposed a way to treat all of these attitudes the same way. Saul Kripke gave
it its mathematics shortly after.

**You don't know everything.** So, for you, several ways the world could be are still open. Each is a
**possible world**. You have ruled some worlds out, and others you can't rule out.

Then:

> **You know p exactly when p is true in every world you cannot rule out.**

Nothing more is needed. Look at what follows automatically:

- **Consequence comes for free.** If p is true in every world you can't rule out, and so is "p implies
  q", then q is true in all of them too. So you know q. Nobody wrote a rule for that.
- **Belief is the same shape with different worlds.** You *believe* p when p is true in every world
  you take to be how things are. The data is the same; only the *set of worlds* differs.
- **Nesting is ordinary.** "alice believes that bob knows p" means: in every world alice believes in,
  p holds in every world bob can't rule out. Worlds inside worlds, no new machinery.
- **The attitudes differ by one property of the worlds.** This is the part that surprises people, and
  it is where the real modelling happens.

### The whole difference between knowing and believing

Is the **actual** world always one of the worlds you can't rule out?

- For **knowledge**, yes. You cannot rule out the world you are actually in. So anything you know is
  true where you actually are: **knowledge is factive**.
- For **belief**, no. You can take yourself to be somewhere you are not. So a belief can be false.

In logic this is one property, *reflexivity*, present or absent. It is the only thing separating a
machine that can be **wrong** from one that cannot be wrong. [Minds, L2](../L2-practitioner/minds.md)
runs exactly this: alice believes the door is locked, the engine works out that she doesn't know it,
and it names the belief false.

Every other choice works the same way. Can you be wrong about what you *want*? Should you know that
you know? Is what ought to be ever simply what is? Each question is a property of the worlds, and
[the modal systems page](modal-systems.md) goes through them one by one.

---

## What this makes a machine able to do

Once attitudes are modal operators over possible worlds, a machine can:

| capability | what it means in practice |
|---|---|
| **Hold a belief distinct from the truth** | a system can represent that a customer *believes* they are entitled to a refund, separately from whether they *are* |
| **Detect a false belief** | a triage assistant notices that a patient believes a medication is safe when the record says otherwise, and says so |
| **Reason about another mind** | "the driver can't have seen the cyclist, so the driver doesn't know the cyclist is there": the basis of theory of mind, negotiation and teaching |
| **Separate ought from is** | a compliance system represents that a payment is *due* without pretending it has been *paid* |
| **Reason over time** | "the licence was valid when the contract was signed", "the patient must eventually be reviewed" |
| **Explain itself** | every conclusion rests on named laws and asserted facts, so "why do you think she knew?" has an answer a person can check |
| **Be held to account** | what the system committed to, and on what grounds, is written down rather than implied by a paragraph of text |

---

## How this helps people think

The most useful effect is on the **people** writing the rules, not the machine.

To put a thought into a modal logic you must decide what kind of thought it is. That forces questions
a design meeting usually skips:

- *Is this something the system knows, or something it was told?* That is the difference between S5
  and KD45, and between an audit that holds and one that doesn't.
- *If the rule says the customer may do this, does that mean they will?* No: a permission is not an
  event, and writing it as one is a bug.
- *Do we assume our agents never forget?* Then say so, as a law, so the day it stops being true a
  proof stops checking.
- *When two policies disagree, which is wrong?* A deontic model finds the obligation and prohibition
  that clash; a pile of `if` statements hides it.

Disagreements then stop being about wording and become about a **specific property**: should
negative introspection hold for this agent? Should this obligation be consistent? That is a question
a room of people can actually settle.

---

## What it does not do, honestly

- **Logical omniscience.** If knowledge is truth in all possible worlds, an agent knows every
  consequence of what it knows, including every theorem of arithmetic. Real people don't. Modal
  logic models an *idealised* reasoner. That's the right model for what a system *is entitled to
  conclude*, and the wrong model for what a person *has noticed*.
- **Where the worlds come from.** The logic tells you what follows once you have said which worlds an
  agent can't rule out. Deciding that from a messy conversation, image or sensor is **perception**,
  and it is exactly where neural networks are strong. That combination is the subject of
  [Neuro-symbolic AI](neuro-symbolic.md).
- **Existence and absence.** A rule engine can't conclude that something exists (every serial
  relation here is seeded). A conclusion drawn from something being *absent* is provisional: learning
  more can make it false, so the reasoner must be able to take it back. [Minds, L3](../L3-advanced/minds.md)
  lists these edges.

---

## Read next

- [The modal systems, explained](modal-systems.md): K, T, D, 4, 5, B, and the logics they make.
- [When a chatbot makes a promise](semantics-and-accountability.md): why this matters in court.
- [Minds, L1](../L1-novice/minds.md): the same ideas, running.

**Sources.** Jaakko Hintikka, *Knowledge and Belief* (1962). Saul Kripke, "Semantical
Considerations on Modal Logic" (1963). Ronald Fagin, Joseph Halpern, Yoram Moses and Moshe Vardi,
*Reasoning About Knowledge* (MIT Press, 1995). Anand Rao and Michael Georgeff, "BDI Agents: From
Theory to Practice" (1995).
