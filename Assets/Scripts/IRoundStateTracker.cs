public interface IRoundStateTracker
{
    bool IsFinalTurnTriggered { get; }
    bool IsFinalTurnInProgress { get; }
}
