public interface ITutorialStep
{
    void OnEnter();
    void OnExit();
    bool IsComplete { get; }
}
