// Steps.cs — runs a plan from Reply.hk and answers each command word it sends.
//
// THE PLAN LEADS, THE HOST FOLLOWS. The plan sends a word on `act` and blocks on `done`; this loop takes
// the word, lets the caller carry it out, and sends the caller's one-word answer back. When the plan
// finishes, the loop does. The host never decides what comes next — which is the point of having a plan.
//
// THE SAME LOOP AS MLambda.Turing.Business's StepProtocol, written out here because a sample should not
// depend on a product for thirty lines.
using System.Reflection;
using MLambda.Hilbert.Runtime.Plans;

namespace MLambda.AI.Airline.Chat;

public static class Steps
{
    /// <summary>The text of Reply.hk, embedded by the csproj.</summary>
    public static string Source { get; } = Embedded("MLambda.AI.Airline.Reply.hk");

    /// <summary>Runs <paramref name="plan"/> to completion and returns every command it sent, in order.</summary>
    public static async Task<IReadOnlyList<string>> RunAsync(
        string plan,
        IBeliefStore beliefs,
        Func<string, CancellationToken, Task<string>> serve,
        CancellationToken cancellationToken = default)
    {
        var act = new PlanChannel();
        var done = new PlanChannel();
        var handle = new PlanRunner(Source, beliefs).Start(plan, act, done);
        var served = new List<string>();

        try
        {
            while (true)
            {
                using var waiting = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                var ready = act.WaitAsync(waiting.Token).AsTask();

                if (await Task.WhenAny(handle.Completion, ready).ConfigureAwait(false) == handle.Completion)
                {
                    waiting.Cancel();
                    await handle.Completion.ConfigureAwait(false);

                    return served;
                }

                await ready.ConfigureAwait(false);

                if (!act.TryReceive(out var word))
                {
                    continue;
                }

                var command = (string)word;
                served.Add(command);

                var answer = await serve(command, cancellationToken).ConfigureAwait(false);
                await done.SendAsync(answer, cancellationToken).ConfigureAwait(false);
            }
        }
        catch
        {
            handle.Abort();
            throw;
        }
    }

    private static string Embedded(string name)
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(name)
            ?? throw new InvalidOperationException($"{name} is not embedded in this assembly.");
        using var reader = new StreamReader(stream);

        return reader.ReadToEnd();
    }
}

/// <summary>What a plan came to believe while it ran, as `name(arg, …)`.</summary>
public sealed class Journal : IBeliefStore
{
    private readonly List<string> beliefs = [];

    public IReadOnlyList<string> Beliefs => beliefs;

    public void Believe(string name, IReadOnlyList<object> args) => beliefs.Add(Written(name, args));

    public void Forget(string name, IReadOnlyList<object> args) => beliefs.Remove(Written(name, args));

    private static string Written(string name, IReadOnlyList<object> args) => $"{name}({string.Join(", ", args)})";
}
