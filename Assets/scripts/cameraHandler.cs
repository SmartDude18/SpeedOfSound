using UnityEngine;

public class cameraHandler : MonoBehaviour
{
    [field:SerializeField]
    public GameObject playerCam { get; private set; }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //lock and hide the cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        //rotate body based on X
        //rotate camera based on y
        //clamp the camera rotation to not be stupid
    }
}
