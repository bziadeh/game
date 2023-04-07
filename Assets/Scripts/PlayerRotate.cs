using UnityEngine;

public class PlayerRotate : MonoBehaviour {

    public GameObject objectToRotate;
    public PlayerMovement movementHandler;

    public float mouseSensitivity = 100.0f;
    public Transform cameraTransform;

    public float minAngle = -60.0f;
    public float maxAngle = 60.0f;

    //Rotation Value
    float yRotate = 0.0f;

    void Start() {
        // Lock cursor to the center of the screen
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update() {

        // Rotate X view
        float horizontalRotation = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        objectToRotate.transform.Rotate(0, horizontalRotation, 0);

        //Rotate Y view if 1st Person
        if (movementHandler.firstPerson)
        {
            yRotate += Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime * -1;
            yRotate = Mathf.Clamp(yRotate, minAngle, maxAngle);
            cameraTransform.eulerAngles = new Vector3(yRotate, cameraTransform.eulerAngles.y, cameraTransform.eulerAngles.z);
        }
    }
}