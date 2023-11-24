using System.Collections.ObjectModel;
using UnityEngine;

/// <summary>
/// Calculations responsible for connecting the gearbox to the wheels.
/// </summary>
public class Transmission : MonoBehaviour
{
    [Header("Vehicle Component Dependencies")]
    [Tooltip("Reference to this vehicle's engine script.")]
    [SerializeField] private Engine engine;
    [Tooltip("Reference to this vehicle's gearbox script.")]
    [SerializeField] private Gearbox gearbox;
    [Tooltip("Reference to this vehicle's rigid body.")]
    [SerializeField] private Rigidbody vehicleRb;
    [Tooltip("Animation curve script dictating the relationship between the clutch coefficient and the dot product of the vehicle's forward and velocity vectors.")]
    [SerializeField] private VehicleController vehicleController;

    [Header("Transmission Data")]
    [Tooltip("This vehicle's drivetrain type.")]
    [SerializeField] private DriveType driveType;
    [Tooltip("Ratio of torque supplied to the front wheels. Used when Drive Type is set to AWD.")]
    [SerializeField][Range(0, 1)] private float torqueBias;
    [Tooltip("Minimum possible value that the clutch coefficient can have.")]
    [SerializeField] private float clutchCoefficientMin;
    [Tooltip("Maximum possible value that the clutch coefficient can have.")]
    [SerializeField] private float clutchCoefficientMax;

    [Header("Clutch Coefficient Data")]
    [Tooltip("The Rigidbody component of the vehicle GameObject.")]
    [SerializeField] private ClutchCoefficientLerpCurve clutchCoefficientLerpCurve;

    /// <summary>
    /// Defines supported transmission types.
    /// </summary>
    private enum DriveType
    {
        FWD,
        RWD,
        AWD
    }

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

    /// <summary>
    /// Angular velocity of the drive axle. If there are multiple drive axles, 
    /// this stores their average angular velocity.
    /// </summary>
    private float driveAxleAngularVelocity;

    /// <summary>
    /// The angular velocity of the clutch disc on the gearbox side of the transmission.
    /// </summary>
    private float gearboxSideClutchAngularVelocity;

    /// <summary>
    /// The difference in angular velocities between the clutch discs on the engine and
    /// gearbox sides of the transmission.
    /// </summary>
    private float clutchAngularVelocityDifference;

    /// <summary>
    /// Scales the amount by which angular velocity differences between gearbox- and engine-
    /// side clutch discs affect engine acceleration/braking.
    /// </summary>
    public float ClutchCoefficient { get; private set; }

    /// <summary>
    /// Computes the angular velocity of the drive axle(s) as the average velocities of all driven wheels.
    /// </summary>
    public void CalculateDriveAxleAngularVelocity()
    {
        switch (driveType)
        {
            case DriveType.FWD:
                driveAxleAngularVelocity =
                    (wheels[0].AngularVelocity
                    + wheels[1].AngularVelocity)
                    * 0.5f;
                break;
            case DriveType.RWD:
                driveAxleAngularVelocity =
                    (wheels[2].AngularVelocity
                    + wheels[3].AngularVelocity)
                    * 0.5f;
                break;
            case DriveType.AWD:
                driveAxleAngularVelocity =
                    (wheels[0].AngularVelocity
                    + wheels[1].AngularVelocity
                    + wheels[2].AngularVelocity
                    + wheels[3].AngularVelocity)
                    * 0.25f;
                break;

        }
    }

    /// <summary>
    /// Calculates the angular velocity of the clutch disc on the gearbox side of the transmission.
    /// </summary>
    private void CalculateGearboxSideClutchAngularVelocity()
    {
        gearboxSideClutchAngularVelocity = driveAxleAngularVelocity * gearbox.TotalGearRatio;
    }

    /// <summary>
    /// Calculates the value of the clutch coefficient this frame.
    /// </summary>
    private void CalculateClutchCoefficient()
    {
        // Check how similar the vehicle's (normalised) forward and velocity vectors are.
        float velocityDotDirection = Mathf.Abs(Vector3.Dot(transform.forward, vehicleRb.velocity.normalized));

        // Use the value obtained above to lookup the clutch coefficient's lerp t value.
        // This is NOT a linear relationship; rather, the clutch coefficient equals 1 when
        // the dot product returns 1, and 0 for all dot product values <= 0.8
        float t = clutchCoefficientLerpCurve.curve.Evaluate(velocityDotDirection);

        // Use the t value to lerp between min and max values for the clutch coefficient.
        ClutchCoefficient = Mathf.Lerp(clutchCoefficientMin, clutchCoefficientMax, t);
    }

    /// <summary>
    /// Calculates the effect of the wheel angular velocity on the engine's angular 
    /// velocity this frame.
    /// </summary>
    public void AccelerateOrBrakeEngine()
    {
        CalculateGearboxSideClutchAngularVelocity();
        CalculateClutchCoefficient();

        // TODO: move constants into a math utils class
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
                engine.AngularVelocity + ClutchCoefficient * clutchAngularVelocityDifference,
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

    /// <summary>
    /// Initialises the transmission of this vehicle.
    /// </summary>
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
}
