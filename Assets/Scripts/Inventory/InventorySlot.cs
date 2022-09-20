using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class InventorySlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    [SerializeField]
    private Image icon;
    [SerializeField]
    private TextMeshProUGUI text;

    private Item item;
    private int quantity = 0;

    private GameObject inventoryContainer;

    // Start is called before the first frame update
    void Awake() {
        // icon = GetComponentInChildren<Image>();
        // text = GetComponentInChildren<TextMeshPro>();
        RemoveItem(); // Displays the slots as empty slots with no text or sprite
        inventoryContainer = transform.parent.gameObject; // TODO: change inventory container gameobject to be a script so item traversal (between slots) can be easily tracked
    }

    /// <summary>
    /// Sets the amount of the current item that is inside of the slot.
    /// </summary>
    /// <param name="newQuantity">The new amount of the item</param>
    /// <returns>The overfill amount that could not be added to the slot</returns>
    public int SetQuantity(int newQuantity) {
        quantity = Mathf.Min(item.GetMaxQuantity(), newQuantity);
        text.SetText(quantity.ToString());
        return newQuantity - quantity;
    }

    /// <summary>
    /// Increments the amount of the current item in the inventory slot by the specified amount.
    /// </summary>
    /// <param name="count">The amount that the quantity is to be increased by</param>
    /// <returns>The overfill amount that could not be added to the slot</returns>
    public int IncrementQuantity(int count) {
        return SetQuantity(quantity + count);
    }

    /// <summary>
    /// Returns the total quantity of the item in the slot.
    /// </summary>
    /// <returns>The amount of the current item in the slot</returns>
    public int GetQuantity() {
        return quantity;
    }

    /// <summary>
    /// Sets the slot to hold a new item and resets the sprite and quantity count to represent it
    /// </summary>
    /// <param name="item">The new item of the slot</param>
    public void SetItem(Item item) {
        this.item = item;
        icon.sprite = this.item.GetSprite();
        SetQuantity(1);
        icon.color = new Color(icon.color.r, icon.color.g, icon.color.b, 1); // Makes the sprite opaque to display the item
    }

    /// <summary>
    /// Returns the name of the current item in the slot.
    /// </summary>
    /// <returns>The name of the item in the slot</returns>
    public string GetItemName() {
        return item.GetName();
    }

    /// <summary>
    /// Removes the item currently in the item slot and sets it to empty.
    /// </summary>
    /// <returns>Returns the item object (reference) that was removed</returns>
    public Item RemoveItem() {
        quantity = 0;
        text.SetText("");
        icon.sprite = null;
        icon.color = new Color(icon.color.r, icon.color.g, icon.color.b, 0); // Makes the sprite transparent so nothing is shown

        Item prevItem = item;
        item = null;

        return prevItem;
    }

    /// <summary>
    /// Determines if the slot is full.
    /// </summary>
    /// <returns>True if the slot is full.</returns>
    public bool IsFull() {
        return quantity >= item.GetMaxQuantity();
    }

    public void AttachToMouse() {
        // If the slot has an item and the mouse doesn't have an item attached
        if ((item != null && quantity > 0) && (MouseTracker.GetItem() == null || MouseTracker.GetQuantity() == 0)) {
            print("attached!");
            MouseTracker.SetItem(item, quantity);
            RemoveItem();
            
            // If the mouse has an item attached
        } else if (MouseTracker.GetItem() != null && MouseTracker.GetQuantity() > 0) {
            print("deposited");

            // If there is no item currently in the slot
            if (item == null || quantity == 0) {
                SetItem(MouseTracker.GetItem()); // Set the item of the slot to the item attached to the mouse
                SetQuantity(MouseTracker.GetQuantity()); // Set the quantity of the item
                MouseTracker.RemoveItem(); // Remove the item from the mouse

                // If the item has the same name and has not reached its max quantity
            } else if (item.GetName() == MouseTracker.GetItem().GetName() && quantity < item.GetMaxQuantity()) {
                // Increment the target slot's quantity & set the mouse item quantity to the spillover amount
                MouseTracker.SetQuantity(IncrementQuantity(MouseTracker.GetQuantity()));

                // If there is no spillover, remove the item from the mouse
                if (MouseTracker.GetQuantity() == 0) {
                    MouseTracker.RemoveItem();
                }
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData) {
        if (item != null) {
            MouseTracker.GetDescriptionTextBox().text = item.GetDescription();
        } else {
            MouseTracker.GetDescriptionTextBox().text = "";
        }
    }

    public void OnPointerExit(PointerEventData eventData) {
        MouseTracker.GetDescriptionTextBox().text = "";
        // print("exit!");
    }
}