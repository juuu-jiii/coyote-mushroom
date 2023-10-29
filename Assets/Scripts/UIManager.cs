using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Handles all UI logic.
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("TextMeshPro")]
    [Tooltip("The TMPro text object to log values consistent across all four wheels to on the canvas.")]
    [SerializeField] private TextMeshProUGUI consistentWheelData;
    [Tooltip("The TMPro text object to log computed values across all four wheels to on the canvas.")]
    [SerializeField] private TextMeshProUGUI computedWheelData;
    [Tooltip("The TMPro text object to log computed values for the vehicle's current torque and RPM.")]
    [SerializeField] private TextMeshProUGUI torqueAndRpm;
    [Tooltip("The slider used to visually represent the vehicle's current RPM.")]
    [SerializeField] private Slider rpmSlider;
    [Tooltip("The TMPro text object to log computed values for the vehicle's current gear.")]
    [SerializeField] private TextMeshProUGUI currentGear;

    [Header("Vehicle")]
    [Tooltip("The VehicleController GameObject associated with the vehicle whose data is to be logged to the screen.")]
    [SerializeField] private VehicleController vehicleController;

    [Tooltip("This vehicle's engine script.")]
    [SerializeField] private Engine engine;
    [Tooltip("This vehicle's gearbox script.")]
    [SerializeField] private Gearbox gearbox;

    /// <summary>
    /// Reference to VehicleController.wheels as a readonly collection.
    /// </summary>
    private ReadOnlyCollection<Wheel> wheels;

    /// <summary>
    /// Reference to VehicleController.suspensions as a readonly collection.
    /// </summary>
    private ReadOnlyCollection<Suspension> suspensions;

    // /// <summary>
    // /// Values that are consistent across all four wheels: restLength, 
    // /// springTravel, springStiffness, and damperStiffness.
    // /// </summary>
    // private string consistentWheelValues;

    // DATA TO LOG:
    // values consistent across all four wheels:
    // - restLength
    // - springTravel
    // - springStiffness
    // - damperStiffness
    // foreach wheel:
    // - wheelPos -
    // - SteerAngle -
    // - currSpringLength -
    // - springForce -
    // - springVelocity -
    // - damperForce -
    // - suspensionForce -
    // - fX/Y/Z

    // Get the array of wheels from vehicleController

    // Start is called before the first frame update
    void Start()
    {
        wheels = vehicleController.Wheels;
        suspensions = vehicleController.Suspensions;

        rpmSlider.minValue = engine.IdleRpm;
        rpmSlider.maxValue = engine.MaxRpm;

        consistentWheelData.text =
            $"restLength: {suspensions[0].RestLength}\n" +
            $"springTravel: {suspensions[0].SpringTravel}\n" +
            $"springStiffness: {suspensions[0].SpringStiffness}\n" +
            $"damperStiffness: {suspensions[0].DamperStiffness}\n";
    }

    // Update is called once per frame
    void Update()
    {
        computedWheelData.text = "";

        for (int i = 0; i < wheels.Count; i++)
        {
            computedWheelData.text +=
                // $"{wheel.WheelPos} steerAngle: {wheel.SteerAngle}\n" +
                $"{wheels[i].WheelPos} currSpringLength: {suspensions[i].CurrSpringLength}\n" +
                $"{wheels[i].WheelPos} springForce: {suspensions[i].SpringForce}\n" +
                // $"{wheel.WheelPos} springVelocity: {wheel.SpringVelocity}\n" +
                $"{wheels[i].WheelPos} damperForce: {suspensions[i].DamperForce}\n" +
                // $"{wheel.WheelPos} suspensionForce: {wheel.SuspensionForce}\n" +
                // $"{wheels[i].WheelPos} angularVelocity: {wheels[i].AngularVelocity}\n" +
                // $"{wheels[i].WheelPos} angularAcceleration: {wheels[i].AngularAcceleration}\n" +
                // $"{wheels[i].WheelPos} inertia: {wheels[i].inertia}\n" +

                // $"{wheels[i].WheelPos} maxAngularVelocity: {wheels[i].maxAngularVelocity}\n" +

                // $"{wheels[i].WheelPos} fX: {wheels[i].fX}\n" +
                $"{wheels[i].WheelPos} fY: {suspensions[i].fY}\n" +
                // $"{wheels[i].WheelPos} fZ: {wheels[i].fZ}\n" +

                // $"{wheels[i].WheelPos} lat slip norm: {wheels[i].lateralSlipNormalised}\n" +
                // $"{wheels[i].WheelPos} long slip norm: {wheels[i].longitudinalSlipNormalised}\n" +
                // $"{wheels[i].WheelPos} max friction: {wheels[i].maxFriction}\n" +
                // $"{wheels[i].WheelPos} longitudinal slip velocity: {wheels[i].longitudinalSlipVelocity}\n" +
                // $"{wheels[i].WheelPos} friction torque: {wheels[i].frictionTorque}\n" +
                // $"{wheels[i].WheelPos} angular accel of friction: {wheels[i].angularAccelerationOfFriction}\n" +
                // $"{wheels[i].WheelPos} traction/friction: {wheels[i].tractionOrFriction}\n" +
                // $"{wheels[i].WheelPos} angular acceleration: {wheels[i].AngularAcceleration}\n" +
                // $"{wheels[i].WheelPos} angular velocity: {wheels[i].AngularVelocity}\n" +
                // $"{wheels[i].WheelPos} local linear velocity Z: {wheels[i].localLinearVelocityVector.z}\n" +
                // $"{wheels[i].WheelPos} linear velocity: {wheels[i].linearVelocity}\n" +

                // $"{wheels[i].WheelPos} driveTorque: {wheels[i].torque}\n" +
                "\n";
        }

        torqueAndRpm.text =
            $"Torque: {engine.CurrentTorque}\n" +
            $"RPM: {engine.CurrentRpm}\n";

        currentGear.text = $"Current Gear: {gearbox.gears[gearbox.CurrentGear]}";

        rpmSlider.value = engine.CurrentRpm;
    }
}
