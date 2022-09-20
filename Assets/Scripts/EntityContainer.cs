using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityContainer : MonoBehaviour {
    List<GameObject> sprites;
    Camera cam;

    void Start() {
        cam = Camera.main;
        StartCoroutine(RotateBillboards());
    }

    /// <summary>
    /// Rotate sprites under this object toward the camera (on different thread).
    /// </summary>
    IEnumerator RotateBillboards() {
        while (true) {
            for (int i = 0; i < transform.childCount; i++) {
                transform.GetChild(i).GetChild(0).LookAt(cam.transform);
            }
            yield return null;
        }
    }
}
