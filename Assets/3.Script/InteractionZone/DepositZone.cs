using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// 플레이어가 진입하면 StackSystem에서 아이템을 순차적으로 받아 쌓는 존.
/// </summary>
public class DepositZone : InteractionZone
{
    [Header("Deposit Settings")]
    [SerializeField] private StackItemType acceptedType = StackItemType.Ore;
    [SerializeField] private float transferDuration = 0.3f;
    [SerializeField] private float transferInterval = 0.1f;
    [SerializeField] private float arcHeight = 1f;

    private readonly List<GameObject> placedItems = new();
    private Coroutine transferCoroutine;
    private Tween _activeTween;

    public int Count => placedItems.Count;
    public bool HasSpace => layout != null && placedItems.Count < layout.Capacity;
    public bool IsTransferring => transferCoroutine != null;
    public StackItemType AcceptedType => acceptedType;

    protected override void OnPlayerEnter(Collider player)
    {
        Debug.Log($"[DepositZone] 플레이어 진입 - {gameObject.name} (수용 타입: {acceptedType})");

        var stackSystem = player.GetComponent<StackSystem>();
        if (stackSystem == null) return;

        if (transferCoroutine != null) StopCoroutine(transferCoroutine);
        transferCoroutine = StartCoroutine(TransferSequence(stackSystem));
    }

    protected override void OnPlayerExit(Collider player)
    {
        Debug.Log($"[DepositZone] 플레이어 이탈 - {gameObject.name}");

        // Kill(complete: true) → onComplete 강제 실행 → placedItems.Add() 즉시 확정
        _activeTween?.Kill(complete: true);
        _activeTween = null;

        if (transferCoroutine != null)
        {
            StopCoroutine(transferCoroutine);
            transferCoroutine = null;
        }
    }

    private IEnumerator TransferSequence(StackSystem stackSystem)
    {
        while (IsPlayerInside && HasSpace && stackSystem.HasItem(acceptedType))
        {
            var item = stackSystem.TakeItem(acceptedType);
            if (item == null) break;

            int targetIndex = placedItems.Count;
            Vector3 localTargetPos = placementOffset + layout.GetLocalPosition(targetIndex);
            Quaternion localTargetRot = layout.ItemRotation;

            Debug.Log($"[DepositZone] 이동 시작 ({targetIndex + 1}/{layout.Capacity}) - {gameObject.name}");

            Vector3 worldTargetPos = GetItemWorldPosition(targetIndex);

            _activeTween = ItemTransferAnimator.Transfer(
                item,
                transform,
                localTargetPos,
                localTargetRot,
                transferDuration,
                arcHeight,
                onComplete: () =>
                {
                    if (item != null)
                    {
                        item.transform.SetParent(null);
                        item.transform.position = worldTargetPos;
                        item.transform.rotation = transform.rotation * layout.ItemRotation;
                        placedItems.Add(item);
                        SoundManager.Instance?.PlaySound(SoundType.Deposit, worldTargetPos);
                        Debug.Log($"[DepositZone] 배치 완료 ({placedItems.Count}/{layout.Capacity}) - {gameObject.name}");
                    }
                }
            );

            yield return _activeTween.WaitForCompletion();

            _activeTween = null;
            yield return new WaitForSeconds(transferInterval);
        }

        transferCoroutine = null;
    }

    /// <summary>외부(Processor 등)에서 아이템을 직접 배치할 때 사용</summary>
    public void PlaceItem(GameObject item)
    {
        if (!HasSpace || item == null) return;

        int index = placedItems.Count;
        item.transform.SetParent(null);
        item.transform.position = GetItemWorldPosition(index);
        item.transform.rotation = transform.rotation * layout.ItemRotation;
        placedItems.Add(item);
        SoundManager.Instance?.PlaySound(SoundType.Deposit, item.transform.position);
    }

    /// <summary>맨 위 아이템 제거 후 반환</summary>
    public GameObject TakeTopItem()
    {
        if (placedItems.Count == 0) return null;

        int last = placedItems.Count - 1;
        var item = placedItems[last];
        placedItems.RemoveAt(last);
        return item;
    }

    private Vector3 GetItemWorldPosition(int index)
        => transform.position + transform.rotation * (placementOffset + layout.GetLocalPosition(index));
}
