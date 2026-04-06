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

    /// <summary>MiningController에서 호출. 공간 있으면 광석 위치에서 아이템 스폰 후 반환. 공간 없으면 null.</summary>
    public GameObject TryMine()
    {
        if (State != OreState.Active) return null;
        if (stackSystem != null && !stackSystem.HasSpace(stackConfig.itemType)) return null;

        State = OreState.Hidden;
        SetVisible(false);
        Debug.Log($"[OreObject] 채굴 완료 - {gameObject.name}");
        StartCoroutine(RespawnRoutine());

        return Instantiate(stackConfig.itemPrefab, transform.position, Quaternion.identity);
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
