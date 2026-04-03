using System.Collections;
using System.Collections.Generic;
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

    public int Count => placedItems.Count;
    public bool HasSpace => layout != null && placedItems.Count < layout.Capacity;
    public StackItemType OutputType => outputType;

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
        transferCoroutine = StartCoroutine(TransferSequence(stackSystem, player.transform));
    }

    protected override void OnPlayerExit(Collider player)
    {
        Debug.Log($"[PickupZone] 플레이어 이탈 - {gameObject.name}");

        if (transferCoroutine != null)
        {
            StopCoroutine(transferCoroutine);
            transferCoroutine = null;
        }
    }

    private IEnumerator TransferSequence(StackSystem stackSystem, Transform playerTransform)
    {
        while (IsPlayerInside && placedItems.Count > 0 && stackSystem.HasSpace(outputType))
        {
            var item = TakeTopItem();
            if (item == null) break;

            Vector3 targetPos = playerTransform.position + Vector3.up;

            Debug.Log($"[PickupZone] 이동 시작 (남은 수량: {placedItems.Count}) - {gameObject.name}");

            yield return StartCoroutine(ItemTransferAnimator.Transfer(
                item, targetPos, transferDuration, arcHeight
            ));

            if (item == null) break;

            bool received = stackSystem.ReceiveItem(item, outputType);
            if (!received)
            {
                // 공간 없으면 원위치 복구
                PlaceItem(item);
                Debug.Log($"[PickupZone] StackSystem 공간 없음, 복귀");
                break;
            }

            Debug.Log($"[PickupZone] 전달 완료 - {gameObject.name}");

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

        Debug.Log($"[PickupZone] 아이템 추가 ({placedItems.Count}/{layout.Capacity}) - {gameObject.name}");
    }

    private GameObject TakeTopItem()
    {
        if (placedItems.Count == 0) return null;

        int last = placedItems.Count - 1;
        var item = placedItems[last];
        placedItems.RemoveAt(last);
        return item;
    }

    private Vector3 GetItemWorldPosition(int index)
        => transform.position + transform.rotation * layout.GetLocalPosition(index);
}
