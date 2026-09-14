// PolicyWording.cs — what each policy SAYS, by id, read from data/policies.json.
//
// WORDS, NOT RULES. Nothing here decides anything: which policy applies to a booking, and whether it obliges
// or forbids, is Policy.hs. This is the sentence a customer reads beside a norm — `BRV-2` has a rule in the
// theory and a title and a text here — and the text a prompt quotes so the model phrases the book's words.
namespace MLambda.AI.Airline.Records;

public sealed record PolicyText(string Id, string Title, string Text);

public sealed class PolicyWording(string source, IReadOnlyList<PolicyText> policies)
{
    /// <summary>Where the wording comes from, and that it is modelled rather than quoted.</summary>
    public string Source => source;

    public IReadOnlyList<PolicyText> All => policies;

    public PolicyText this[string id] =>
        policies.FirstOrDefault(p => p.Id == id) ?? throw new KeyNotFoundException($"No wording for policy '{id}'.");

    public static PolicyWording Load(string path)
    {
        var shelf = DataFile.Read<Shelf>(path);

        return new PolicyWording(shelf.Source, shelf.Policies);
    }

    private sealed record Shelf(string Source, IReadOnlyList<PolicyText> Policies);
}
