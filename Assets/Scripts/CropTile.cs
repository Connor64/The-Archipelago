using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CropTile : ItemContainer {
    private SpriteRenderer spriteRenderer;
    private Sprite defaultSprite;
    private Quaternion defaultRotation;
    private float cropAge;
    [HideInInspector]
    public bool grown;
    private int spriteMaxIndex;

    // Start is called before the first frame update
    protected override void Start() {
        base.Start();

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        defaultSprite = spriteRenderer.sprite;
        defaultRotation = transform.rotation;
        StartCoroutine(GrowCrop());
    }

    // Update is called once per frame
    protected override void Update() {
        if (spriteRenderer.sprite == defaultSprite) {
            transform.rotation = defaultRotation;
        } else {
            transform.LookAt(Camera.main.transform);
        }
    }

    public override void Interact() {
        if (item == null && MouseTracker.GetItem() != null) {
            int overflow = SetItem(MouseTracker.GetItem(), MouseTracker.GetQuantity());
            if (overflow != -1) {
                MouseTracker.SetQuantity(overflow);
            }
        } else if (item != null && MouseTracker.GetItem() == null) {
            int itemQuantity = 0;
            playerInventory.AddItem(RemoveItem(out itemQuantity), itemQuantity);
        }
    }

    private IEnumerator GrowCrop() {
        float timeSince = 0;
        while (true) {
            if (item != null && !grown) {
                if (!MenuController.paused) {
                    timeSince += Time.deltaTime;
                    if (timeSince >= 1.5f) {
                        cropAge += Random.Range(0.0f, 1.18f); // Every 1.5 seconds, the growth of the crop increases by a random value between 0 and 1.18

                        // The growing stage the plant is in based on its current age/maturity
                        int stage = spriteMaxIndex - Mathf.Clamp(Mathf.RoundToInt(cropAge / ((float)item.GetTimeToMaturity() / (float)item.GetSpriteCount())), 0, spriteMaxIndex);

                        spriteRenderer.sprite = item.GetSprite(stage); // Update the sprite to reflect the current stage it is in.
                        timeSince = 0;
                    }

                    // If the crop has not been just planted (bc on other thread) and is fully grown
                    if (cropAge != 0 && cropAge >= item.GetTimeToMaturity()) {
                        cropAge = 0; // Resets the crop age (not really necessary since the crop can't grow unless reset)
                        quantity += Random.Range(1, 3); // You can get 1 or 3 back (bc no seeds, acts like carrots/potatoes from Minecraft)
                        grown = true; // Makes it so the crop cannot be grown forever

                        // Highlights the specific crop green if it is grown
                        // outline.OutlineColor = Color.green;
                        // outline.OutlineWidth = 4;
                    }
                }
            }
            yield return null;
        }
    }

    public override int SetItem(Item item, int quantity) {
        // TODO: Remake these functions to not use base functions
        if (item.GetItemType() == ItemType.crop) {
            int overflow = base.SetItem(item, 1); // can only place one crop

            spriteRenderer.sprite = item.GetSprite(2);
            cropAge = 0;
            spriteMaxIndex = (this.item.GetSpriteCount() - 1);
            return quantity - 1 + overflow;
        } else {
            Debug.LogError("Invalid item type");
            return -1; // nothing was added, so full amount is returned
        }
    }

    public override Item RemoveItem(out int oldQuantity) {
        spriteRenderer.sprite = defaultSprite;
        cropAge = 0;
        grown = false;
        // outline.OutlineColor = defaultColor; // Reset the highlight color
        return base.RemoveItem(out oldQuantity);
    }
}