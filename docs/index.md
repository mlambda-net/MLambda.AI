---
title: Learn AI by reading real Hilbert
hide:
  - navigation
  - toc
---

# MLambda.AI

<section class="ml-hero">
<div class="ml-hero__grid">
<div>
<p class="ml-eyebrow">MLambda.AI · samples you can read, run and prove</p>
<h2>Artificial intelligence, <span class="ml-gradient">written so you can check it.</span></h2>
<p class="ml-lead">
Logic, agents, machine learning and reinforcement learning, each one real <strong>Hilbert</strong>
source that the build compiles and checks, with a test asserting what it does. Nothing here is
pseudocode, and a proof that stops checking stops the build.
</p>
<div class="ml-actions">
<a class="ml-button ml-button--primary" href="hilbert/01-what-hilbert-is/">Start with Hilbert →</a>
<a class="ml-button ml-button--ghost" href="ideas/">Why it matters</a>
<a class="ml-button ml-button--ghost" href="#three-levels">Pick your level</a>
<a class="ml-button ml-button--ghost" href="https://github.com/mlambda-net/MLambda.AI">Source on GitHub</a>
</div>
<div class="ml-stats">
<div class="ml-stat"><b>3</b><span>levels, novice to advanced</span></div>
<div class="ml-stat"><b>5</b><span>subjects at every level</span></div>
<div class="ml-stat"><b>83</b><span>theorems the kernel checks</span></div>
<div class="ml-stat"><b>305</b><span>tests asserting behaviour</span></div>
</div>
</div>

<div class="ml-window">
<div class="ml-window__bar"><i></i><i></i><i></i>Animals.hs</div>
<div class="highlight"><pre><code>theory Animals (Thing)
{
  def kind_of(narrow: Thing, wide: Thing)
  def known(who: Thing, kind: Thing)

  -- what you are told becomes what is so
  law directly = ∀ x k, known(x, k) ⇒ is_a(x, k)

  -- and classification climbs
  law climbing = ∀ x k w, is_a(x, k) ∧ kind_of(k, w) ⇒ is_a(x, w)

  query kinds(of: Thing, kind?: Thing) :- is_a(of, kind)
}</code></pre></div>
<div class="ml-window__out">
› What is fluffy? &nbsp;<b>cat, animal</b><br>
&nbsp;&nbsp;'animal' is in that list and nobody put it there.<br>
› <span class="ml-ok">Proved</span> &nbsp;a_cat_is_an_animal<br>
› <span class="ml-ok">Proved</span> &nbsp;classification_climbs_twice
</div>
</div>
</div>
</section>

<h2 class="ml-section-title">How to use this site</h2>
<p class="ml-section-lead">Four steps. The first is short and every sample assumes it; the last is what the other three were for.</p>

<div class="ml-path">
<a class="ml-step" style="--c: var(--ml-hilbert)" href="hilbert/01-what-hilbert-is/"><b>Learn Hilbert</b><span>Six short pages: the four dialects, the build, reading a proof.</span></a>
<a class="ml-step" style="--c: var(--ml-l1)" href="L1-novice/"><b>Level 1 · Novice</b><span>Facts, rules, and a machine working out what nobody wrote down.</span></a>
<a class="ml-step" style="--c: var(--ml-l2)" href="L2-practitioner/"><b>Levels 2 and 3</b><span>What the notation buys you, then proving things about the systems.</span></a>
<a class="ml-step" style="--c: var(--ml-capstone)" href="actuarial/"><b>The capstone</b><span>One real house, three agents, one verdict.</span></a>
</div>

<h2 class="ml-section-title">Why it matters</h2>
<p class="ml-section-lead">Four short essays, no code required: what it takes to put a thought into a machine, and why it is not a prompt.</p>

