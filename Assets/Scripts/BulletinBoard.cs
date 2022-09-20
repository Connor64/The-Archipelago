using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletinBoard : InteractableObject {
    [TextArea(10, 15)]
    [SerializeField]
    private string text;
    private MenuController menuController;
    private bool inProximity = false;
    public Quest quest;

    [HideInInspector]
    public bool questComplete = false;

    protected override void Start() {
        base.Start();

        menuController = GameObject.Find("Canvas").GetComponent<MenuController>();
    }

    // Update is called once per frame
    protected override void Update() {
    }

    public override void Interact() {
        // If it's not in range, tell the player and exit
        if (!inProximity) {
            menuController.DisplayDialog("Too far away to read!");
            return;
        }

        if (quest == null) {
            menuController.DisplayDialog(text);
        } else {
            if (quest.isComplete) {
                menuController.DisplayDialog("Quest complete!");
            } else {
                menuController.DisplayQuestDialog(text, quest);
            }
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (other.tag == "Player") {
            inProximity = true;
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.tag == "Player") {
            inProximity = false;
        }
    }
}