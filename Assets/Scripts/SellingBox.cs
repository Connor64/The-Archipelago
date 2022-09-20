using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SellingBox : ItemContainer {
    // private InventoryManager inventoryManager;
    [SerializeField]
    private SpriteRenderer spriteRenderer;
    private Sprite defaultSprite;

    // Start is called before the first frame update
    protected override void Start() {
        base.Start();

        defaultSprite = spriteRenderer.sprite;
    }

    // Update is called once per frame
    protected override void Update() {}

    public override void Interact() {
        if (MouseTracker.GetItem() != null) {
            int overflow = SetItem(MouseTracker.GetItem(), MouseTracker.GetQuantity());
            if (overflow != -1) {
                MouseTracker.SetQuantity(overflow);
            }
        } else if (item != null && MouseTracker.GetItem() == null) {
            int itemQuantity = 0;
            playerInventory.AddItem(RemoveItem(out itemQuantity), itemQuantity);
        }
    }

    public override int SetItem(Item item, int quantity) {
        if (item.GetItemType() == ItemType.crop) {
            int oldQuantity;
            base.RemoveItem(out oldQuantity); // Clears the old item when a new one is placed
            base.SetItem(item, quantity);
            InventoryManager.playerBalance += item.GetValue() * quantity;
            spriteRenderer.sprite = item.GetSprite();
            print(InventoryManager.playerBalance);
            return 0; // Never should have overflow
        } else {
            return quantity; // Return the item back as it is not a crop/sellable item
        }
    }

    public override Item RemoveItem(out int oldQuantity) {
        InventoryManager.playerBalance -= item.GetValue() * quantity;
        spriteRenderer.sprite = defaultSprite;
        return base.RemoveItem(out oldQuantity);
    }
}