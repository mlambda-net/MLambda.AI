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
  MLambda.AI\         <- you are here
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
dotnet run --project src/MLambda.AI.Logic     # list the logic samples
dotnet run --project src/MLambda.AI.Agent     # list the agents
dotnet run --project src/MLambda.AI.ML        # list the models
dotnet run --project src/MLambda.AI.Learning  # list the learners
dotnet run --project src/MLambda.AI.Minds     # time, duty, knowledge, belief
dotnet run --project src/MLambda.AI.Actuarial # one house, three agents, one verdict
dotnet run --project src/MLambda.AI.Airline   # a chat that cannot promise what policy does not grant
```

`HilbertProofStrict` is on, so a theorem that stops checking fails the build and `sorry` is not
available.

What a logic sample prints:

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
| [`MLambda.AI.Agent`](src/MLambda.AI.Agent/) | BDI agents | `.ha` `.hs` `.hp` |
| [`MLambda.AI.ML`](src/MLambda.AI.ML/) | machine learning | `.hb` |
| [`MLambda.AI.Learning`](src/MLambda.AI.Learning/) | reinforcement learning | `.hb` `.hp` |
| [`MLambda.AI.Minds`](src/MLambda.AI.Minds/) | modal logic: time, duty, knowledge, belief, mental states | `.hs` `.hp` |
| [`MLambda.AI.Actuarial`](src/MLambda.AI.Actuarial/) | house-purchase risk, from `houses.csv` | all four |
| [`MLambda.AI.Airline`](src/MLambda.AI.Airline/) | an LLM assistant held to policies as deontic norms (Moffatt v. Air Canada) | `.hs` `.hp` `.ha` `.hk` + TAV |

All six exist: `MLambda.AI.Logic` (six samples), `MLambda.AI.Agent` (three agents and the theory
beneath them), `MLambda.AI.ML` (four models), `MLambda.AI.Learning` (a bandit, two cliff learners and the
algebra they rely on), `MLambda.AI.Minds` (time, duty, knowledge and belief, in five samples), and the
capstone, `MLambda.AI.Actuarial` — three agents, one real house, one
verdict. Read [docs/actuarial](docs/actuarial/) last.

`MLambda.AI.Airline` is the first sample that talks to a language model. In Moffatt v. Air Canada
(2024 BCCRT 149) a chatbot promised a bereavement refund the written policy did not allow, and the
airline was held to it. Here DeepSeek reads the message and phrases the reply, and nothing it says
commits anyone:

- `Policy.hs` is the policy expert. Every policy is a norm of deontic logic — `O(airline) refund` under
  REF-1, `F(airline) bereavement_fare` under BRV-2 — read over ideal situations, and CHAT-1 forbids the
  assistant to promise what the airline is not obliged to give. `Policy.hp` proves axiom D and the
  norms the Moffatt case turns on.
- `Assistant.ha` chooses between answering, asking for a booking and declining.
- `Reply.hk` fixes the order: review the draft, phrase the norms, fall back to plain wording, print.
- The prompts are TAV templates; every reply prints the norms it rests on.

It needs a DeepSeek key in the environment variable `LLM-API` (or `LLM_API`); the endpoint is a
constant in `DeepSeek.cs`. Its tests use a fake model, including one taken over by a prompt injection.

## Where to start

- New to all of it: [docs/hilbert/01-what-hilbert-is.md](docs/hilbert/01-what-hilbert-is.md)
- Ready for a sample: [docs/L1-novice/logic.md](docs/L1-novice/logic.md)