<div class="ml-cards">
<a class="ml-card ml-card--ideas" href="ideas/thinking-machines/">
<p class="ml-card__kicker">Minds</p>
<p class="ml-card__title">Putting a thought into a machine</p>
<p>Why belief, knowledge and intention defeat tables and text, and how possible worlds make them tractable.</p>
<span class="ml-card__more">Read →</span>
</a>
<a class="ml-card ml-card--ideas" href="ideas/modal-systems/">
<p class="ml-card__kicker">Logic</p>
<p class="ml-card__title">The modal systems, explained</p>
<p>K, T, D, S4, S5, KD45: six choices about what an attitude may do, and how to pick them for a real problem.</p>
<span class="ml-card__more">Read →</span>
</a>
<a class="ml-card ml-card--ideas" href="ideas/semantics-and-accountability/">
<p class="ml-card__kicker">Accountability</p>
<p class="ml-card__title">When a chatbot makes a promise</p>
<p>An airline paid for its chatbot's answer. What the answer meant, and the problems semantics solves.</p>
<span class="ml-card__more">Read →</span>
</a>
<a class="ml-card ml-card--ideas" href="ideas/neuro-symbolic/">
<p class="ml-card__kicker">Neuro-symbolic</p>
<p class="ml-card__title">A network that proposes, a logic that checks</p>
<p>Why AlphaGeometry and AlphaProof pair a model with a verifier, and what chain-of-thought cannot give you.</p>
<span class="ml-card__more">Read →</span>
</a>
</div>

<h2 class="ml-section-title" id="three-levels">Three levels</h2>
<p class="ml-section-lead">Every subject appears at each of them. A page is L1 because it is on the L1 shelf, so start where you are and climb.</p>

<div class="ml-cards">
<a class="ml-card ml-card--l1" href="L1-novice/">
<p class="ml-card__kicker">Level 1 · Novice</p>
<p class="ml-card__title">Your first reasoning machine</p>
<p>You have never written a rule for a computer to reason with. You will state facts and rules separately and watch consequences appear.</p>
<ul><li>Animals, Families, Chains</li><li>A thermostat that intends</li><li>A line found from its points</li><li>A bandit that learns</li><li>Traffic lights and duties</li></ul>
<span class="ml-card__more">Start L1 →</span>
</a>
<a class="ml-card ml-card--l2" href="L2-practitioner/">
<p class="ml-card__kicker">Level 2 · Practitioner</p>
<p class="ml-card__title">What the notation buys you</p>
<p>You can read a theory and want to know why it is written that way: modal frames, certificates, agents that commit and give up.</p>
<ul><li>Worlds and Counting</li><li>Collector and Cleaner</li><li>Perceptron and Classifier</li><li>Q-table and Sarsa</li><li>Knowing vs believing</li></ul>
<span class="ml-card__more">Start L2 →</span>
</a>
<a class="ml-card ml-card--l3" href="L3-advanced/">
<p class="ml-card__kicker">Level 3 · Advanced</p>
<p class="ml-card__title">Proving things about the systems</p>
<p>You want guarantees, not only programs: the theory beneath every agent, the law that is missing on purpose, the algebra learners stand on.</p>
<ul><li>Sorts, and the missing law</li><li>Agency: belief, desire, intention</li><li>Nearest, and what is not claimed</li><li>Identities, proved by the kernel</li><li>Deadlines, and who knew</li></ul>
<span class="ml-card__more">Start L3 →</span>
</a>
</div>

<h2 class="ml-section-title">Every subject, at every level</h2>
<p class="ml-section-lead">Read across a row to follow one subject from its first idea to its proofs, or down a column to take a whole level at once.</p>

