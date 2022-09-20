using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An object that contains an individual item. Can be used for crop tiles or chests.
/// </summary>
public abstract class ItemContainer : InteractableObject {
    protected Item item;
    protected int quantity;
    protected InventoryManager playerInventory;
    
    // Start is called before the first frame update
    protected override void Start() {
        base.Start();
        playerInventory = GameObject.FindGameObjectWithTag("Player").GetComponent<InventoryManager>();
    }

    public abstract override void Interact();

    /// <summary>
    /// Sets the item of the container with a specified quantity.
    /// </summary>
    /// <param name="item">The new item o be added</param>
    /// <param name="quantity">The quantity of the new item</param>
    public virtual int SetItem(Item item, int quantity) {
        this.item = item;
        this.quantity = Mathf.Min(quantity, item.GetMaxQuantity());
        return quantity - this.quantity;
    }

    /// <summary>
    /// Sets the item of the container and returns the previous item.
    /// </summary>
    /// <param name="item">The new item to be added</param>
    /// <param name="quantity">The quantity of the new item</param>
    /// <returns>The item that was previously assigned to the container. Null if nothing was assigned.</returns>
    public virtual Item SetItem(Item item, int quantity, out int oldQuantity) {
        Item oldItem = this.item;
        oldQuantity = this.quantity; // "returns" the quantity of the item to be replaced

        this.item = item;
        this.quantity = quantity;
        return oldItem;
    }

    public virtual Item RemoveItem(out int oldQuantity) {
        Item oldItem = item;
        oldQuantity = quantity;

        item = null;
        quantity = 0;

        return oldItem;
    }

    public Item GetItem() {
        return item;
    }

    public int GetQuantity() {
        return quantity;
    }
}