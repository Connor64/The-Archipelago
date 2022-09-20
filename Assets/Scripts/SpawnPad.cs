using UnityEngine;

public class SpawnPad : MonoBehaviour {
    // Start is called before the first frame update
    void Start() {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        player.transform.position = transform.position;
        player.transform.rotation = Quaternion.Euler(0, transform.rotation.y, 0);
    }
}