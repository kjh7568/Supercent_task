using System;
using System.Collections;
using UnityEngine;

public class OreObject : MonoBehaviour
{
    public enum OreState { Active, Mining, Hidden }

    [Header("Settings")]
    [SerializeField] private float miningDuration = 2f;
    [SerializeField] private float respawnTime = 10f;

    public OreState State { get; private set; } = OreState.Active;
    public event Action OnPickedUp;

    private Renderer[] renderers;
    private Collider[] colliders;
    private float miningTimer;

    private void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
        colliders = GetComponentsInChildren<Collider>();
    }

    public void StartMining()
    {
        if (State != OreState.Active) return;
        State = OreState.Mining;
        miningTimer = 0f;
    }

    public void UpdateMining(float deltaTime)
    {
        if (State != OreState.Mining) return;

        miningTimer += deltaTime;
        if (miningTimer >= miningDuration)
            CompletePickup();
    }

    public void CancelMining()
    {
        if (State != OreState.Mining) return;
        State = OreState.Active;
        miningTimer = 0f;
    }

    private void CompletePickup()
    {
        State = OreState.Hidden;
        miningTimer = 0f;
        SetVisible(false);
        OnPickedUp?.Invoke();
        StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(respawnTime);
        State = OreState.Active;
        SetVisible(true);
    }

    private void SetVisible(bool visible)
    {
        foreach (var r in renderers) r.enabled = visible;
        foreach (var c in colliders) c.enabled = visible;
    }
}
