using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WheelPosition
{
    FrontLeft,
    FrontRight,
    RearLeft,
    RearRight
}

public class Wheel : MonoBehaviour
{
    [Header("Rigidbody")]
    [Tooltip("The Rigidbody component of the vehicle GameObject.")]
    [SerializeField] private Rigidbody vehicleRb;

    [Header("Suspension")]
    [Tooltip("The length of the spring at rest.")]
    [SerializeField] private float restLength;
    [Tooltip("Controls how much the spring compresses and expands (and therefore how far the wheel moves up and down).")]
    [SerializeField] private float springTravel;
    [Tooltip("Stiffness of the spring. The higher the value, the more upward (spring) force gets generated to push the vehicle off the ground.")]
    [SerializeField] private float springStiffness;
    [Tooltip("Stiffness of the damper. The higher the value, the larger the force acting in the direction opposite that of the spring force.")]
    [SerializeField] private float damperStiffness;

    /// <summary>
    /// Minimum length of the spring. Used alongside maxLength to clamp springLength.
    /// </summary>
    private float minLength;

    /// <summary>
    /// Maximum length of the spring. Used alongside minLength to clamp springLength.
    /// </summary>
    private float maxLength;

    /// <summary>
    /// The length of the spring during the previous frame.
    /// </summary>
    private float prevSpringLength;

    /// <summary>
    /// The length of the spring during the current frame.
    /// </summary>
    public float currSpringLength;

    /// <summary>
    /// The upward force that is generated to push the vehicle off the ground, against gravity.
    /// </summary>
    private float springForce;

    /// <summary>
    /// Change in spring length over this frame and the last.
    /// </summary>
    private float springVelocity;

    /// <summary>
    /// The force that counteracts springForce to prevent the vehicle from bouncing uncontrollably.
    /// </summary>
    private float damperForce;

    /// <summary>
    /// The resultant force that pushes the vehicle up, above the ground.
    /// </summary>
    private Vector3 suspensionForce;

    [Header("Wheel")]
    [Tooltip("The position of this wheel on the vehicle.")]
    [SerializeField] private WheelPosition wheelPos;

    [Tooltip("The radius of the wheel.")]
    [SerializeField] private float wheelRadius;

    [Tooltip("Time it takes to lerp between wheelAngle and steerAngle. The higher this value is, the faster the lerping between the two angles, and vice versa.")]
    [SerializeField] private float steerTime;

    /// <summary>
    /// Gets the position of this wheel on the vehicle.
    /// </summary>
    public WheelPosition WheelPos { get { return wheelPos; } }

    /// <summary>
    /// The angle at which this wheel is currently steering, in degrees.
    /// </summary>
    public float SteerAngle { get; set; }

    /// <summary>
    /// Tracks the current steering angle of this wheel.
    /// </summary>
    private float wheelAngle;

    // Start is called before the first frame update
    void Start()
    {
        // Range of values for spring length is restLength +- springTravel.
        minLength = restLength - springTravel;
        maxLength = restLength + springTravel;
    }

    // Update is called once per frame
    // Perform non-physics calculations here.
    void Update()
    {
        // Address snapping issue caused by steering in one direction before 
        // quickly steering in the opposite direction.
        // Use Lerp() to smoothly rotate between wheelAngle and SteerAngle.
        wheelAngle = Mathf.Lerp(wheelAngle, SteerAngle, steerTime * Time.deltaTime);

        // Rotate the wheel along the y-axis the number of degrees specified 
        // by wheelAngle. Pass in wheelAngle instead of SteerAngle because 
        // wheelAngle contains the snapping-corrected value as obtained from
        // the call to Lerp() above.
        // This can also be written transform.localRotation = Quaternion.Euler(Vector3.up * wheelAngle);
        transform.localRotation = Quaternion.Euler(
            transform.localRotation.x, 
            transform.localRotation.y + wheelAngle, 
            transform.localRotation.z);
    }

    // Perform physics calculations here.
    void FixedUpdate()
    {
        // Calculate suspension physics when the vehicle is on the ground.
        if (Physics.Raycast(transform.position, -transform.up, out RaycastHit hit, maxLength + wheelRadius))
        {
            // Store the length of the spring during the previous frame.
            prevSpringLength = currSpringLength;

            // wheelRadius is a constant that has been set in the Inspector.
            // hit.distance = hit.point - raycast origin
            currSpringLength = hit.distance - wheelRadius;

            // Do not let the spring length exceed minLength or maxLength.
            currSpringLength = Mathf.Clamp(currSpringLength, minLength, maxLength);

            // Measure the change in the spring's length over a fixed duration. 
            // In this case, the duration is the time between this frame and the last.
            // Use fixedDeltaTime since physics calculations are taking place in FixedUpdate.
            springVelocity = (prevSpringLength - currSpringLength) / Time.fixedDeltaTime;

            // Apply formulae to calculate spring and damper forces.
            springForce = springStiffness * (restLength - currSpringLength);
            damperForce = damperStiffness * springVelocity;

            // Add spring and damper forces together to obtain a resultant force
            // for the spring this frame.
            // The raycast is pointing downwards, but the resultant force acts
            // upwards to push the vehicle off the ground. Hence, multiply 
            // springForce by transform.up to ensure the force points upwards.
            suspensionForce = (springForce + damperForce) * transform.up;

            // Add the force calculated above to the rigidbody.
            // vehicleRb.AddForce(suspensionForce) is incorrect - it applies a 
            // force at the centre of gravity of the vehicle.
            // The force should instead be applied where the wheel is.
            // Recall that this script is applied on a per-wheel basis, and so
            // for a regular vehicle it runs for a total of four wheels. The 
            // following line of code achieves this effect for THIS wheel, by
            // applying suspensionForce at hit.point (the location which the 
            // raycast hits the ground).
            vehicleRb.AddForceAtPosition(suspensionForce, hit.point);
        }
        // Otherwise, the vehicle is airborne. Max out suspension length.
        else
            currSpringLength = maxLength;
    }
}
