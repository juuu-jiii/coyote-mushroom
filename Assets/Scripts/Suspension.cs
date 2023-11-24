using UnityEngine;

/// <summary>
/// Logic determining suspension behaviour.
/// All initialisation/update/fixedUpdate methods called from Wheel class.
/// This is so suspension calculations are done before tyre calculations.
/// </summary>
public class Suspension : MonoBehaviour
{
    [Tooltip("The length of the spring at rest.")]
    [SerializeField] private float restLength;
    [Tooltip("Controls how much the spring compresses and expands (and therefore how far the wheel moves up and down).")]
    [SerializeField] private float springTravel;
    [Tooltip("Stiffness of the spring. The higher the value, the more upward (spring) force gets generated to push the vehicle off the ground.")]
    [SerializeField] private float springStiffness;
    [Tooltip("Stiffness of the damper. The higher the value, the larger the force acting in the direction opposite that of the spring force.")]
    [SerializeField] private float damperStiffness;
    [Tooltip("Minimum possible value SpringForce can store.")]
    [SerializeField] private float minForce;
    [Tooltip("Maximum possible value SpringForce can store.")]
    [SerializeField] private float maxForce;
    [Tooltip("The script of the Wheel object this spring is attached to.")]
    [SerializeField] private Wheel wheel;

    /// <summary>
    /// Public getter for restLength.
    /// </summary>
    public float RestLength { get { return restLength; } }

    /// <summary>
    /// Public getter for springTravel.
    /// </summary>
    public float SpringTravel { get { return springTravel; } }

    /// <summary>
    /// Public getter for springStiffness.
    /// </summary>
    public float SpringStiffness { get { return springStiffness; } }

    /// <summary>
    /// Public getter for damperStiffness.
    /// </summary>
    public float DamperStiffness { get { return damperStiffness; } }

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
    public float CurrSpringLength { get; private set; }

    /// <summary>
    /// The upward force that is generated to push the vehicle off the ground, against gravity.
    /// </summary>
    public float SpringForce { get; private set; }

    /// <summary>
    /// Change in spring length over this frame and the last.
    /// </summary>
    public float SpringVelocity { get; private set; }

    /// <summary>
    /// The force that counteracts springForce to prevent the vehicle from bouncing uncontrollably.
    /// </summary>
    public float DamperForce { get; private set; }

    /// <summary>
    /// The resultant force that pushes the vehicle up, above the ground.
    /// </summary>
    public Vector3 SuspensionForce { get; private set; }

    /// <summary>
    /// The GameObject containing the mesh for the wheel this spring is attached to.
    /// </summary>
    private GameObject wheelMesh;

    /// <summary>
    /// The upward force to be applied to the wheel.
    /// </summary>
    public float fY;

    /// <summary>
    /// Whether the wheel this spring is attached to is making contact with the ground this frame.
    /// </summary>
    public bool OnGround { get; private set; }

    /// <summary>
    /// The location on the ground hit by this raycast suspension spring.
    /// </summary>
    private RaycastHit hit;

    /// <summary>
    /// Public getter for the hit data returned from the suspension raycast.
    /// </summary>
    public RaycastHit GroundHit => hit;

    /// <summary>
    /// Initialises suspension for this vehicle.
    /// </summary>
    public void Init()
    {
        // Range of values for spring length is restLength +- springTravel.
        minLength = restLength - springTravel;
        maxLength = restLength + springTravel;

        wheelMesh = wheel.WheelMesh;
    }

    /// <summary>
    /// Adjusts this wheel's local y-position based on calculated suspension values.
    /// </summary>
    public void UpdateWheelPosition()
    {
        // Update the wheel mesh's position based on the length of the suspension.
        // The value of y increases as we go upwards, and decreases as we go downwards.
        // This is why we need to negate currSpringLength to ensure the wheels do not
        // end up on top of the car's chassis instead of below.
        // Additionally, since the wheel mesh is a child of the GameObject this
        // script is attached to, animating their rotation as the car steers is
        // automatically handled.
        wheelMesh.transform.localPosition = new Vector3(
            wheelMesh.transform.localPosition.x,
            -CurrSpringLength,
            wheelMesh.transform.localPosition.z);

        // TODO: refactor this into its own script
        #region Visualising suspension length and wheel radius by drawing rays
        // Visualise the suspension length and wheel radius by drawing rays.
        // The ray from DrawRay() is drawn from start to start + dir in world coordinates.

        // // Suspension length
        // Debug.DrawRay(
        //     transform.position,
        //     -transform.up * CurrSpringLength,
        //     Color.green);

        // // Wheel radius
        // Debug.DrawRay(
        //     transform.position + (-transform.up * CurrSpringLength),
        //     -transform.up * wheelRadius,
        //     Color.magenta);

        // Alternatively, to make the ray reach the ground in one call to DrawRay():
        // Debug.DrawRay(transform.position, -transform.up * (currSpringLength + wheelRadius), Color.green);
        #endregion;
    }

    /// <summary>
    /// Acts as the suspension by casting a ray downwards to determine whether this 
    /// wheel is touching the ground.
    /// </summary>
    private void DetermineIfOnGround()
    {
        OnGround = Physics.Raycast(transform.position, -transform.up, out hit, maxLength + wheel.Radius);
    }

    /// <summary>
    /// Calculates the length of this spring based on the position of the wheel this 
    /// script is attached to as it makes contact with the ground.
    /// </summary>
    private void CalculateLengthOnGround()
    {
        // ======================================================================================
        // ============================ SUSPENSION CALCULATIONS =================================
        // ======================================================================================
        // Store the length of the spring during the previous frame.
        prevSpringLength = CurrSpringLength;

        // wheelRadius is a constant that has been set in the Inspector.
        // hit.distance = hit.point - raycast origin
        CurrSpringLength = hit.distance - wheel.Radius;

        // Do not let the spring length exceed minLength or maxLength.
        CurrSpringLength = Mathf.Clamp(CurrSpringLength, minLength, maxLength);
    }

    /// <summary>
    /// Sets the length of this spring to its maximum. Assumes this wheel is not 
    /// touching the ground.
    /// </summary>
    private void CalculateLengthInAir()
    {
        CurrSpringLength = maxLength;
    }

    /// <summary>
    /// Calculates the forces exerted by this spring during the current frame.
    /// </summary>
    private void CalculateForce()
    {
        // Measure the change in the spring's length over a fixed duration. 
        // In this case, the duration is the time between this frame and the last.
        // Use fixedDeltaTime since physics calculations are taking place in FixedUpdate.
        SpringVelocity = (prevSpringLength - CurrSpringLength) / Time.fixedDeltaTime;

        // Apply formulae to calculate spring and damper forces.
        SpringForce = Mathf.Clamp(springStiffness * (restLength - CurrSpringLength), minForce, maxForce);
        DamperForce = damperStiffness * SpringVelocity;
        fY = SpringForce + DamperForce;

        // Add spring and damper forces together to obtain a resultant force
        // for the spring this frame.
        // The raycast is pointing downwards, but the resultant force acts
        // upwards to push the vehicle off the ground. Hence, multiply 
        // springForce by transform.up to ensure the force points upwards.
        SuspensionForce = fY * transform.up;
    }

    /// <summary>
    /// Updates spring physics.
    /// </summary>
    public void FixedUpdatePhysics()
    {
        DetermineIfOnGround();

        // Vehicle is on the ground. Perform suspension physics calculations.
        if (OnGround)
        {
            CalculateLengthOnGround();
            CalculateForce();
        }
        // Vehicle is airborne. Max out suspension length.
        else
        {
            CalculateLengthInAir();
        }
    }
}
