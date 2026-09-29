using Microsoft.Extensions.AI;

namespace PantryChef.Tests.Fakes;

public sealed class FakeChatClient : IChatClient
{
    public const string OneRecipeJson = """
        { "recipes": [ {
            "title": "Spinach Rice", "summary": "Easy weeknight bowl.", "totalMinutes": 20,
            "ingredients": [ { "name": "spinach", "amount": "1 bag" }, { "name": "rice", "amount": "1 cup" } ],
            "steps": [ "Cook the rice.", "Stir in the spinach." ],
            "missingIngredients": []
        } ] }
        """;

    public string ResponseText { get; set; } = OneRecipeJson;
    public Exception? ExceptionToThrow { get; set; }
    public IReadOnlyList<ChatMessage> LastMessages { get; private set; } = [];
    
    public void Reset()
    {
        ResponseText = OneRecipeJson;
        ExceptionToThrow = null;
        LastMessages = [];
    }

    public Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
    {
        LastMessages = messages.ToList();
        cancellationToken.ThrowIfCancellationRequested();
        if (ExceptionToThrow is not null) throw ExceptionToThrow;
        return Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant, ResponseText)));
    }

    public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();

    public object? GetService(Type serviceType, object? serviceKey = null) => null;

    public void Dispose() { }
}