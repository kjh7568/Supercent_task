using System;
using System.Collections;
using UnityEngine;

public class OreObject : MonoBehaviour
{
    public enum OreState { Active, Hidden }

    [Header("Settings")]
    [SerializeField] private float respawnTime = 10f;

    [Header("Stack")]
    [SerializeField] private StackItemConfig stackConfig;

    public OreState State { get; private set; } = OreState.Active;
    public StackItemConfig StackConfig => stackConfig;
    public event Action<StackItemConfig> OnPickedUp;

    private Renderer[] renderers;
    private Collider[] colliders;
    private StackSystem stackSystem;

    private void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
        colliders = GetComponentsInChildren<Collider>();
        stackSystem = FindObjectOfType<StackSystem>();
    }

    /// <summary>WorkerController에서 호출. stackSystem 체크 없이 채굴 완료 처리.</summary>
    public void MineByWorker()
    {
        if (State != OreState.Active) return;

        State = OreState.Hidden;
        SetVisible(false);
        Debug.Log($"[OreObject] 인부 채굴 완료 - {gameObject.name}");
        StartCoroutine(RespawnRoutine());
    }

    /// <summary>MiningController에서 호출. 공간 있으면 즉시 채굴 완료 처리.</summary>
    public void TryMine()
    {
        if (State != OreState.Active) return;
        if (stackSystem != null && !stackSystem.HasSpace(stackConfig.itemType)) return;

        State = OreState.Hidden;
        SetVisible(false);
        OnPickedUp?.Invoke(stackConfig);
        Debug.Log($"[OreObject] 채굴 완료 - {gameObject.name}");
        StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(respawnTime);
        State = OreState.Active;
        SetVisible(true);
        Debug.Log($"[OreObject] 리스폰 - {gameObject.name}");
    }

    private void SetVisible(bool visible)
    {
        foreach (var r in renderers) r.enabled = visible;
        foreach (var c in colliders) c.enabled = visible;
    }
}
