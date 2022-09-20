using UnityEngine;

/// <summary>
/// An item object in the game world (acts as an item "slot")
/// </summary>
public class ItemObject : MonoBehaviour {

    private Item item;
    public ItemManager itemManager;
    private SpriteRenderer spriteRenderer;
    private GameObject spriteObject;
    private Camera cam;

    void Awake() {
        spriteObject = transform.GetChild(0).gameObject;
        spriteRenderer = spriteObject.GetComponent<SpriteRenderer>();
        // SpawnCrop();
        cam = Camera.main;

        transform.parent = GameObject.FindWithTag("EntityContainer").transform;
    }

    void Update() {
        // spriteObject.transform.LookAt(cam.transform, Vector3.up);
    }

    public string GetItemName() {
        return item.GetName();
    }

    public void SpawnCrop() {
        SpawnItem(itemManager.GetCrops()[Random.Range(0, itemManager.GetCrops().Length)]);
    }

    public void SpawnItem(Item itemToSpawn) {
        if (item == null) {
            item = itemToSpawn;
            spriteRenderer.sprite = item.GetSprite();
        } else {
            Debug.LogError("ItemObject " + name + " is already a " + item.GetItemType().ToString() + ". It cannot be respawned!");
        }
    }

    public void SpawnItem(ItemType itemType) {
        SpawnItem(itemManager.GetRandomItemByType(itemType));
    }

    public Item GetItem() {
        return item;
    }
}