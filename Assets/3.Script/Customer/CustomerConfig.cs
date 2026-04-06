using UnityEngine;

/// <summary>
/// 손님 한 명의 행동 설정값.
/// 요구 아이템 타입·수량, 이동 속도, 아이템당 코인 보상.
/// </summary>
[CreateAssetMenu(fileName = "CustomerConfig", menuName = "Customer/Customer Config")]
public class CustomerConfig : ScriptableObject
{
    [Header("Demand")]
    public StackItemType demandType = StackItemType.Product;
    [Min(1)] public int minDemand = 1;
    [Min(1)] public int maxDemand = 5;

    [Header("Movement")]
    public float moveSpeed = 3f;

    [Header("Reward")]
    public int coinRewardPerItem = 10;

    [Header("Visual")]
    public Material purchasedMaterial;
}
