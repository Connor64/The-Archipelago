using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/ItemManager", order = 1)]
public class ItemManager : ScriptableObject {
    [SerializeField]
    private Item[] crops;

    [SerializeField]
    private Item[] tools;

    [SerializeField]
    private Item[] trash;

    [SerializeField]
    private Dictionary<ItemType, Item[]> ItemsByType;

    public GameObject itemObjectPrefab;

    void AddToDictionary() {
        ItemsByType = new Dictionary<ItemType, Item[]>();

        ItemsByType.Add(ItemType.crop, crops);
        ItemsByType.Add(ItemType.trash, trash);
        ItemsByType.Add(ItemType.shovel, tools);
    }

    // ----------------------- Getters ----------------------- \\

    public Item[] GetCrops() {
        return crops;
    }

    public Item[] GetTools() {
        return tools;
    }

    public Item[] GetTrash() {
        return trash;
    }

    public Item[] GetItemsByType(ItemType itemType) {
        if (ItemsByType == null) {
            AddToDictionary();
        }

        Item[] array;
        ItemsByType.TryGetValue(itemType, out array);
        if (array == null)
            Debug.LogError("Unable to get item array of type '" + itemType.ToString() + "'.");
        return array;
    }

    public Item GetRandomItemByType(ItemType itemType) {
        Item[] items = GetItemsByType(itemType);
        return items[Random.Range(0, items.Length)];
    }
}