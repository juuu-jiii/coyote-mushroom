using UnityEngine;

/// <summary>
/// Manages all brake discs.
/// </summary>
public class BrakingSystem : MonoBehaviour
{
    [Tooltip("The amount of braking power to send to the front wheels.")]
    [SerializeField] private float brakeBias;
    [Tooltip("Coefficient that scales the strength of all brakes.")]
    [SerializeField] private float brakeStrength; // 1500
    [Tooltip("Coefficient taht scales the strength of the handbrake.")]
    [SerializeField] private float handbrakeStrength; // 1.5 (or just leave as zero)
    [Tooltip("The maximum value that braking and handbraking forces, combined, are allowed to sum to.")]
    [SerializeField] private float maxCombinedBrakeTorque; // 4000

    /// <summary>
    /// References to all brakes belonging to this vehicle.
    /// </summary>
    public Brake[] brakes;

    /// <summary>
    /// Player input value for the brake keybind, in range [0, 1].
    /// </summary>
    public float brakeInput;

    /// <summary>
    /// Player input value for the handbrake keybind, storing either 0 or 1.
    /// </summary>
    public int handbrakeInput;

    /// <summary>
    /// Stores the ratio of braking power distributed to the front and rear wheels.
    /// </summary>
    private float[] brakeRatio = new float[2];

    /// <summary>
    /// Initialises the braking system.
    /// </summary>
    public void Init()
    {
        brakeRatio[0] = brakeBias;
        brakeRatio[1] = 1f - brakeBias;
    }

    /// <summary>
    /// Calculates and applies brake torque for each brake disc.
    /// </summary>
    public void FixedUpdatePhysics()
    {
        for (int i = 0; i < brakes.Length; i++)
        {
            // The 0th index of brakeRatio applies to the front wheels.
            // The 1st index of brakeRatio applies to the rear wheels.
            brakes[i].brakeTorque = brakeInput * brakeRatio[i / 2] * brakeStrength;

            // Use (i / 2) because the handbrake only affects the rear wheels.
            // Wrap in parens to force compiler to perform int division (and not float division).
            // Since handbrakeInput stores either 0 or 1, it is used to "switch"
            // effects on or off depending on whether the handbrake is engaged.
            brakes[i].handbrakeTorque = handbrakeInput * (i / 2) * brakeStrength * handbrakeStrength;

            // Prevent the combined braking torque from exceeding the predefined maximum.
            brakes[i].combinedTorque = Mathf.Clamp(brakes[i].brakeTorque + brakes[i].handbrakeTorque, 0, maxCombinedBrakeTorque);

            // Only apply brake torque if either the brakes or handbrake are engaged.
            if (brakes[i].combinedTorque > 0f) brakes[i].ApplyBrakeTorque();
        }
    }
}
