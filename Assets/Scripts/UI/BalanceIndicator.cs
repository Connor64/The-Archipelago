using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BalanceIndicator : MonoBehaviour {
    private TextMeshProUGUI textMesh;

    void Start() {
        textMesh = GetComponent<TextMeshProUGUI>();
    }

    void Update() {
        textMesh.SetText("Player Balance: $" + InventoryManager.playerBalance);
    }
}