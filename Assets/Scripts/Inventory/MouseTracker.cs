using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MouseTracker : MonoBehaviour {
    private static Item floatingItem;
    private static int itemQuantity;
    private static Image image;
    [SerializeField]
    private TextMeshProUGUI descriptionText;
    private static TextMeshProUGUI descriptionTextStatic;

    private InventoryManager playerInventory;

    private RaycastHit containerHit; // Used to detect if the mouse is hovering over an item container
    private InteractableObject previousObject;
    [SerializeField]
    private LayerMask raycastMask;
    [SerializeField]
    private ItemManager itemManager;

    void Awake() {
        image = gameObject.AddComponent<Image>();
        image.raycastTarget = false;
        image.color = new Color(image.color.r, image.color.g, image.color.b, 0);
        image.rectTransform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

        descriptionTextStatic = descriptionText;

        playerInventory = GameObject.FindGameObjectWithTag("Player").GetComponent<InventoryManager>();

        StartCoroutine(ContainerHover());
    }

    void Update() {
        if (floatingItem != null) {
            image.transform.position = Input.mousePosition;
        }
    }

    /// <summary>
    /// Tracks the hovering of item containers to interact with them.
    /// </summary>
    /// <returns></returns>
    public IEnumerator ContainerHover() {
        while (true) {
            // If the raycast from the camera position to the point the mouse is hovering hits a collider in the raycast mask
            if (!MenuController.paused && !MenuController.inInventory
             && Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out containerHit, 100.0f, raycastMask)) {
                InteractableObject currentObject = containerHit.collider.GetComponent<InteractableObject>();
                // If the container is not null
                if (currentObject != null) {
                    // If the container was not selected last time
                    if (currentObject != previousObject) {
                        if (previousObject != null) {
                            previousObject.Hover(false);
                        }
                        currentObject.Hover(true);
                        previousObject = currentObject;
                    }

                    // If the primary mouse button is pressed while hovering a container
                    if (Input.GetMouseButtonDown(0)) {
                        currentObject.Interact();
                        print("clicked");
                    }
                }
            } else if (previousObject != null && previousObject.isHovered()) {
                // If the previously selected container is still selected
                previousObject.Hover(false);
                previousObject = null;
            }

            yield return null;
        }
    }

    private void ContainerInteract(ItemContainer container) {
        
    }

    /// <summary>
    /// Sets the item following the mouse cursor.
    /// </summary>
    /// <param name="selectedItem">The selected item</param>
    /// <param name="quantity">The amount of the item</param>
    public static void SetItem(Item selectedItem, int quantity) {
        floatingItem = selectedItem;
        itemQuantity = quantity;
        image.sprite = floatingItem.GetSprite();
        image.color = new Color(image.color.r, image.color.g, image.color.b, 1);
    }

    /// <summary>
    /// Gets the item currently attached to the mouse. Returns null if no item.
    /// </summary>
    /// <returns>The Item attached to the mouse.</returns>
    public static Item GetItem() {
        return floatingItem;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static int GetQuantity() {
        return itemQuantity;
    }

    public static void SetQuantity(int quantity) {
        if (quantity == 0) {
            RemoveItem();
        } else {
            itemQuantity = quantity;
        }
    }

    public static void RemoveItem() {
        floatingItem = null;
        itemQuantity = 0;
        image.sprite = null;
        image.color = new Color(image.color.r, image.color.g, image.color.b, 0);
    }

    public static TextMeshProUGUI GetDescriptionTextBox() {
        return descriptionTextStatic;
    }
}