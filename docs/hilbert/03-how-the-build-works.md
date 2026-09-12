# 3. How the build works

Hilbert is not a separate step you remember to run. It is MSBuild targets, so `dotnet build` does
everything.

## Importing the targets

A project imports the targets for the dialects it uses. From
[`MLambda.AI.Logic.csproj`](../../src/MLambda.AI.Logic/MLambda.AI.Logic.csproj):

```xml
<Import Project="$(HilbertRoot)\src\MLambda.Hilbert.Compiler\Shin\MLambda.Hilbert.Shin.targets" />
<Import Project="$(HilbertRoot)\src\MLambda.Hilbert.Compiler\Proof\MLambda.Hilbert.Proof.targets" />
```

**There is no list of source files.** The targets glob every `.hs`, `.ha`, `.hb` and `.hp` under the
project directory. Adding a sample means adding a file.

## What comes out

| Dialect | Lands in |
|---|---|
| `.hb` | `Generated/Hilbert/<File>.g.cs` |
| `.hs` / `.ha` | `Generated/Shin/<File>Engine.g.cs` |
| `.hp` | `Generated/Proof/<File>Proofs.g.cs` |

`Generated/` is in `.gitignore`. It is an output, rebuilt every time — never edit it, never commit
it. Reading it is a good idea, though: it is how you learn what names your theory produced.

## Two properties that matter

**`RootNamespace` is required.** The targets raise an error without it, because they have to put the
generated code somewhere. A theory called `Animals` in a project whose `RootNamespace` is
`MLambda.AI.Logic` generates into `MLambda.AI.Logic.Animals`.

**`HilbertProofStrict` decides whether an unfinished proof is allowed.**

```xml
<HilbertProofStrict>true</HilbertProofStrict>
```

With it on, `sorry` is `HP0002` and the build fails. This repository turns it on everywhere. A
sample lands complete or not at all.

## The names you get are not always the names you wrote

The generator singularises. A theory called `Animals` produces `IAnimalEngine` and
`AnimalEngineFactory` — **singular**. Upstream, a theory called `Requirements` produces
`IRequirementEngine` the same way.

So: build first, read `Generated/`, then write the C# that calls it. Guessing the names costs more
time than looking.

## Where the compilers come from

They are not on NuGet yet, so this repository reaches them by relative path into a sibling
MLambda.Genesis checkout. Every path lives in one file, `Directory.Build.props`, as two properties:

```xml
<GenesisRoot>$(MSBuildThisFileDirectory)..\MLambda.Genesis</GenesisRoot>
<HilbertRoot>$(GenesisRoot)\modules\MLambda.Hilbert</HilbertRoot>
```

**Two facts worth knowing**, because neither is guessable:

1. **Not everything you need is under `$(HilbertRoot)`.** Lowering a `.hs` produces code over
   `MLambda.Shin.Runtime`, and every generated engine opens with
   `using static MLambda.Church.Semantic.AstHelpers;` even though nothing you wrote mentions it.
   Both live elsewhere in Genesis and are reached through `$(GenesisRoot)`. That is why the seam has
   two properties and not one.
2. **A theory that proves is not always a theory that runs.** Lowering a theory to an engine is
   stricter than checking proofs about it: an axiom schema is fine to reason about but hits
   `HS0020` when lowered. Upstream's proof corpus therefore imports *only* the Proof targets.
   `Animals` is written as a plain Horn theory so it can do both.

If the Genesis checkout is missing, the build stops with one sentence saying so, rather than a wall
of missing-project errors.

Next: [the Prelude](04-the-prelude.md).
