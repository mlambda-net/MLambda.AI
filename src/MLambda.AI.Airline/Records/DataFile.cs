// DataFile.cs — reads a JSON file from data/, wherever the process was started.
using System.Text.Json;

namespace MLambda.AI.Airline.Records;

internal static class DataFile
{
    private static readonly JsonSerializerOptions Json = new() { PropertyNameCaseInsensitive = true };

    /// <summary>A data file, from the working directory or, failing that, beside the executable.</summary>
    ///
    /// <remarks>BOTH, BECAUSE `dotnet run` AND AN XUNIT HOST DISAGREE about where they run from; the
    /// csproj copies data/ to the output so the second always succeeds.</remarks>
    public static T Read<T>(string path)
    {
        var resolved = File.Exists(path) ? path : Path.Combine(AppContext.BaseDirectory, path);

        return JsonSerializer.Deserialize<T>(File.ReadAllText(resolved), Json)
            ?? throw new InvalidDataException($"{path} is empty.");
    }
}
