using System;
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

    [Header("Component Scripts")]
    [Tooltip("Array tracking all wheels (scripts) of this vehicle.")]
    [SerializeField] private Wheel[] wheels;
    [Tooltip("Array tracking all suspension scripts of this vehicle.")]
    [SerializeField] private Suspension[] suspensions;
    [Tooltip("This vehicle's engine script.")]
    [SerializeField] private Engine engine;
    [Tooltip("The vehicle's gearbox script.")]
    [SerializeField] private Gearbox gearbox;
    [Tooltip("The vehicle's braking system script.")]
    [SerializeField] private BrakingSystem brakingSystem;

    [Header("Centre of Mass")]
    [Tooltip("The Rigidbody component of the vehicle GameObject.")]
    [SerializeField] private Rigidbody vehicleRb;
    [Tooltip("The amount by which to adjust the centre of mass automatically computed for the vehicle's rigid body.")]
    [SerializeField] private Vector3 centreOfMass;

    [Header("Debugging")]
    [Tooltip("The GameObject referencing the body geometry of the vehicle.")]
    [SerializeField] private GameObject body;
    [Tooltip("The amount by which to scale down forces acting on the wheel when visualising them.")]
    [SerializeField] private float forceScale;

    [Header("Vehicle Specs")]
    [Tooltip("The distance between the front and rear wheels of this vehicle.")]
    [SerializeField] private float wheelBase; // in m
    [Tooltip("The distance between this vehicle's rear wheels.")]
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
    /// Encapsulation container for the suspensions array.
    /// </summary>
    private ReadOnlyCollection<Suspension> readonlySuspensions;

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
    /// Public getter for readonlySuspensions.
    /// </summary>
    public ReadOnlyCollection<Suspension> Suspensions
    {
        get
        {
            if (readonlySuspensions == null)
                readonlySuspensions = new ReadOnlyCollection<Suspension>(suspensions);

            return readonlySuspensions;
        }
    }

    /// <summary>
    /// Steering input from the player controller.
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
    /// Whether to draw forces acting on the vehicle.
    /// </summary>
    private bool areForcesRendered = false;

    /// <summary>
    /// Tracks all transforms of the vehicle's wheels.
    /// </summary>
    private Transform[] wheelTransforms = new Transform[4];

    /// <summary>
    /// Draws forces acting on the vehicle.
    /// </summary>
    private void RenderForces()
    {
        // Loop through collection of all wheel gameObjects. For each, get its transform.position in world space
        // Inner loop: three iterations, 
        // O get the right/upward/forward forces (Vec3's) in object space
        // O get the wheel script for each wheel gameObject
        // O multiply each right/upward/forward vector (which are of unit length) by the corresponding x/y/z forces stored in the wheel scripts
        //      - before that though, divide the x/y/z forces by a constant (35?) because they might be too large -- check logs to see
        // O use different colours for each of the axes (rgb)
        // - Get wheel component's transform.position in world space + right/up/fwd * x/y/z (which has been divided)
        // Debug.DrawLine/Handles.DrawLine

        // Vectors for visually displaying (scaled) forces acting on this wheel.
        Vector3 scaledRight;
        Vector3 scaledUp;
        Vector3 scaledForward;
        float scaledFX;
        float scaledFY;
        float scaledFZ;

        for (int i = 0; i < wheels.Length; i++)
        {
            // Scale fX/Y/Z down for representation purposes when debugging.
            scaledFX = wheels[i].fX / forceScale;
            scaledFY = suspensions[i].fY / forceScale;
            scaledFZ = wheels[i].fZ / forceScale;

            // Get the right/upward/forward (unit) vectors of each wheel.
            // We could multiply them with fX/Y/Z directly, but their raw values
            // are so large that they would be useless for debugging purposes if
            // drawn.
            //
            // Instead, multiply each with their scaled values computed above.
            scaledRight = wheels[i].transform.right * scaledFX;
            scaledUp = wheels[i].transform.up * scaledFY;
            scaledForward = wheels[i].transform.forward * scaledFZ;

            // Draw the forces acting on each of the three (local) cardinal axes.
            Debug.DrawLine(
                wheelTransforms[i].transform.position,
                wheelTransforms[i].transform.position + scaledRight,
                Color.red);
            Debug.DrawLine(
                wheelTransforms[i].transform.position,
                wheelTransforms[i].transform.position + scaledUp,
                Color.green);
            Debug.DrawLine(
                wheelTransforms[i].transform.position,
                wheelTransforms[i].transform.position + scaledForward,
                Color.blue);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        vehicleRb.centerOfMass = centreOfMass;

        for (int i = 0; i < wheels.Length; i++)
        {
            // Get child transform of each wheel script i.e., get transform of 
            // the wheel GameObject.
            wheelTransforms[i] = wheels[i].transform.GetChild(0);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Render the vehicle's body.
        if (Input.GetKeyDown(KeyCode.Backspace)) body.SetActive(!body.activeInHierarchy);
        if (Input.GetKeyDown(KeyCode.Tab)) areForcesRendered = !areForcesRendered;

        // Draw debug forces.
        if (areForcesRendered) RenderForces();

        // Shift up/down.
        if (Input.GetKeyDown(KeyCode.LeftShift)) gearbox.GearUp();
        if (Input.GetKeyDown(KeyCode.LeftControl)) gearbox.GearDown();

        // Engage handbrake.
        brakingSystem.handbrakeInput = Convert.ToInt32(Input.GetKey(KeyCode.Space));

        // Read player input.
        steerInput = Input.GetAxis("Horizontal");

        // Split input axis handling to separately handle throttle and brake inputs.
        engine.ThrottleInput = Mathf.Clamp(Input.GetAxis("Vertical"), 0f, 1f);
        brakingSystem.brakeInput = -Mathf.Clamp(Input.GetAxis("Vertical"), -1f, 0);

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
