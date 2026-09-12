# 4. The Prelude

About forty modules ship beside the compiler and are reachable from a `.hb` with `open`:

```
open Prelude.Regression
```

They are the mathematics you would otherwise rewrite: `Arithmetic`, `Calculus`, `Statistics`,
`Distributions`, `Regression`, `Networks`, `Learning`, `Policies`, `Temporal`, `Decisions`,
`Deliberation`, `Hazards`, `Fields`, `Bayes`, and more.

**We consume these; we do not reimplement them.** A sample that hand-rolls a Gaussian is teaching
you how to hand-roll a Gaussian, which is not the subject.

## When a sample may open one

The rule these samples follow:

- **A novice sample opens only what its subject forces**, and its page says which module and why.
  In logic, that is usually nothing at all — `Animals` opens no module, which is part of why it is
  the first sample.
- In machine learning and reinforcement learning a first sample cannot avoid the Prelude, so it
  opens exactly one module and names it.
- **From L2 onward, open freely.** By then the point is the subject, not the ceremony.

## A note for `.hs` readers

The Prelude is a `.hb` facility — it is mathematics over tensors and reals. A theory (`.hs`) does
not open Prelude modules; it declares its own relations and laws. If you find yourself wanting
arithmetic inside a theory, that is usually a sign the calculation belongs in a `.hb` and the theory
should reason about its result.

Next: [reading a proof](05-reading-a-proof.md).
