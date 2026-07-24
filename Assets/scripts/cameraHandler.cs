using UnityEngine;

public class cameraHandler : MonoBehaviour
{
    [Header("Basic Camera Settings")]
    [SerializeField]
    private Vector2 cameraSensitivity;
    [SerializeField]
    private int minCameraYAngle, maxCameraYAngle;

    [Space(15)]
    [Header("object Configurations")]
    [SerializeField]
    private GameObject playerCam;
    [SerializeField]
    private Rigidbody playerRigidbody;

    private float cameraYAngle = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //lock and hide the cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //rotate body based on X
        Vector3 currentPlayerAngle = playerRigidbody.rotation.eulerAngles;
        playerRigidbody.rotation = Quaternion.Euler(currentPlayerAngle.x, currentPlayerAngle.y + (Input.GetAxis("Mouse X") * cameraSensitivity.x * Time.deltaTime), currentPlayerAngle.z);
        //rotate camera based on y
        cameraYAngle -= (Input.GetAxis("Mouse Y") * Time.deltaTime * cameraSensitivity.y);
        cameraYAngle = Mathf.Clamp(cameraYAngle, minCameraYAngle, maxCameraYAngle);
        playerCam.transform.localRotation = Quaternion.Euler(cameraYAngle, 0, 0);
    }
}
