// Program.cs — a console chat with the airline assistant, over DeepSeek.
//
// THE KEY COMES FROM THE ENVIRONMENT. Set `LLM-API` (or `LLM_API` where a shell cannot spell a hyphen) to a
// DeepSeek API key. The address the prompts go to is `DeepSeek.Endpoint`, a constant, and nothing reads it
// from configuration.
using System.Text;
using MLambda.AI.Airline.Chat;
using MLambda.AI.Airline.Llm;
using MLambda.AI.Airline.Records;

Console.OutputEncoding = Encoding.UTF8;

var key = DeepSeek.KeyFrom(DeepSeek.ReadEnvironment);

if (key is null)
{
    Console.Error.WriteLine($"No DeepSeek key. Set the environment variable {DeepSeek.KeyVariable} (or {DeepSeek.KeyVariableAlias}) and run again.");
    return 1;
}

using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(60) };
using var cancel = new CancellationTokenSource();
Console.CancelKeyPress += (_, press) =>
{
    press.Cancel = true;
    cancel.Cancel();
};

var wording = PolicyWording.Load("data/policies.json");
var desk = new Desk(new DeepSeek(http, key), wording, Bookings.Load("data/bookings.json"), Console.Out);

Console.WriteLine("Airline assistant — refunds and bereavement fares, answered from written policy.");
Console.WriteLine();
Console.WriteLine("In Moffatt v. Air Canada (2024 BCCRT 149) a chatbot promised a bereavement refund the written");
Console.WriteLine("policy did not allow, and the airline was held to it. Here a model drafts and phrases, and");
Console.WriteLine("Policy.hs — every policy an obligation O or a prohibition F — decides. Every reply names its norms.");
Console.WriteLine();
Console.WriteLine("Bookings you can ask about:");
Console.WriteLine("  MOF240  already flown, non-refundable  (the Moffatt situation)");
Console.WriteLine("  FUT315  not flown yet, non-refundable");
Console.WriteLine("  RFD512  not flown yet, refundable");
Console.WriteLine("  CXL777  cancelled by the airline");
Console.WriteLine();
Console.WriteLine($"Policies: {wording.Source}");
Console.WriteLine("Type 'quit' to leave.");
Console.WriteLine();

while (!cancel.IsCancellationRequested)
{
    Console.Write("You: ");
    var line = Console.ReadLine();

    if (line is null || line.Trim().Equals("quit", StringComparison.OrdinalIgnoreCase))
    {
        break;
    }

    if (string.IsNullOrWhiteSpace(line))
    {
        continue;
    }

    try
    {
        await desk.HandleAsync(line, cancel.Token);
    }
    catch (OperationCanceledException)
    {
        break;
    }
}

return 0;
