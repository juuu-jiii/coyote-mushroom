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
    [SerializeField] private TextMeshProUGUI torqueAndRpm;
    [SerializeField] private Slider rpmSlider;

    [Header("Vehicle")]
    [Tooltip("The VehicleController GameObject associated with the vehicle whose data is to be logged to the screen.")]
    [SerializeField] private VehicleController vehicleController;

    [SerializeField] private Engine engine;

    /// <summary>
    /// Reference to VehicleController.wheels as a readonly collection.
    /// </summary>
    private ReadOnlyCollection<Wheel> wheels;

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

        rpmSlider.minValue = engine.IdleRpm;
        rpmSlider.maxValue = engine.MaxRpm;

        consistentWheelData.text =
            $"restLength: {wheels[0].RestLength}\n" +
            $"springTravel: {wheels[0].SpringTravel}\n" +
            $"springStiffness: {wheels[0].SpringStiffness}\n" +
            $"damperStiffness: {wheels[0].DamperStiffness}\n";
    }

    // Update is called once per frame
    void Update()
    {
        computedWheelData.text = "";

        foreach (Wheel wheel in wheels)
        {
            computedWheelData.text +=
                $"{wheel.WheelPos} steerAngle: {wheel.SteerAngle}\n" +
                $"{wheel.WheelPos} currSpringLength: {wheel.CurrSpringLength}\n" +
                $"{wheel.WheelPos} springForce: {wheel.SpringForce}\n" +
                $"{wheel.WheelPos} springVelocity: {wheel.SpringVelocity}\n" +
                $"{wheel.WheelPos} damperForce: {wheel.DamperForce}\n" +
                $"{wheel.WheelPos} suspensionForce: {wheel.SuspensionForce}\n" +
                $"{wheel.WheelPos} fX: {wheel.fX}\n" +
                $"{wheel.WheelPos} fY: {wheel.fY}\n" +
                $"{wheel.WheelPos} fZ: {wheel.fZ}\n" +
                "\n";
        }

        torqueAndRpm.text =
            $"Torque: {engine.CurrentTorque}\n" +
            $"RPM: {engine.CurrentRpm}\n";

        rpmSlider.value = engine.CurrentRpm;
    }
}
