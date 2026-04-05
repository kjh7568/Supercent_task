using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    private List<ITutorialStep> steps = new();
    private int currentIndex = -1;

    public ITutorialStep CurrentStep => currentIndex >= 0 && currentIndex < steps.Count
        ? steps[currentIndex] : null;

    public bool IsRunning => currentIndex >= 0 && currentIndex < steps.Count;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (!IsRunning) return;
        if (CurrentStep.IsComplete)
            NextStep();
    }

    public void RegisterStep(ITutorialStep step)
    {
        steps.Add(step);
    }

    public void StartTutorial()
    {
        if (steps.Count == 0)
        {
            Debug.LogWarning("[Tutorial] 등록된 스텝이 없습니다.");
            return;
        }
        currentIndex = 0;
        steps[currentIndex].OnEnter();
        Debug.Log($"[Tutorial] 시작 → 스텝 0");
    }

    private void NextStep()
    {
        steps[currentIndex].OnExit();
        currentIndex++;

        if (currentIndex >= steps.Count)
        {
            Debug.Log("[Tutorial] 모든 스텝 완료");
            return;
        }

        steps[currentIndex].OnEnter();
        Debug.Log($"[Tutorial] 스텝 {currentIndex} 진입");
    }
}
