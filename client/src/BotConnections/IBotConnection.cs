namespace ModuloFivePadClient.BotConnections
{
    public delegate void ActionListRetreivedHandler(IBotConnection sender, IEnumerable<BotAction> botActions);
    public delegate void ActionSuceededHandler(IBotConnection sender);
    public delegate void ActionFailedHandler(IBotConnection sender, IActionFailureReason actionFailureReason);

    public interface IBotConnection
    {
        String BotName { get; }

        Task<BotStatus> GetStatusAsync();

        Task<IList<BotAction>> GetBotActionListsAsync();
        
        void InvokeAction(string? botActionId, IEnumerable<BotArgument> arguments);

        IList<BotAction> BotActions { get; }

        event ActionListRetreivedHandler? OnActionListRetrieved;

        event ActionSuceededHandler? ActionSuceeded;
    }
}