<div class="ml-matrix-wrap">
<table class="ml-matrix">
<thead>
<tr><th></th><th>L1 · Novice</th><th>L2 · Practitioner</th><th>L3 · Advanced</th></tr>
</thead>
<tbody>
<tr>
<th>Logic<small>theorem proving</small></th>
<td><a href="L1-novice/logic/"><b>Animals, Families, Chains</b><span>a rule works out what nobody wrote down</span></a></td>
<td><a href="L2-practitioner/logic/"><b>Worlds and Counting</b><span>modal frames, arithmetic by certificate</span></a></td>
<td><a href="L3-advanced/logic/"><b>Sorts</b><span>and the law that is missing</span></a></td>
</tr>
<tr>
<th>Minds<small>time, duty, knowledge</small></th>
<td><a href="L1-novice/minds/"><b>Traffic and Duty</b><span>always, eventually, and what ought to be</span></a></td>
<td><a href="L2-practitioner/minds/"><b>Knowledge and Mind</b><span>knowing is not believing</span></a></td>
<td><a href="L3-advanced/minds/"><b>Deadlines</b><span>and what these logics cannot say</span></a></td>
</tr>
<tr>
<th>Agents<small>belief, desire, intention</small></th>
<td><a href="L1-novice/agent/"><b>Thermostat</b><span>a goal that never changes, intentions that do</span></a></td>
<td><a href="L2-practitioner/agent/"><b>Collector and Cleaner</b><span>commitment, attention, giving up</span></a></td>
<td><a href="L3-advanced/agent/"><b>Agency</b><span>the theory underneath every agent</span></a></td>
</tr>
<tr>
<th>Machine learning<small>models in <code>.hb</code></small></th>
<td><a href="L1-novice/ml/"><b>Line</b><span>a model is a formula you can read</span></a></td>
<td><a href="L2-practitioner/ml/"><b>Perceptron and Classifier</b><span>weights, and an instructive failure</span></a></td>
<td><a href="L3-advanced/ml/"><b>Nearest</b><span>and what this project does not claim</span></a></td>
</tr>
<tr>
<th>Reinforcement learning<small>learning from reward</small></th>
<td><a href="L1-novice/learning/"><b>Bandit</b><span>explore or exploit</span></a></td>
<td><a href="L2-practitioner/learning/"><b>Q-table and Sarsa</b><span>two learners, one line of source apart</span></a></td>
<td><a href="L3-advanced/learning/"><b>Identities</b><span>the algebra, proved</span></a></td>
</tr>
</tbody>
</table>
</div>

<h2 class="ml-section-title">One language, four dialects</h2>
<p class="ml-section-lead">One compiler and one token set. Each file extension says a different kind of thing. <a href="hilbert/02-the-four-dialects/">Read the tour →</a></p>

<div class="ml-dialects">
<div class="ml-dialect" style="--c: var(--ml-hilbert)"><code>.hs</code><p>A <strong>theory</strong>: relations, Horn laws, queries. It becomes an engine you assert facts into and ask questions of.</p></div>
<div class="ml-dialect" style="--c: var(--ml-l1)"><code>.hp</code><p><strong>Theorems</strong> over a theory, discharged by <code>proof … qed</code> and checked by a small kernel.</p></div>
<div class="ml-dialect" style="--c: var(--ml-l2)"><code>.ha</code><p>An <strong>agent</strong>: beliefs, desires, intentions, commitment, attention and plans.</p></div>
<div class="ml-dialect" style="--c: var(--ml-l3)"><code>.hb</code><p><strong>Mathematics</strong>: formulas, models and processes over tensors and reals, lowered to code.</p></div>
</div>

<div class="ml-capstone">
<div>
<p class="ml-card__kicker" style="--c: var(--ml-capstone)">The capstone · read after L3</p>
<h3>One house. Three agents. One verdict.</h3>
<p>A buyer looks at a real house from 2 930 Ames sales. A valuation model, a condition model and a credit model each form a belief and an intention, and an adjudicator decides — with a veto, and a reserve that may refuse to exist.</p>
<div class="ml-actions"><a class="ml-button ml-button--primary" href="actuarial/">Open the capstone →</a></div>
</div>
<div class="highlight" data-ml-painted="skip"><pre><code>ValuationAgent   +0.07 spreads over     → Buy
ConditionAgent   repair $17,523         → Negotiate
CreditAgent      default chance 30.40%  → Walk

Adjudicator      Walk. One agent seeing ruin
                 outranks two seeing a bargain.</code></pre></div>
</div>

<p class="ml-note">
<strong>Running the samples yourself.</strong> The Hilbert compilers are not on NuGet yet, so the
repository builds against a sibling checkout of
<a href="https://github.com/mlambda-net/MLambda.Genesis">MLambda.Genesis</a>. Clone both side by side,
then <code>dotnet build MLambda.AI.slnx</code> and <code>dotnet test MLambda.AI.slnx</code>. Every
sample page ends with the command that runs it.
</p>
