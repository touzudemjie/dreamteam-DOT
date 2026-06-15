using UnityEngine;

[CreateAssetMenu(fileName = "Item",menuName = "SO/Item")]
public class Item : ScriptableObject
{
    public enum ItemVariants
    {
        Basketball
    }
    [SerializeField] private ItemVariants _item;
}
