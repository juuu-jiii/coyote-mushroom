using System.Collections.ObjectModel;
using UnityEngine;

/// <summary>
/// Calculations responsible for connecting the gearbox to the wheels.
/// </summary>
public class Transmission : MonoBehaviour
{
    /// <summary>
    /// Defines supported transmission types.
    /// </summary>
    private enum DriveType
    {
        FWD,
        RWD,
        AWD
    }

    [Tooltip("This vehicle's drivetrain type.")]
    [SerializeField] private DriveType driveType;
    [Tooltip("Reference to this vehicle's engine script.")]
    [SerializeField] private Engine engine;
    [Tooltip("Reference to this vehicle's gearbox script.")]
    [SerializeField] private Gearbox gearbox;
    [Tooltip("Reference to this vehicle's controller script.")]
    [SerializeField] private VehicleController vehicleController;
    [Tooltip("Ratio of torque supplied to the front wheels. Used when Drive Type is set to AWD.")]
    [SerializeField][Range(0, 1)] private float torqueBias;

    /// <summary>
    /// There are two sets of wheels in a regular vehicle: one in front and one in the back.
    /// </summary>
    private const int SetsOfWheels = 2;

    /// <summary>
    /// Temporarily hardcoded value for the differential (currently set to fully locked).
    /// </summary>
    private const float DifferentialValue = 0.5f;

    /// <summary>
    /// Values for the ratio of torque delivered to the front and rear sets of wheels.
    /// </summary>
    private float[] torqueRatio = new float[SetsOfWheels];

    /// <summary>
    /// Reference to VehicleController.wheels as a readonly collection.
    /// </summary>
    private ReadOnlyCollection<Wheel> wheels;

    private float averageDriveAxleAngularVelocity;
    private float gearboxSideClutchAngularVelocity;
    private float clutchAngularVelocityDifference;
    public float clutchCoefficient;
    public float clutchCoefficientMin;
    public float clutchCoefficientMax;
    public Rigidbody vehicleRb;
    public ClutchCoefficientLerpCurve clutchCoefficientLerpCurve;

    /// <summary>
    /// Computes the angular velocity of the drive axle(s) as the average velocities of all driven wheels.
    /// </summary>
    public void CalculateDriveAxleAngularVelocity()
    {
        switch (driveType)
        {
            case DriveType.FWD:
                averageDriveAxleAngularVelocity =
                    (wheels[0].AngularVelocity
                    + wheels[1].AngularVelocity)
                    * 0.5f;
                break;
            case DriveType.RWD:
                averageDriveAxleAngularVelocity =
                    (wheels[2].AngularVelocity
                    + wheels[3].AngularVelocity)
                    * 0.5f;
                break;
            case DriveType.AWD:
                averageDriveAxleAngularVelocity =
                    (wheels[0].AngularVelocity
                    + wheels[1].AngularVelocity
                    + wheels[2].AngularVelocity
                    + wheels[3].AngularVelocity)
                    * 0.25f;
                break;

        }
    }

    private void CalculateGearboxSideClutchAngularVelocity()
    {
        gearboxSideClutchAngularVelocity = averageDriveAxleAngularVelocity * gearbox.TotalGearRatio;
    }

    private void CalculateClutchCoefficient()
    {
        // clutchCoefficient = 0.3f;

        // float t = Mathf.Abs(Vector3.Dot(transform.forward, vehicleRb.velocity.normalized));
        // clutchCoefficient = Mathf.Lerp(clutchCoefficientMin, clutchCoefficientMax, t);

        float velocityDotDirection = Mathf.Abs(Vector3.Dot(transform.forward, vehicleRb.velocity.normalized));
        float t = clutchCoefficientLerpCurve.curve.Evaluate(velocityDotDirection);
        clutchCoefficient = Mathf.Lerp(clutchCoefficientMin, clutchCoefficientMax, t);
    }

    public void AccelerateOrBrakeEngine()
    {
        // CalculateDriveAxleAngularVelocity(); TODO: run this outside
        CalculateGearboxSideClutchAngularVelocity();
        CalculateClutchCoefficient();

        // TODO: move constants into a math utils class
        // TODO: need to start looking into what order the various scripts and components need to be updated in within fixedUpdate
        // If gear is currently in neutral, wheels can neither accelerate nor brake engine.
        if (gearbox.CurrentGear != Gearbox.NEUTRAL_GEAR)
        {
            // Get the angular velocity difference between the clutch on the engine side vs the clutch on the gearbox
            // side. The clutch on the engine side rotates at the same rate as the engine, so its angular velocity is
            // equal to that of the engine's.
            clutchAngularVelocityDifference = gearboxSideClutchAngularVelocity - engine.AngularVelocity;

            // Calculate how much the wheels accelerate or brake the engine, but don't let the result cause the RPM
            // to rise above its maximum or dip below its minimum.
            engine.AngularVelocity = Mathf.Clamp(
                engine.AngularVelocity + clutchCoefficient * clutchAngularVelocityDifference,
                engine.IdleRpm * Engine.RPM_TO_RAD_PER_SEC,
                engine.MaxRpm * Engine.RPM_TO_RAD_PER_SEC);
        }
    }

    /// <summary>
    /// Performs torque calculations for each set of wheels.
    /// </summary>
    public void CalculateWheelDriveTorque()
    {
        // The torque available at a wheel depends primarily on the engine's torque, the product of the current drive
        // ratio and the final drive ratio. However, we must also account for the vehicle's drivetrain layout and the
        // current state of the differential.
        // This calculation does not account for transmission losses (typically 10-12%), which means for greater
        // accuracy we should multiply the following by 0.9f (transmission efficiency for 2 driven axles)
        for (int i = 0; i < wheels.Count; i++)
        {
            wheels[i].torque = Mathf.Max(0, engine.CurrentTorque)
                                    * gearbox.TotalGearRatio
                                    * torqueRatio[i / SetsOfWheels]
                                    * DifferentialValue;
        }
    }

    public void Init()
    {
        wheels = vehicleController.Wheels;

        // Assign torque ratios to each set of wheels depending on the drivetrain layout of the vehicle.
        switch (driveType)
        {
            case DriveType.FWD:
                // All power gets delivered to the front wheels.
                torqueRatio[0] = 1;
                torqueRatio[1] = 0;
                break;
            case DriveType.RWD:
                // All power gets delivered to the rear wheels.
                torqueRatio[0] = 0;
                torqueRatio[1] = 1;
                break;
            case DriveType.AWD:
                // All power gets delivered to the front wheels.
                torqueRatio[0] = torqueBias;
                torqueRatio[1] = 1 - torqueBias;
                break;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        // wheels = vehicleController.Wheels;

        // // Assign torque ratios to each set of wheels depending on the drivetrain layout of the vehicle.
        // switch (driveType)
        // {
        //     case DriveType.FWD:
        //         // All power gets delivered to the front wheels.
        //         torqueRatio[0] = 1;
        //         torqueRatio[1] = 0;
        //         break;
        //     case DriveType.RWD:
        //         // All power gets delivered to the rear wheels.
        //         torqueRatio[0] = 0;
        //         torqueRatio[1] = 1;
        //         break;
        //     case DriveType.AWD:
        //         // All power gets delivered to the front wheels.
        //         torqueRatio[0] = torqueBias;
        //         torqueRatio[1] = 1 - torqueBias;
        //         break;
        // }
    }

    void FixedUpdate()
    {
        // Debug.Log("transmission fixedUpdate");
        // CalculateWheelDriveTorque();
        // AccelerateOrBrakeEngine();
    }
}
