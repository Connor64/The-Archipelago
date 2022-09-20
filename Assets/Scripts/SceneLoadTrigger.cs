using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(SphereCollider))]
public class SceneLoadTrigger : MonoBehaviour {
    [SerializeField]
    private int sceneBuildIndex = 0;

    [SerializeField]
    private string sceneName = ""; // Leave blank to use build index

    void Start() {
        SphereCollider trigger = GetComponent<SphereCollider>();
        if (trigger == null) {
            trigger = gameObject.AddComponent<SphereCollider>();
        }
        trigger.isTrigger = true;
    }

    void OnTriggerEnter(Collider other) {
        if (sceneName != "") {
            SceneManager.LoadScene(sceneName);
        } else {
            SceneManager.LoadScene(sceneBuildIndex);
        }
    }

}