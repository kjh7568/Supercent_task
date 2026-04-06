using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// 존에 쌓인 아이템을 플레이어가 진입하면 순차적으로 StackSystem으로 전달하는 존.
/// </summary>
public class PickupZone : InteractionZone
{
    [Header("Pickup Settings")]
    [SerializeField] private StackItemType outputType = StackItemType.Product;
    [SerializeField] private float transferDuration = 0.3f;
    [SerializeField] private float transferInterval = 0.1f;
    [SerializeField] private float arcHeight = 1f;

    private readonly List<GameObject> placedItems = new();
    private Coroutine transferCoroutine;
    private Tween _activeTween;

    public int Count => placedItems.Count;
    public bool HasSpace => layout != null && placedItems.Count < layout.Capacity;
    public int RemainingCapacity => layout != null ? layout.Capacity - placedItems.Count : 0;
    public StackItemType OutputType => outputType;

    /// <summary>CashRegister 등 외부에서 코인 발사 시 슬롯 localPosition 쿼리용</summary>
    public Vector3 GetItemLocalPositionAt(int index)
        => placementOffset + layout.GetLocalPosition(index);

    /// <summary>CashRegister 등 외부에서 코인 발사 시 슬롯 worldPosition 쿼리용</summary>
    public Vector3 GetItemWorldPositionAt(int index)
        => transform.position + transform.rotation * (placementOffset + layout.GetLocalPosition(index));

    /// <summary>이미 위치가 확정된 아이템을 placedItems에 직접 등록 (코인 날리기 연출 완료 후 사용)</summary>
    public void AddPlacedItem(GameObject item)
    {
        placedItems.Add(item);
        SoundManager.Instance?.PlaySound(SoundType.ItemAppear, item.transform.position);
    }

    protected override void OnPlayerEnter(Collider player)
    {
        Debug.Log($"[PickupZone] 플레이어 진입 - {gameObject.name} (출력 타입: {outputType})");

        var stackSystem = player.GetComponent<StackSystem>();
        if (stackSystem == null)
        {
            Debug.LogWarning($"[PickupZone] StackSystem 없음 - {player.name}");
            return;
        }

        Debug.Log($"[PickupZone] 아이템 수: {placedItems.Count}, HasSpace({outputType}): {stackSystem.HasSpace(outputType)}");

        if (transferCoroutine != null) StopCoroutine(transferCoroutine);
        transferCoroutine = StartCoroutine(TransferSequence(stackSystem));
    }

    protected override void OnPlayerExit(Collider player)
    {
        Debug.Log($"[PickupZone] 플레이어 이탈 - {gameObject.name}");

        // Kill(complete: true) → onComplete 강제 실행 → ReceiveItem() 즉시 확정
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
        while (IsPlayerInside)
        {
            // 아이템이 없거나 공간이 없으면 대기
            if (placedItems.Count == 0 || !stackSystem.HasSpace(outputType))
            {
                yield return new WaitForSeconds(0.1f);
                continue;
            }

            var stackPoint = stackSystem.GetStackPoint(outputType);
            if (stackPoint == null) { yield return new WaitForSeconds(0.1f); continue; }

            var item = TakeTopItem();
            if (item == null) break;

            Vector3 localTargetPos = stackPoint.GetNextItemLocalPosition();
            Quaternion localTargetRot = Quaternion.identity;

            Debug.Log($"[PickupZone] 이동 시작 (남은 수량: {placedItems.Count}) - {gameObject.name}");

            _activeTween = ItemTransferAnimator.Transfer(
                item,
                stackPoint.transform,
                localTargetPos,
                localTargetRot,
                transferDuration,
                arcHeight,
                onComplete: () =>
                {
                    bool received = stackSystem.ReceiveItem(item, outputType);
                    if (!received)
                    {
                        // 공간 없으면 원위치 복구
                        PlaceItem(item);
                        Debug.Log($"[PickupZone] StackSystem 공간 없음, 복귀");
                    }
                    else
                    {
                        SoundType pickupSound = outputType == StackItemType.Coin ? SoundType.PickupCoin : SoundType.PickupItem;
                        SoundManager.Instance?.PlaySound(pickupSound, transform.position);
                        Debug.Log($"[PickupZone] 전달 완료 - {gameObject.name}");
                    }
                }
            );

            yield return _activeTween.WaitForCompletion();

            _activeTween = null;
            yield return new WaitForSeconds(transferInterval);
        }

        transferCoroutine = null;
    }

    /// <summary>외부(Processor, CashRegister 등)에서 아이템을 직접 배치할 때 사용</summary>
    public void PlaceItem(GameObject item)
    {
        if (!HasSpace || item == null) return;

        int index = placedItems.Count;
        item.transform.SetParent(null);
        item.transform.position = GetItemWorldPosition(index);
        item.transform.rotation = transform.rotation * layout.ItemRotation;
        placedItems.Add(item);
        SoundManager.Instance?.PlaySound(SoundType.ItemAppear, item.transform.position);

        Debug.Log($"[PickupZone] 아이템 추가 ({placedItems.Count}/{layout.Capacity}) - {gameObject.name}");
    }

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
