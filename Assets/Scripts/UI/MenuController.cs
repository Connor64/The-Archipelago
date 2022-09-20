using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour {
    private List<GameObject> menus;
    public Color menuColors;
    public static bool paused, inInventory;
    public int targetWidth = 1024;
    private Image menuBackground;

    private static MenuController menuInstance;

    private Quest currentQuest;

    [SerializeField]
    private GameObject mainInventoryObject, hotbarObject;

    void Awake() {
        DontDestroyOnLoad(gameObject);

        // Singleton-like behavior
        if (menuInstance == null) {
            menuInstance = this;
        } else {
            Destroy(gameObject);
            return;
        }

        print("what");

        // Ensure both flags are set to false as the game start w/ no inventory or pause menu open
        paused = false;
        inInventory = false;

        // Get all sub-menus and add them to array of gameobjects
        menus = new List<GameObject>();

        // Add menu screens
        foreach (Transform menu in transform) {
            // Only add the menu screens if they are under layer 5 ("UI") -> this omits the ItemTracker
            if (menu.gameObject.layer == 5) {
                menus.Add(menu.gameObject);
            }

            // Scale the inventory based on the target resolution vs current resolution
            float ratio = (float)Screen.width / (float)targetWidth;
            menu.localScale = new Vector3(ratio, ratio, 1);
        }

        // Add image component to canvas object for background color in menus
        menuBackground = gameObject.AddComponent<Image>();
        menuBackground.color = menuColors;

        // inventoryScaler.transform.localScale = new Vector3(ratio, ratio, 1);

        gameObject.SetActive(true);
        Resume();

        SceneManager.sceneLoaded += OnSceneLoaded; // Call this function every time a scene is loaded
    }

    /// <summary>
    /// Resets the GUI upon scene load so pause menu/inventory don't stay open.
    /// </summary>
    void OnSceneLoaded(Scene aScene, LoadSceneMode aMode) {
        Resume();
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            if (!paused) {
                AccessMenu(0); // Enable only pause menu
            } else {
                Resume();
            }
        } else if (Input.GetKeyDown(KeyCode.E)) {
            if (inInventory) {
                Resume();
            } else {
                AccessMenu(2);
                inInventory = true;
            }
        }
    }

    public void DisplayDialog(string text) {
        TextMeshProUGUI dialogText = AccessMenu(3).transform.Find("DialogText").GetComponentInChildren<TextMeshProUGUI>();
        dialogText.SetText(text);
    }

    public void DisplayQuestDialog(string text, Quest quest) {
        currentQuest = quest;

        TextMeshProUGUI dialogText = AccessMenu(4).transform.Find("DialogText").GetComponentInChildren<TextMeshProUGUI>();
        dialogText.SetText(text);
    }

    public void CheckCurrentQuest() {
        if (currentQuest == null) {
            Debug.LogError("Unable to check quest! - Null value");
        } else {
            if (InventoryManager.playerBalance >= currentQuest.necessaryFunds) {
                InventoryManager.playerBalance -= currentQuest.necessaryFunds;
                currentQuest.Completed();
            } else {
                DisplayDialog("Insufficient Funds! Come back once you have $" + currentQuest.necessaryFunds);
            }
        }
    }

    public GameObject GetMainInventory() {
        return mainInventoryObject;
    }

    public GameObject GetHotbar() {
        return hotbarObject;
    }

    public void QuitGame() {
        // Application.Quit(0);
        SceneManager.LoadScene(0); // Loads the first scene in the build order (should be the title screen)
    }

    public GameObject AccessMenu(int menuIndex) {
        for (int i = 0; i < menus.Count; i++) {
            menus[i].SetActive(i == menuIndex); // Enable only the target index
        }

        menuBackground.enabled = true; // Enable the menu background color
        Pause(true);

        return menus[menuIndex];
    }

    /// <summary>
    /// Pauses the game from a gameplay perspective (time scale is set to 0 and player cannot do anything in game world)
    /// </summary>
    /// <param name="newVal">The new value for the 'paused' boolean variable</param>
    private void Pause(bool newVal) {
        Time.timeScale = newVal ? 0 : 1;
        paused = newVal;
    }

    public void Resume() {
        Pause(false);
        inInventory = false;

        foreach (GameObject menu in menus) {
            menu.SetActive(false);
        }

        menuBackground.enabled = false; // Disable the menu background color
    }
}