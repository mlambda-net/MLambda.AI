// ILlm.cs — the whole of what this assistant needs from a language model: one question, one answer.
//
// DELIBERATELY SMALL. A system prompt and a user prompt go in, text comes out, and that is the entire
// seam. Nothing here streams, calls tools or keeps a conversation, because the model is not the part of
// this program that has to be clever: it reads language and phrases decisions, and Policy.hs decides.
// A test hands in a fake; Program.cs hands in DeepSeek.
namespace MLambda.AI.Airline.Llm;

public interface ILlm
{
    /// <summary>One completion. Throws <see cref="LlmUnavailableException"/> when there is no answer.</summary>
    Task<string> CompleteAsync(string system, string user, CancellationToken cancellationToken = default);
}

/// <summary>The model could not be reached, refused, or answered with nothing.</summary>
///
/// <remarks>NOT A CRASH. The plan reads it as the word "unavailable", and the reply falls back to the
/// plain template — which is exactly as correct as the model's phrasing, only less warm.</remarks>
public sealed class LlmUnavailableException(string message, Exception? inner = null) : Exception(message, inner);
