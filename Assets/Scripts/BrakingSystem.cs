using UnityEngine;

public class BrakingSystem : MonoBehaviour
{
    [Tooltip("References to all brakes belonging to this vehicle.")]
    public Brake[] brakes;
    [Range(0, 1)]
    public float brakeInput;
    public int handbrakeInput;
    public float brakeBias;
    public float brakeStrength; // 1500
    public float maxCombinedBrakeTorque; // 4000
    public float brakeTorque;
    private float handbrakeTorque;
    private float[] brakeRatio = new float[2];

    /// <summary>
    /// Coefficient applied to handbrake calculations to increase handbrake strength.
    /// </summary>
    public float handbrakeStrength; // 1.5 (or just leave as zero)

    public void Init()
    {
        brakeRatio[0] = brakeBias;
        brakeRatio[1] = 1f - brakeBias;
    }

    public void CalculateAndApplyBrakeTorque()
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

            brakes[i].combinedTorque = Mathf.Clamp(brakes[i].brakeTorque + brakes[i].handbrakeTorque, 0, maxCombinedBrakeTorque);
            if (brakes[i].combinedTorque > 0f) brakes[i].ApplyBrakeTorque();
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
