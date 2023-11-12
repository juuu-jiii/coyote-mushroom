using System;
using UnityEngine;

public class VehicleManager : MonoBehaviour
{
    public Engine engine;
    public Suspension[] suspension;
    public Transmission transmission;
    public Wheel[] wheels;
    public Rigidbody vehicleRb;

    private void ApplyForceToRigidbody(Suspension suspension, Wheel wheel)
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

    // Start is called before the first frame update
    void Start()
    {
        // TODO: add wheels to array in inspector


        foreach (Suspension s in suspension)
            s.Init();

        transmission.Init();

        foreach (Wheel w in wheels)
            w.Init();
    }

    private void Update()
    {
        for (int i = 0; i < wheels.Length; i++)
        {
            wheels[i].UpdatePositionAndRotation();
            suspension[i].UpdateWheelPosition();
        }
    }

    private void FixedUpdate()
    {
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
            ApplyForceToRigidbody(suspension[i], wheels[i]);
        }

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
