using System.Collections.ObjectModel;
using UnityEngine;

/// <summary>
/// Calculations responsible for connecting the gearbox to the wheels.
/// </summary>
public class Transmission : MonoBehaviour
{
    private enum DriveType
    {
        FWD,
        RWD,
        AWD
    }

    [SerializeField] private DriveType driveType;
    [SerializeField] private Engine engine;
    [SerializeField] private Gearbox gearbox;
    [SerializeField] private VehicleController vehicleController;
    [SerializeField][Range(0, 1)] private float torqueRatioFront;

    private const int NumberOfWheels = 2;
    private const float DifferentialValue = 0.5f;

    private float[] torqueRatio = new float[NumberOfWheels];

    /// <summary>
    /// Reference to VehicleController.wheels as a readonly collection.
    /// </summary>
    private ReadOnlyCollection<Wheel> wheels;

    private void CalculateWheelDriveTorque()
    {
        for (int i = 0; i < wheels.Count; i++)
        {
            wheels[i].driveTorque = Mathf.Max(0, engine.CurrentTorque)
                                    * gearbox.TotalGearRatio
                                    * torqueRatio[i / NumberOfWheels]
                                    * DifferentialValue;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        wheels = vehicleController.Wheels;

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
                torqueRatio[0] = torqueRatioFront;
                torqueRatio[1] = 1 - torqueRatioFront;
                break;
        }
    }

    void FixedUpdate()
    {
        CalculateWheelDriveTorque();
    }
}
