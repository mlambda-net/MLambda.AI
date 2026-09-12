# MLambda.AI

Samples of AI written in the Hilbert language family — agents, machine learning, reinforcement
learning, theorem proving — with documentation explaining how each one works.

Every sample is real Hilbert source that the build compiles and checks, with a test asserting what
it does. Nothing here is pseudocode, and a proof that stops checking stops the build.

## Before you build: you need MLambda.Genesis

The Hilbert compilers are not on NuGet yet, so this repository reaches them by relative path.
**Check out MLambda.Genesis beside this repository:**

```
D:\Workspace\
  MLambda.Genesis\    <- the compilers live here
  MLamba.AI\          <- you are here
```

```bash
git clone --recurse-submodules https://github.com/mlambda-net/MLambda.Genesis.git
```

If it lives somewhere else, pass the path: `dotnet build -p:GenesisRoot=/path/to/MLambda.Genesis`.

`Directory.Build.props` is the only file that knows about any of this. When Hilbert ships as a NuGet
package, that file and one `ItemGroup` per project are the whole migration.

## Build and run

```bash
dotnet build MLambda.AI.slnx
dotnet test MLambda.AI.slnx
dotnet run --project src/MLambda.AI.Logic
```

`HilbertProofStrict` is on, so a theorem that stops checking fails the build and `sorry` is not
available.

What the last command prints:

```
What is fluffy?      cat animal
  'animal' is in that list and nobody put it there.

And what the build checked before this program was allowed to run:
  Proved   what_is_known_is_so
           ∀ x k, known(x, k) ⇒ is_a(x, k)
  ...
```

## The projects

| Project | Subject | Dialects |
|---|---|---|
| [`MLambda.AI.Logic`](src/MLambda.AI.Logic/) | theorem proving: arithmetic, modal and sortal logic | `.hs` `.hp` |
| `MLambda.AI.Agent` | BDI agents | `.ha` `.hs` `.hp` |
| `MLambda.AI.ML` | machine learning | `.hb` |
| `MLambda.AI.Learning` | reinforcement learning | `.hb` `.hp` |
| `MLambda.AI.Actuarial` | house-purchase risk, from `houses.csv` | all four |

`MLambda.AI.Logic` exists today, with one sample in it; the rest arrive in later plans.

## Where to start

- New to all of it: [docs/hilbert/01-what-hilbert-is.md](docs/hilbert/01-what-hilbert-is.md)
- Ready for a sample: [docs/L1-novice/logic.md](docs/L1-novice/logic.md)
