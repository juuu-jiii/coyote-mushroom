using System.Collections.ObjectModel;
using UnityEngine;

/// <summary>
/// This is where vehicle data will be stored, including:
/// - wheelbase
/// - vehicle specs
/// - gear ratios
/// - torque curve of the engine
/// - etc.
/// </summary>
public class VehicleController : MonoBehaviour
{
    /*
    * Steering forces are only added to the front wheels. Therefore, the 
    * Ackermann angle steering calculations will only apply to them. Here, we
    * calculate the Ackermann angles for both right and left front wheels.
    * The results of these calculations then get passed to the Wheel scripts.
    */

    [Tooltip("Array tracking all wheels of this vehicle.")]
    [SerializeField] private Wheel[] wheels;

    [Tooltip("The GameObject referencing the body geometry of the vehicle.")]
    [SerializeField] private GameObject body;

    [Header("Vehicle Specs")]
    [SerializeField] private float wheelBase; // in m
    [SerializeField] private float rearTrack; // in m
    [Tooltip("The minimum dimension of available space required for the vehicle to make a semi-circular U-turn without skidding, in metres. Also known as a turning circle.")]
    [SerializeField] private float turningRadius;

    /// <summary>
    /// Serves as an encapsulation container for the wheels array.
    /// Prefer ReadOnlyCollection<T> over IEnumerable<T> as the former has 
    /// readonly indexing, whereas the latter does not have indexing at all.
    /// https://learn.microsoft.com/en-us/dotnet/api/system.collections.objectmodel.readonlycollection-1?view=net-7.0
    /// </summary>
    private ReadOnlyCollection<Wheel> readonlyWheels;

    /// <summary>
    /// Public getter for readonlyWheels.
    /// </summary>
    public ReadOnlyCollection<Wheel> Wheels
    {
        get
        {
            if (readonlyWheels == null)
                readonlyWheels = new ReadOnlyCollection<Wheel>(wheels);

            return readonlyWheels;
        }
    }

    /// <summary>
    /// Input from the player controller.
    /// </summary>
    private float steerInput;

    /// <summary>
    /// Ackermann angle for the front right wheel.
    /// </summary>
    private float ackermannAngleLeft;

    /// <summary>
    /// Ackermann angle for the front right wheel.
    /// </summary>
    private float ackermannAngleRight;

    /// <summary>
    /// </summary>

    {
        if (isBodyRendered = !isBodyRendered) body.SetActive(true);
        else body.SetActive(false);
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        // Render the vehicle's body.
        if (Input.GetKeyDown(KeyCode.Backspace)) body.SetActive(!body.activeInHierarchy);

        steerInput = Input.GetAxis("Horizontal");

        // ======================================================================================
        // ============================= STEERING CALCULATIONS ==================================
        // ======================================================================================

        // Perform Ackermann angle steering calculations for both front wheels separately.
        // Handle the cases where the vehicle is steering right, steering left, and not steering at all.

        // Steering right
        if (steerInput > 0)
        {
            ackermannAngleLeft = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turningRadius + rearTrack / 2)) * steerInput;
            ackermannAngleRight = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turningRadius - rearTrack / 2)) * steerInput;
        }
        // Steering left
        else if (steerInput < 0)
        {
            ackermannAngleLeft = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turningRadius - rearTrack / 2)) * steerInput;
            ackermannAngleRight = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turningRadius + rearTrack / 2)) * steerInput;
        }
        // Not steering i.e., steerInput == 0
        else
        {
            ackermannAngleLeft = ackermannAngleRight = 0;
        }

        // Pass the Ackermann angle calculation results to the appropriate wheels.
        foreach (Wheel w in wheels)
        {
            switch (w.WheelPos)
            {
                case WheelPosition.FrontLeft:
                    w.SteerAngle = ackermannAngleLeft;
                    break;
                case WheelPosition.FrontRight:
                    w.SteerAngle = ackermannAngleRight;
                    break;
                default:
                    break;
            }
        }
    }
}
