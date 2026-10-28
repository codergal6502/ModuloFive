using System.Diagnostics.CodeAnalysis;
using ModuloFivePadClient.BotConnections;

namespace MockObjects;

public class MockBotConnection : IBotConnection
{
    public string BotName { get; set; } = string.Empty;
    public bool ShouldInvocationFail { get; set; } = false;

    public IList<BotAction> BotActions { get; set; } = [];

    public event ActionListRetreivedHandler? OnActionListRetrieved;
    public event ActionSuceededHandler? ActionSuceeded;
    public event ActionFailedHandler? ActionFailed;

    public Task<IList<BotAction>> GetBotActionListsAsync()
    {
        return Task.Run(() => BotActions);
    }

    public Task<BotStatus> GetStatusAsync()
    {
        throw new NotImplementedException();
    }

#warning no failure?
    public void InvokeAction(string botActionId, IEnumerable<BotArgument> arguments)
    {
        OnMockInvokeAction?.Invoke(botActionId, arguments);
        if (ShouldInvocationFail)
        {
            ActionFailed?.Invoke(this, new MockActionFailureReason());
        }
        else
        {
            ActionSuceeded?.Invoke(this);
        }
    }

    public Action<string, IEnumerable<BotArgument>>? OnMockInvokeAction { get; set; }
}
