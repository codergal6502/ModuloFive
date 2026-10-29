using System.Diagnostics.CodeAnalysis;
using ModuloFivePadClient.BotConnections;

namespace MockObjects;

public class MockBotConnection : IBotConnection
{
    public string BotName { get; set; } = string.Empty;
    public bool ShouldInvocationSucceed { get; set; } = true;

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
    public bool InvokeAction(string botActionId, IEnumerable<BotArgument> arguments)
    {
        OnMockInvokeAction?.Invoke(botActionId, arguments);
        if (ShouldInvocationSucceed)
        {
            ActionSuceeded?.Invoke(this, botActionId);
            return true;
        }
        else
        {
            ActionFailed?.Invoke(this, botActionId, new MockActionFailureReason());
            return false;
        }
    }

    public Action<string, IEnumerable<BotArgument>>? OnMockInvokeAction { get; set; }
}
