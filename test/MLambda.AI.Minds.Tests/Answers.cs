// Answers.cs — a query's answer as a list, so a test can pin the whole of it.
//
// PINNING THE WHOLE ANSWER IS THE POINT. A query that answers nothing satisfies every
// `DoesNotContain` ever written, so every negative assertion in this project stands beside a
// positive one about the same answer.
namespace MLambda.AI.Minds.Tests;

internal static class Answers
{
    /// <summary>Everything the query streamed, sorted.</summary>
    public static async Task<List<string>> Sorted(IAsyncEnumerable<string> rows)
    {
        var all = new List<string>();
        await foreach (var row in rows)
        {
            all.Add(row);
        }

        all.Sort(StringComparer.Ordinal);
        return all;
    }

    /// <summary>Everything the query streamed, in the order it streamed it.</summary>
    public static async Task<List<T>> Rows<T>(IAsyncEnumerable<T> rows)
    {
        var all = new List<T>();
        await foreach (var row in rows)
        {
            all.Add(row);
        }

        return all;
    }
}
