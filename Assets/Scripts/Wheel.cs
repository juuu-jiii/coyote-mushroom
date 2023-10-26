using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Defines the possible wheel locations in a 4-wheel vehicle.
/// </summary>
public enum WheelPosition
{
    FrontLeft,
    FrontRight,
    RearLeft,
    RearRight
}

/// <summary>
/// This is where logic determining wheel and suspension behaviour is written.
/// </summary>
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
    [Tooltip("Minimum possible value SpringForce can store.")]
    [SerializeField] private float minForce;
    [Tooltip("Maximum possible value SpringForce can store.")]
    [SerializeField] private float maxForce;

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

    [Header("Wheel")]
    [Tooltip("Whether this wheel can be steered.")]
    [SerializeField] private bool canSteer;

    [Tooltip("The position of this wheel on the vehicle.")]
    [SerializeField] private WheelPosition wheelPos;

    [Tooltip("The radius of the wheel (in m).")]
    [SerializeField] private float radius;

    [Tooltip("The mass of the wheel (in kg).")]
    [SerializeField] private float mass;

    [Tooltip("Rate at which to adjust wheelAngle such that it equals SteerAngle. The higher this value is, the quicker wheelAngle will equal SteerAngle.")]
    [SerializeField] private float steerRate;

    [Tooltip("The GameObject containing the mesh for this wheel.")]
    [SerializeField] private GameObject wheelMesh;

    [Header("Tyre")]
    [Tooltip("Measures the tyre's lateral grip capabilities. A tyre with a higher cornering stiffness produces a greater lateral acceleration for a given slip angle.")]
    [Range(0, 2)]
    [SerializeField] private float corneringStiffness;

    [Tooltip("Measures the tyre's longitudinal grip capabilities. A tyre with a higher longitudinal stiffness produces a greater longitudinal acceleration and deceleration without losing traction.")]
    [Range(0, 2)]
    [SerializeField] private float longitudinalStiffness;

    [Tooltip("Force curve for this tyre")]
    [SerializeField] private TyreForceCurve tyreForceCurve;

    [Header("Other")]
    [Tooltip("Reference to this vehicle's engine script.")]
    [SerializeField] private Engine engine;

    [Tooltip("Reference to this vehicle's gearbox script.")]
    [SerializeField] private Gearbox gearbox;

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

    /// <summary>
    /// The linear velocity vector of the vehicle (and therefore that of this 
    /// wheel), expressed in terms of this wheel's local space. This represents
    /// the "actual" velocity of the wheel.
    /// </summary>
    private Vector3 localLinearVelocityVector;

    /// <summary>
    /// The linear velocity (m/s) of this wheel, converted from its angular velocity.
    /// </summary>
    private float linearVelocity;

    /// <summary>
    /// The angular velocity (rad/s) of this wheel.
    /// </summary>
    public float AngularVelocity { get; private set; }

    /// <summary>
    /// Maximum angular velocity given the RPM of the engine and the total gear 
    /// ratio, assuming ideal conditions and ignoring energy loss.
    /// </summary>
    public float maxAngularVelocity;

    /// <summary>
    /// The angular acceleration (rad/s^2) of this wheel.
    /// </summary>
    public float AngularAcceleration { get; private set; }

    /// <summary>
    /// The delta by which to rotate this wheel (degrees) during the current frame.
    /// </summary>
    private float localDeltaWheelRotation;

    /// <summary>
    /// The torque available at this wheel.
    /// </summary>
    public float torque;

    /// <summary>
    /// The inertia of this wheel.
    /// </summary> <summary>
    /// 
    /// </summary>
    public float inertia;

    /// <summary>
    /// The longitudinal (forward) force applied to this wheel.
    /// </summary>
    public float fZ;

    /// <summary>
    /// The upward force applied to this wheel.
    /// </summary>
    public float fY;

    /// <summary>
    /// The lateral (sideways) force applied to this wheel.
    /// </summary>
    public float fX;

    /// <summary>
    /// Calculates inertia of this wheel using the formula for a closed cylinder
    /// i.e., the closest approximation to the shape of a wheel. 
    /// (I = 0.5 * m * r ^ 2)
    /// </summary>

    /// <summary>
    /// Measures the relative movement between the tangential velocity at the
    /// wheel's circumference and the actual speed of the vehicle.
    /// </summary>
    private float longitudinalSlipVelocity;

    /// <summary>
    /// Lateral slip ratio of this tyre.
    /// </summary>
    private float lateralSlipNormalised;

    /// <summary>
    /// Longitudinal slip ratio of this tyre.
    /// </summary>
    private float longitudinalSlipNormalised;

    /// <summary>
    /// Measures the grip between the tyre and the ground surface.
    /// </summary>
    private float traction;

    /// <summary>
    /// This slip vector of this wheel's tyre.
    /// </summary>
    private Vector2 slip;

    /// <summary>
    /// The magnitude of the slip vector.
    /// </summary>
    private float combinedSlip;

    /// <summary>
    /// A value from 0 to 1 representing the ratio of friction acting on this
    /// wheel's tyre.
    /// </summary>
    private float frictionForceScale;

    /// <summary>
    /// The amount of force generated by this wheel's tyre for the current 
    /// frame, normalised.
    /// </summary>
    private Vector2 tyreForceNormalised;

    /// <summary>
    /// The amount of force generated by this wheel's tyre for the current frame.
    /// </summary>
    private Vector2 tyreForce;

    /// <summary>
    /// The maximum possible friction that can act on this wheel's tyre for the 
    /// current frame.
    /// </summary>
    private float maxFriction;

    private void CalculateInertia()
    {
        inertia = 0.5f * mass * radius * radius;
    }

    /// <summary>
    /// Computes the difference between the actual speed of the vehicle and
    /// the tangential velocity at the circumference of the wheel.
    /// </summary>
    private void CalculateLongitudinalSlipVelocity(RaycastHit hit)
    {
        // Get the velocity of the vehicle's rigid body at the point where 
        // the raycast hits the ground (i.e., the bottom of the wheel).
        // Rigidbody.GetPointVelocity() returns a value in world space, so 
        // wrap that result in a Transform.InverseTransformDirection() to 
        // transform it to the local space of this wheel.
        localLinearVelocityVector = transform.InverseTransformDirection(vehicleRb.GetPointVelocity(hit.point));

        // Using formula linear velocity = angular velocity * radius
        linearVelocity = AngularAcceleration * radius;

        // Calculate the longitudinal slip velocity of this wheel:
        // Get the difference between the wheel's linear velocity (computed 
        // from its angular velocity) and the forward vector of its linear
        // velocity vector.
        longitudinalSlipVelocity = linearVelocity - localLinearVelocityVector.z;
    }

    // Start is called before the first frame update
    void Start()
    {
        // Range of values for spring length is restLength +- springTravel.
        minLength = restLength - springTravel;
        maxLength = restLength + springTravel;

        AngularVelocity = 0;
        CalculateInertia();
    }

    // Update is called once per frame
    // Perform non-physics calculations here.
    void Update()
    {
        // Address snapping issue caused by steering in one direction before 
        // quickly steering in the opposite direction.
        //
        // Use MoveTowards() to smoothly move wheelAngle (the current angle the
        // wheels are pointing in) towards SteerAngle (the angle the wheels
        // should be pointing in), using steerTime to adjust the rate at which
        // this process occurs.
        //
        // Only apply the above logic if these wheels can steer.
        //
        // This is a simpler implementation than using lerp. Lerp requires us to
        // keep track of the interpolation parameter 't', as well as the start
        // and end values to interpolate between. With MoveTowards, we always move
        // maxDelta towards any given position, removing the need to care about t.
        // We also pass currentValue in at the first parameter instead of a start
        // value, which is what lerp expects. This way, we always operate off the
        // current position, removing the need to track the start value separately.
        if (canSteer)
        {
            // In an implementation that handles axis input manually, we would
            // probably want to further adjust steerTime based on whether the
            // wheelAngle is already in the same direction as that of input
            // (checking for the sign of input against rotation is one way to do
            // this). If input is in one direction, but wheelAngle is in another,
            // that implies the player was steering in one direction but has
            // switched to another. We would need to increase steerTime 
            // temporarily for faster response.
            //
            // Another implementation we'd need to include is to auto-correct
            // steering to the centre if no steering input is received.
            //
            // See lines 249 - 265 in: 
            // https://github.com/KatVHarris/GravityInfiniteRunner/blob/master/Unity/Assets/Sample%20Assets/Vehicles/Car/Scripts/CarController.cs
            wheelAngle = Mathf.MoveTowards(wheelAngle, SteerAngle, steerRate * Time.deltaTime);
        }

        // Rotate the wheel along the y-axis the number of degrees specified 
        // by wheelAngle. Pass in wheelAngle instead of SteerAngle because 
        // wheelAngle contains the snapping-corrected value as obtained from
        // the call to Lerp() above.
        // This can also be written transform.localRotation = Quaternion.Euler(Vector3.up * wheelAngle);
        transform.localRotation = Quaternion.Euler(
            transform.localRotation.x,
            transform.localRotation.y + wheelAngle,
            transform.localRotation.z);

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

        // Calculate the wheel's local angular velocity.
        // AngularVelocity = localLinearVelocity.z / radius;

        // Animate the rotation of the wheel based on its angular velocity.
        localDeltaWheelRotation = AngularVelocity * Time.deltaTime * Mathf.Rad2Deg;

        // Add the calculated rotation to the local transform of wheelMesh
        // i.e., rotate the wheel!
        wheelMesh.transform.Rotate(
            new Vector3(
                localDeltaWheelRotation,    // pitch
                0f,                         // yaw
                0f),                        // roll
            Space.Self);                    // apply to local coordinate space

        // // This can also be done by setting the local rotation of wheelMesh to 
        // // a quaternion. An accumulator variable needs to be used to prevent the 
        // // same quaternion from being created every iteration. For example:
        // currRotationAccumulator += wheelMesh.transform.localRotation.x + localDeltaWheelRotation;
        // wheelMesh.transform.localRotation = Quaternion.Euler(
        //     currRotationAccumulator, // pitch
        //     wheelMesh.transform.localRotation.y, // yaw
        //     wheelMesh.transform.localRotation.z); // roll
    }

    // Perform physics calculations here.
    void FixedUpdate()
    {
        #region Calculate wheel angular acceleration and velocity
        AngularAcceleration = torque / inertia;
        AngularVelocity += AngularAcceleration * Time.deltaTime;

        // The formula engine.AngularVelocity / gearbox.TotalGearRatio gives
        // the maximum angular velocity a wheel can have for a given gear.
        // Explicitly handle the case where currentGear is N, else we will be
        // dividing by zero. Set it equal to a random large number instead.
        maxAngularVelocity = gearbox.TotalGearRatio == 0 ?
                                9999
                                : engine.AngularVelocity / gearbox.TotalGearRatio;

        // Do not let the wheels spin faster than the engine.
        if (gearbox.CurrentGear > 1)
        {
            AngularVelocity = Mathf.Min(
                Mathf.Abs(AngularVelocity),
                Mathf.Abs(maxAngularVelocity))
                * Mathf.Sign(maxAngularVelocity);
        }

        // AngularVelocity = gearbox.TotalGearRatio == 0 ? 0 : engine.AngularVelocity / gearbox.TotalGearRatio;
        #endregion

        // TODO: move suspension logic into separate script
        #region Calculate suspension physics when the vehicle is on the ground.
        if (Physics.Raycast(transform.position, -transform.up, out RaycastHit hit, maxLength + radius))
        {
            // ======================================================================================
            // ============================ SUSPENSION CALCULATIONS =================================
            // ======================================================================================
            // Store the length of the spring during the previous frame.
            prevSpringLength = CurrSpringLength;

            // wheelRadius is a constant that has been set in the Inspector.
            // hit.distance = hit.point - raycast origin
            CurrSpringLength = hit.distance - radius;

            // Do not let the spring length exceed minLength or maxLength.
            CurrSpringLength = Mathf.Clamp(CurrSpringLength, minLength, maxLength);

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

            // ======================================================================================
            // ==================== LATERAL/LONGITUDINAL FORCE CALCULATIONS =========================
            // ======================================================================================

            CalculateLongitudinalSlipVelocity(hit);






            lateralSlipNormalised = Mathf.Clamp(localLinearVelocityVector.x * corneringStiffness * (-1f), -1f, 1f);

            // If the wheel's linear velocity and the longitudinal slip 
            // velocity point in the same directions, perform traction 
            // calculations.
            if (localLinearVelocityVector.z * longitudinalSlipVelocity > 0f)
            {
                traction = torque / radius;
                longitudinalSlipNormalised = Mathf.Clamp(traction / Mathf.Max(fY, 0.0000001f), -2f, 2f);
            }
            // Otherwise, the wheel's linear velocity and longitudinal slip
            // velocity point in different directions. Perform friction
            // calculations.
            else
            {
                longitudinalSlipNormalised = Mathf.Clamp(
                    longitudinalSlipVelocity * longitudinalStiffness,
                    -2f,
                    2f);
            }




            // Vector representation of slip in lateral and longitudinal directions.
            slip = new Vector2(lateralSlipNormalised, longitudinalSlipNormalised);

            // Combined slip is the magnitude of the slip vector.
            combinedSlip = slip.magnitude;

            // From the Pacejka curve, lookup the friction scale factor based on the
            // current combined slip value.
            frictionForceScale = tyreForceCurve.curve.Evaluate(combinedSlip);

            // The normalised amount of force the tyres generate is equal to the 
            // direction of the slip vector (i.e., the slip vector, normalised)
            // scaled by frictionForceScale.
            // This seems to be a simplification of how tyres work. According to ChatGPT:
            // Maximum traction does not always occur at peak slip. 
            /*
                The relationship between traction and slip is more complex. It depends on 
                various factors such as the condition of the road, the tire type, the 
                vehicle's weight distribution, and the specific driving conditions.
                
                In many cases, maximum traction is achieved just before the point of peak slip, 
                especially for most conventional road surfaces and standard driving scenarios.

                However, the specific point of maximum traction can vary depending on the 
                unique characteristics of the tire and the road surface. In certain specialized 
                driving scenarios, such as off-road driving or high-performance racing, 
                the relationship between traction and slip can differ due to the specific 
                demands of the terrain or the driving conditions, and achieving maximum traction 
                may involve different strategies and considerations.
            */
            // But since the point of maximum traction and peak slip are close enough, this
            // will provide a good enough approximation.
            tyreForceNormalised = slip.normalized * frictionForceScale;

            // maxFriction = fY * frictionCoefficient, where fY is the normal reaction force
            // from the ground acting on the tyre. For now, because multiple surface types 
            // have not been implemented, we assume frictionCoefficient == 1.
            // In the event fY is negative, take 0 as maxFriction.
            maxFriction = Mathf.Max(fY, 0);

            // Finally, multiply with the maximum possible friction based on the surface
            // type the vehicle is travelling to get the total amount of force generated 
            // by the tyres this frame.
            tyreForce = tyreForceNormalised * maxFriction;

            fZ = tyreForce.y;
            fX = tyreForce.x;

            // Perform calculations for the forward and sideways forces acting 
            // on this wheel.
            // fZ is simplified for now just to provide a "proof-of-concept" so
            // the car can be driven. Moving forward grip will need to be 
            // accounted for, and so localWheelVelocity.z and a grip curve will have to come into the picture.
            // Not sure why we need to multiply by springForce, though. I think it's to ensure the spring's forces (as they change) are also taken into account when moving.
            // We need to invert on the right axis since that's the direction in which we're actually going to turn
            // Very, very valet explains that we don't have to just invert, we can invert and multiply by a value between 0 and 1 to create the impression grip --> curves
            // Right now, the handling feels off. The wheels turn the same way regardless of the car's speed. And turning when slow isn't good either.
            //      You need to make it so the car steers better at lower speeds. Either directly tie the steer angle to the speed, or do it by manipulating grip based on speed.
            // fZ = 0; // Input.GetAxis("Vertical") * SpringForce * 0.5f;

            // The following line is incorrect -- x forces are never clamped to
            // [-fY, +fY]. They can therefore exceed spring + damper force, which
            // is not possible.
            // This means you get exceptional grip, and when max turn angle is
            // reached, extreme negative values can be obtained.
            // fX = localLinearVelocity.x * SpringForce * (-1);

            // The following line is the correct way to implement the previous
            // (incorrect) example. By clamping the value of fX, we obtain more
            // "realistic" vehicle handling.
            //
            // The x-component of the vector is the direction in which the wheel
            // "doesn't like" to slide. We therefore need a force that prevents 
            // the wheel from sliding in that direction. This force is friction,
            // and can be obtained by simply negating the force in the
            // x-direction, since we don't want any of that velocity to exist in
            // order to avoid sliding sideways.
            //
            // From the equation for calculating friction force:
            // - The negation (multiplying by -1) acts as the friction coefficient.
            //   We are saying "apply 100% friction in the opposite direction of
            //   the x-component."
            // - SpringForce == N
            // - localLinearVelocity.x == slip
            // Note we are assuming full friction force in this current example.
            //
            // If we wanted, we could adjust this "negation"; multiplying by a 
            // value -1 < x < 0 removes (100 - 100x)% of the velocity in the
            // direction of the x-component, and can be used to model a loss
            // of grip due to varying surfaces, tire wear, breaking traction, 
            // etc.
            //
            // Note that we take SpringForce and not fY 
            // (which is SpringForce + DamperForce) here.
            // fX = Mathf.Clamp(SpringForce * localLinearVelocityVector.x * (-1), -fY, fY);

            // Add the force calculated above to the rigidbody.
            // vehicleRb.AddForce(force) is incorrect - it applies a 
            // force at the centre of gravity of the vehicle.
            // The force should instead be applied where the wheel is.
            // Recall that this script is applied on a per-wheel basis, and so
            // for a regular vehicle it runs for a total of four wheels. The 
            // following line of code achieves this effect for THIS wheel, by
            // applying suspensionForce at hit.point (the location which the 
            // raycast hits the ground).
            vehicleRb.AddForceAtPosition(
                SuspensionForce + fZ * transform.forward + fX * transform.right,
                hit.point);
        }
        // Otherwise, the vehicle is airborne. Max out suspension length.
        else
        {
            // Wheels above the ground, so no lat/long forces are acting upon
            // them this frame.
            fX = fZ = 0;

            CurrSpringLength = maxLength;
        }
        #endregion
    }
}
