using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
    public float movementSpeed = 10;
    public float movementThreshold = 0.1f;
    private Rigidbody rb;

    Vector3 relativeCameraPosition;

    public float cameraFollowSpeed = 10.0f;
    public float cameraSensitivity = 10.0f; // Used for rotation
    public GameObject cameraTarget;
    public float cameraAngleFromPlayer = 60.0f;
    [Range(5, 35.0f)]
    public float cameraDistanceFromPlayer = 10.0f;
    public float minCameraDistance, maxCameraDistance;
    private float cameraHeight, cameraAngleXZ;

    private Vector2 previousMousePosition;

    void Awake() {
        // Limit framerate to 60 FPS
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;

        // DontDestroyOnLoad(gameObject);
    }

    void Start() {
        rb = GetComponent<Rigidbody>();
        relativeCameraPosition = Camera.main.transform.position - transform.position;
        previousMousePosition = Input.mousePosition;
        cameraHeight = Mathf.Sin(cameraAngleFromPlayer / (180 * Mathf.PI)) * cameraDistanceFromPlayer;
    }

    void Update() {
        if (!MenuController.paused) {
            CameraRotate();
        }

        float scrollWheel = Input.mouseScrollDelta.y;
        if (scrollWheel > 0 || scrollWheel < 0) {
            CameraZoom(scrollWheel);
        }

        // cameraHeight = Mathf.Sin(cameraAngleFromPlayer / (180 * Mathf.PI)) * cameraDistanceFromPlayer;

        previousMousePosition = Input.mousePosition;
    }

    void FixedUpdate() {
        if (!MenuController.paused) {
            PlayerMove();
            CameraFollow();
            Camera.main.transform.LookAt(cameraTarget.transform.position);
        }
    }


    private void PlayerMove() {
        // Get the direction that the player wants to go
        Vector3 direction = new Vector3(Mathf.Clamp(Input.GetAxis("Horizontal"), -1, 1), 0, Mathf.Clamp(Input.GetAxis("Vertical"), -1, 1));

        // If the direction isn't 0 (i.e., no input), then rotate the player to point in the direction they want to go
        if (direction != Vector3.zero) {
            float playerAngle = Mathf.Atan2(direction.x, direction.z); // Gets angle based on input from controller/keyboard

            float camAngle = cameraAngleXZ < 0 ? cameraAngleXZ + 6.28318f : cameraAngleXZ; // Add 2*PI if less than 0, converts angle to always work on unit circle
            camAngle -= 4.712385f; // Camera angle is at a 270 degree offset from input rotation, so "rotating" it by 270 (3/2 * PI) degrees corrects it (I got this just from testing)

            playerAngle = (playerAngle - camAngle) * (180 / Mathf.PI); // Subtract camera angle from input angle and convert to degrees

            transform.rotation = Quaternion.Euler(0, playerAngle, 0);
        }

        // If the magnitude of the direction is greather than the threshold (i.e., stick is pushed far enough), then push the player forward
        if (direction.magnitude > movementThreshold) {
            transform.position += transform.forward * movementSpeed * Time.fixedDeltaTime * direction.magnitude;
        }
    }

    /// <summary>
    /// Rotates the camera around player based on mouse input.
    /// </summary>
    private void CameraRotate() {
        // If the right mouse button is being held down and the game is not paused/in inventory
        if (!MenuController.paused && !MenuController.inInventory && Input.GetMouseButton(1)) {
            // Rotate around player, using difference between mouse positions as angle
            float deltaX = Input.mousePosition.x - previousMousePosition.x;
            Camera.main.transform.RotateAround(transform.position, Vector3.up, deltaX * cameraSensitivity * Time.fixedDeltaTime);
        }
    }

    /// <summary>
    /// Pulls and pushes camera based on scroll wheel input.
    /// </summary>
    /// <param="input"></param>
    private void CameraZoom(float input) {
        float newDistance = cameraDistanceFromPlayer - (input * cameraSensitivity / 2);

        cameraDistanceFromPlayer = Mathf.Clamp(newDistance, minCameraDistance, maxCameraDistance);
        
        cameraHeight = Mathf.Sin(cameraAngleFromPlayer / (180 * Mathf.PI)) * cameraDistanceFromPlayer;
    }

    /// <summary>
    /// Moves and rotates the camera toward player's current position.
    /// </summary>
    private void CameraFollow() {
        // Get distance from camera to player on X and Z axes
        float cameraToPlayerX = Camera.main.transform.position.x - transform.position.x;
        float cameraToPlayerZ = Camera.main.transform.position.z - transform.position.z;

        cameraAngleXZ = Mathf.Atan2(cameraToPlayerZ, cameraToPlayerX); // Calculate the angle between the player and camera on the XZ plane

        // Calculate the target position's distance from the player on the X and Z axes
        float targetX = Mathf.Cos(cameraAngleXZ) * cameraDistanceFromPlayer;
        float targetZ = Mathf.Sin(cameraAngleXZ) * cameraDistanceFromPlayer;

        // Lerp camera position towards the target location (player position + target displacement)
        Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, transform.position + new Vector3(targetX, cameraHeight, targetZ), Time.fixedDeltaTime * cameraFollowSpeed);
    }
}