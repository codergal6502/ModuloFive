using ModuloFivePadClient.BotConnections;

namespace MockObjects;

public class MockActionFailureReason : IActionFailureReason
{
    public string FailureReason => "The test was configured to fail.";
}
