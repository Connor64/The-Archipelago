using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScreen : MonoBehaviour {

    /// <summary>
    /// Loads the scene in the build order at the specified build index.
    /// </summary>
    /// <param name="buildIndex">The index of the scene in the build order</param>
    public void LoadScene(int buildIndex) {
        SceneManager.LoadScene(buildIndex);
    }

    /// <summary>
    /// Quits the game with an exit code of 0.
    /// </summary>
    public void QuitGame() {
        Application.Quit(0);
    }

    public void Start() {
        Destroy(GameObject.FindGameObjectWithTag("Player")); // Make sure the player object doesn't exist in this scene
    }
}