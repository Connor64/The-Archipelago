using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour {
    private int columns = 3;
    private int rows = 5;
    private int hotbarSlots = 3;

    private GameObject mainInventoryObject;
    private GameObject hotbarObject;
    public GameObject gameplayHotbar;

    public static List<InventorySlot> inventorySlots;

    public ItemManager itemManager;

    [HideInInspector]
    public static int playerBalance = 0;

    private static InventoryManager inventoryInstance;

    public CropBed targetCropBed;

    // Start is called before the first frame update
    void Start() {
        DontDestroyOnLoad(gameObject);

        // Singleton-like behavior
        if (inventoryInstance == null) {
            inventoryInstance = this;
        } else {
            Destroy(gameObject);
        }

        inventorySlots = new List<InventorySlot>();

        MenuController menuController = GameObject.Find("Canvas").GetComponent<MenuController>();

        menuController.AccessMenu(2);

        mainInventoryObject = menuController.GetMainInventory();
        hotbarObject = menuController.GetHotbar();

        // Array sizes should update based on scene
        columns = mainInventoryObject.GetComponent<GridLayoutGroup>().constraintCount;
        rows = mainInventoryObject.transform.childCount / columns;
        hotbarSlots = hotbarObject.transform.childCount;

        foreach (Transform slot in hotbarObject.transform) {
            inventorySlots.Add(slot.GetComponent<InventorySlot>());
        }
        foreach (Transform slot in mainInventoryObject.transform) {
            inventorySlots.Add(slot.GetComponent<InventorySlot>());
        }

        // If the shovel is not in the player's inventory, add it.
        if (HasItem(itemManager.GetTools()[0]) == 0) {
            AddItem(itemManager.GetTools()[0], 1);
        }

        menuController.Resume();
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.P)) {
            playerBalance += 50;
        }
    }

    /// <summary>
    /// Add an item to the first available slot in the player's inventory.
    /// </summary>
    /// <param name="item">The item to be added</param>
    /// <returns>True if the item was added, false if the item could not be added.</returns>
    public bool AddItem(Item item, int quantity) {
        if (item != null) { // If the item is null, don't do anything
            // int firstRow = -1, firstColumn = -1; // The first available location if all slots containing the item are full
            InventorySlot firstSlot = null;

            foreach (InventorySlot slot in inventorySlots) {
                // If the slot contains an item that is the same type and isn't full
                if (slot.GetQuantity() > 0 && slot.GetItemName() == item.GetName() && !slot.IsFull()) {
                    int overfill = slot.IncrementQuantity(quantity);
                    if (overfill > 0)
                        AddItem(item, overfill);
                    // slot.IncrementQuantity(1); // TODO: Allow spill-over (spawn item back into world if it cannot be added fully into inventory)
                    return true;
                } else if (firstSlot == null && slot.GetQuantity() == 0) {
                    firstSlot = slot; // The first available slot if the item can't be added to a pre-existing stack
                }
            }

            // If the item is a new item to the inventory or it couldn't fit into the other slots of the same type
            // If there is an available slot available
            if (firstSlot != null) {
                firstSlot.SetItem(item);
                return true;
            } else {
                print("Cannot add item!");
            }
        }
        return false;
    }

    /// <summary>
    /// Checks if the player has the specified item type in their inventory.
    /// </summary>
    /// <param name="item">The item to be searched for</param>
    /// <returns>The amount of the item the player has</returns>
    public int HasItem(Item item) {
        int quantity = 0;
        foreach (InventorySlot slot in inventorySlots) {
            if (slot.GetQuantity() > 0 && item.GetName() == slot.GetItemName()) {
                quantity += slot.GetQuantity();
            }
        }

        return quantity;
    }

    private void OnTriggerEnter(Collider other) {
        ItemObject itemObject = other.gameObject.GetComponent<ItemObject>();

        if (itemObject != null) {
            if (AddItem(itemObject.GetItem(), 1)) {
                // TODO: Add variable amounts of item in a single stack spawned
                Destroy(other.gameObject);
            }
        }
    }
}