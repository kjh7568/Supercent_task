using UnityEngine;

[CreateAssetMenu(fileName = "PrisonUpgradeData", menuName = "Game/Prison Upgrade Data")]
public class PrisonUpgradeData : ScriptableObject
{
    [System.Serializable]
    public class StageData
    {
        public int cost;
        public int newCapacity;
    }

    /// <summary>인덱스 0 = 1단계 업그레이드, 1 = 2단계 ...</summary>
    public StageData[] stages;
}
