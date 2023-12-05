using UnityEngine;

public class VehicleManager : MonoBehaviour
{
    [Header("Vehicle Component Dependencies")]
    [Tooltip("Reference to this vehicle's engine script.")]
    [SerializeField] private Engine engine;
    [Tooltip("Reference to this vehicle's suspension script.")]
    [SerializeField] private Suspension[] suspension;
    [Tooltip("Reference to this vehicle's transmission script.")]
    [SerializeField] private Transmission transmission;
    [Tooltip("References to all wheels belonging to this vehicle.")]
    [SerializeField] private Wheel[] wheels;
    [Tooltip("Reference to this vehicle's rigid body.")]
    [SerializeField] private Rigidbody vehicleRb;
    [Tooltip("Reference to this vehicle's anti-roll bars.")]
    [SerializeField] private AntiRollBars antiRollBars;
    [Tooltip("Reference to this vehicle's braking system script.")]
    [SerializeField] private BrakingSystem brakingSystem;

    /// <summary>
    /// Applies upwards, sideways, and forward forces generated by a given wheel
    /// and suspension spring this frame to the vehicle rigid body at the point 
    /// which the provided spring's raycast hits the ground.
    /// </summary>
    /// <param name="suspension">
    /// The suspension spring whose upwards forces are to be applied and whose raycast 
    /// contact point will be used as the position at which to apply the forces
    /// to the vehicle's rigid body.
    /// </param>
    /// <param name="wheel">
    /// The wheel whose sideways and forward forces are to be applied.
    /// /// </param>
    private void ApplyForceToRigidbody(Suspension suspension, Wheel wheel)
    {
        if (suspension.OnGround)
        {
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
                suspension.SuspensionForce + wheel.fZ * transform.forward + wheel.fX * transform.right,
                suspension.GroundHit.point);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        foreach (Suspension s in suspension)
            s.Init();

        transmission.Init();

        foreach (Wheel w in wheels)
            w.Init();

        brakingSystem.Init();
    }

    private void Update()
    {
        // Perform visual position and rotation updates.
        for (int i = 0; i < wheels.Length; i++)
        {
            wheels[i].UpdatePitchAndYawRotation();
            suspension[i].UpdateWheelPosition();
        }
    }

    private void FixedUpdate()
    {
        // Perform physics updates.

        // 1. get drive torque
        transmission.CalculateWheelDriveTorque();

        // 2. wheel acceleration
        engine.CalculateTorqueAndRpmComplex();

        // // 3. raycast for suspension
        // // 4. update suspension length
        // // 5. get suspension force
        // // 7. update wheel location (6 is below)
        // foreach (Suspension s in suspension)
        //     s.FixedUpdatePhysics();

        // // 8. get wheel linear velocity local
        // // 9. get long slip velocity
        // // 10. get tyre force combined
        // // 11. apply tyre force
        // // 12. get friction torque
        // foreach (Wheel w in wheels)
        // {
        //     w.FixedUpdatePhysics();
        // }

        for (int i = 0; i < suspension.Length; i++)
        {
            suspension[i].FixedUpdatePhysics();
            wheels[i].FixedUpdatePhysics();
            if (antiRollBars) antiRollBars.FixedUpdatePhysics();
            ApplyForceToRigidbody(suspension[i], wheels[i]);
        }

        brakingSystem.FixedUpdatePhysics();

        // 13. get total drive velocity == CalculateDriveAxleAngularVelocity
        transmission.CalculateDriveAxleAngularVelocity();

        // 14. wheels accelerate or brake the engine
        transmission.AccelerateOrBrakeEngine();



        /*

        via FixedUpdatePhysics() method

        15. rotate wheels (visually)
        Rigid body add force at position:
            - 6. apply suspension force (skip)
        */
    }
}
