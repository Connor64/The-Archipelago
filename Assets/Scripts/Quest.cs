using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Quest", order = 2)]
public class Quest : ScriptableObject {
    [System.NonSerialized]
    public bool isComplete = false;

    public int necessaryFunds = 0;
    public string questName = "Default Quest";
    
    public GameObject spawnableObject;
    public Vector3 spawnLocation;
    public Quaternion spawnRotation;

    public void Completed() {
        if (spawnableObject != null) {
            Instantiate(spawnableObject, spawnLocation, spawnRotation);
        }

        GameObject.Find("Canvas").GetComponent<MenuController>().DisplayDialog("You completed the " + questName + " quest!");
        isComplete = true;
    }
}