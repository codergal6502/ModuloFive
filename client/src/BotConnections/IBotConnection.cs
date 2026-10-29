namespace ModuloFivePadClient.BotConnections
{
    public delegate void ActionListRetreivedHandler(IBotConnection sender, IEnumerable<BotAction> botActions);
    public delegate void ActionSuceededHandler(IBotConnection sender, string actionId);
    public delegate void ActionFailedHandler(IBotConnection sender, string actionId, IActionFailureReason actionFailureReason);

    public interface IBotConnection
    {
        String BotName { get; }

        Task<BotStatus> GetStatusAsync();

        Task<IList<BotAction>> GetBotActionListsAsync();

        bool InvokeAction(string botActionId, IEnumerable<BotArgument> arguments);

        IList<BotAction> BotActions { get; }

        event ActionListRetreivedHandler? OnActionListRetrieved;

        event ActionSuceededHandler? ActionSuceeded;
        event ActionFailedHandler? ActionFailed;
    }
}
