using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CropBed : MonoBehaviour {
    [SerializeField]
    private GameObject spriteIcon;
    [SerializeField]
    private CropTile[] cropTiles;

    private Camera cam;
    private static InventoryManager inventoryManager;

    void Start() {
        cam = Camera.main;
        spriteIcon.SetActive(false);
        if (inventoryManager == null) {
            inventoryManager = GameObject.FindGameObjectWithTag("Player").GetComponent<InventoryManager>();
        }

        StartCoroutine(CheckTiles());
    }

    void Update() {
        // If the controls icon of the crop bed is active, rotate it towards the camera to act as a billboard sprite
        if (spriteIcon.activeSelf) {
            spriteIcon.transform.LookAt(cam.transform);
        }
    }

    private IEnumerator CheckTiles() {
        float timeSince = 0;
        while (true) {
            timeSince += Time.deltaTime;
            if (timeSince > 0.5f) {
                foreach (CropTile tile in cropTiles) {
                    // If one of the tiles has a grown crop, enable indicator
                    if (tile.grown) {
                        spriteIcon.SetActive(true);
                        break;
                    }
                    // If none of the crops are grown, disable it.
                    spriteIcon.SetActive(false);
                }
            }
            yield return null;
        }
    }



    // private void OnTriggerEnter(Collider collider) {
    //     // If the player enters the range of the crop bed, enable the control icon
    //     if (collider.tag == "Player") {
    //         if (inventoryManager.targetCropBed == null) {
    //             inventoryManager.targetCropBed = this;
    //             spriteIcon.SetActive(true);
    //         }
    //     }
    // }

    // private void OnTriggerExit(Collider collider) {
    //     if (collider.tag == "Player") {
    //         if (inventoryManager.targetCropBed == this) {
    //             inventoryManager.targetCropBed = null;
    //             spriteIcon.SetActive(false);
    //         }
    //     }
    // }

}