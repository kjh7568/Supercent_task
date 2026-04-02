using UnityEngine;

[CreateAssetMenu(fileName = "StackItemConfig", menuName = "StackSystem/Item Config")]
public class StackItemConfig : ScriptableObject
{
    [Header("Item Info")]
    public StackItemType itemType;
    public GameObject itemPrefab;
}
