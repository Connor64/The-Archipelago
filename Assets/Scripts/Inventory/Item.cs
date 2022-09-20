using UnityEngine;
using System;

public enum ItemType {
    crop,
    shovel,
    trash,
}

[Serializable]
public class Item {
    [SerializeField]
    private ItemType itemType;

    [SerializeField]
    private string itemName;

    [SerializeField]
    private Sprite[] sprites;

    [SerializeField]
    private int maxQuantity;

    [SerializeField]
    private bool spawnable;

    [TextArea(10,15)]   
    [SerializeField]
    private string description;

    [SerializeField]
    private int timeToMaturity;

    [SerializeField]
    private int value;

    // ----------------------- Getters ----------------------- \\

    public string GetName() {
        return itemName;
    }

    /// <summary>
    /// Returns the first sprite in the array.
    /// </summary>
    /// <returns>A sprite object</returns>
    public Sprite GetSprite() {
        return sprites[0];
    }

    /// <summary>
    /// Returns the sprite at the specified index in the sprite array. Returns the first sprite if it is out of bounds.
    /// </summary>
    /// <param name="index">The specified location of the desired sprite</param>
    /// <returns>A sprite object</returns>
    public Sprite GetSprite(int index) {
        if (index >= 0 || index < sprites.Length) {
            return sprites[index];
        } else {
            Debug.Log("Item sprite index out of range!");
            return sprites[0];
        }
    }

    public int GetSpriteCount() {
        return sprites.Length;
    }

    public ItemType GetItemType() {
        return itemType;
    }

    public int GetMaxQuantity() {
        return maxQuantity;
    }

    public bool IsSpawnable() {
        return spawnable;
    }

    public string GetDescription() {
        return description;
    }

    public int GetTimeToMaturity() {
        return timeToMaturity;
    }

    public int GetValue() {
        return value;
    }
}