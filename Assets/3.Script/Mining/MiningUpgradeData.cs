using UnityEngine;

[CreateAssetMenu(fileName = "MiningUpgradeData", menuName = "Game/Mining Upgrade Data")]
public class MiningUpgradeData : ScriptableObject
{
    [System.Serializable]
    public class StageData
    {
        public float miningInterval;
        public float miningRadius;
        public int maxCarry;
        public int cost;
    }

    /// <summary>인덱스 0 = 1단계 업그레이드, 1 = 2단계 업그레이드 ...</summary>
    public StageData[] stages;
}
