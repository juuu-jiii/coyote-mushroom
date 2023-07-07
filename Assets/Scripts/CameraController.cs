using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Tooltip("References to all the cameras in the scene.")]
    [SerializeField] private GameObject[] cameras;
    [Tooltip("The player vehicle.")]
    [SerializeField] private GameObject vehicle;
    [Tooltip("The point of reference at which the current camera is pointed towards.")]
    [SerializeField] private GameObject cameraReferencePoint;

    /// <summary>
    /// All the possible views for the camera.
    /// </summary>
    private enum CameraViews
    {
        Chase,
        Dash,
        Orbit
    }

    /// <summary>
    /// The currently-selected camera view.
    /// </summary>
    private CameraViews currentCamera;

    /// <summary>
    /// The offset of the orbit camera from the camera reference point.
    /// </summary>
    private Vector3 orbitOffset;

    /// <summary>
    /// orbitOffset with a rotation applied. Used for the orbit camera.
    /// </summary>
    private Vector3 orbitOffsetWithAngle;

    // Start is called before the first frame update
    void Start()
    {
        // Hide cursor and fix its location.
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // "Default" camera view is chase.
        currentCamera = CameraViews.Chase;
        
        // Casting from an enum to an int is inexpensive in C# because enums 
        // are ints under the hood.
        // https://stackoverflow.com/questions/3256713/enum-and-performance
        cameras[(int)currentCamera].SetActive(true);

        // Rotate the orbit camera to look at the vehicle.
        cameras[(int)CameraViews.Orbit].transform.LookAt(cameraReferencePoint.transform.position);

        // orbitOffset is the offset of the camera FROM cameraReferencePoint.
        // This means we need the vector FROM cameraReferencePoint TO the camera.
        // Hence, we perform a vector subtraction camera - cameraReferencePoint.
        orbitOffset = cameras[(int)CameraViews.Orbit].transform.position - cameraReferencePoint.transform.position;
    }

    // LateUpdate runs after Update.
    // "For example a follow camera should always be implemented in LateUpdate 
    // because it tracks objects that might have moved inside Update."
    // https://docs.unity3d.com/ScriptReference/MonoBehaviour.LateUpdate.html
    void LateUpdate()
    {
        // Change views when 'C' is pressed.
        if (Input.GetKeyDown(KeyCode.C))
        {
            // Deactivate the current camera.
            cameras[(int)currentCamera].SetActive(false);

            // Set the current camera to point to the next one in the cameras array.
            currentCamera = (CameraViews)(((int)currentCamera + 1) % 3);

            // Then, activate it.
            cameras[(int)currentCamera].SetActive(true);
        }

        // Each camera has different behaviour logic applied to it.
        switch (currentCamera)
        {
            // Chase camera:
            // The chase camera is aimed at (and set as a child of) 
            // cameraReferencePoint. Make cameraReferencePoint adhere to the 
            // vehicle's yaw rotation, but ignore pitch and roll. This way, the
            // camera's movement is not subject to jitter caused by bumps in 
            // the road or the vehicle banking when steering, for example.
            case CameraViews.Chase:
                cameraReferencePoint.transform.rotation = Quaternion.Euler(
                    0f, 
                    vehicle.transform.eulerAngles.y, 
                    0f);
                break;
            // No unique behaviour for the dash camera. It is a child of the 
            // vehicle GameObject and follows along with its movement, subject 
            // to rotations along all three axes.
            case CameraViews.Dash:
                break;
            // The orbit camera acts like the chase camera, but circles the 
            // vehicle when the mouse is moved along the x-axis.
            case CameraViews.Orbit:
                // Remove jitter in the same way as the chase camera.
                // The orbit camera has been set as a child of cameraReference.
                cameraReferencePoint.transform.rotation = Quaternion.Euler(
                    0f, 
                    vehicle.transform.eulerAngles.y, 
                    0f);

                // Update the vector from cameraReferencePoint to the current 
                // camera each frame, so we can orbit the camera based on where
                // it currently is relative to cameraReferencePoint each time
                // the mouse is moved.
                orbitOffset = cameras[2].transform.position 
                                - cameraReferencePoint.transform.position;


                // Get the mouse movement along the x-axis.
                // There are two separate cases to handle:
                // 1. Zero mouse movement: camera only follows the vehicle.
                //      No additional logic needed.
                // 2. Non-zero mouse movement: camera follows the vehicle, but 
                //      also orbits the car in the direction (and circular
                //      displacement) specified by mouse movement.
                if (Input.GetAxis("Mouse X") != 0f)
                {
                    // Rotate orbitOffset around the y-axis by the amount the 
                    // mouse has moved.
                    // https://discussions.unity.com/t/unity-quaternion-multiplication-by-vector3/194873
                    // https://discussions.unity.com/t/multiply-quaternion-by-vector3-how-is-it-done-mathematically/59230/2
                    orbitOffsetWithAngle = Quaternion.AngleAxis(Input.GetAxis("Mouse X"), Vector3.up) 
                                            * orbitOffset;

                    // Move the orbit camera to its new position.
                    cameras[(int)currentCamera].transform.position = cameraReferencePoint.transform.position 
                                                                        + orbitOffsetWithAngle;

                    // Rotate the orbit camera to look at the vehicle.
                    cameras[(int)currentCamera].transform.LookAt(cameraReferencePoint.transform.position);
                }
                
                break;
        }
    }
}
