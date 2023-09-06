using UnityEngine;

/// <summary>
/// Blends engine sounds between throttle on and off states.
/// </summary>
public class ThrottleBlender : MonoBehaviour
{
    [Tooltip("The manager of engine sounds played when the throttle is on.")]
    [SerializeField] private EngineSoundManager throttleOn;
    [Tooltip("The manager of engine sounds played when the throttle is off.")]
    [SerializeField] private EngineSoundManager throttleOff;
    [Tooltip("How quickly throttle on/off sounds are blended i.e., the crossfade duration.")]
    [SerializeField] private float blendFactor;
    [Tooltip("The vehicle's engine script.")]
    [SerializeField] private Engine engine;

    // Update is called once per frame
    void Update()
    {
        #region No blending
        // if (engine.ThrottleInput > 0f)
        // {
        //     throttleOn.masterVolume = 1f;
        //     throttleOff.masterVolume = 0f;
        // }
        // else
        // {
        //     throttleOn.masterVolume = 0f;
        //     throttleOff.masterVolume = 1f;
        // }
        #endregion

        // Blend between throttle on/off sounds.
        //
        // If throttle is pressed, gradually fade out throttle off sounds, 
        // while fading in throttle on sounds. Cap the minimum volume at 0 and 
        // maximum volume at 1 to prevent it from going out of range.
        if (engine.ThrottleInput > 0f)
        {
            throttleOn.masterVolume = Mathf.Min(1, throttleOn.masterVolume + blendFactor * Time.deltaTime);
            throttleOff.masterVolume = Mathf.Max(0, throttleOff.masterVolume - blendFactor * Time.deltaTime);
        }
        // If throttle is not pressed, gradually fade out throttle on sounds, 
        // while fading in throttle off sounds.
        else
        {
            throttleOn.masterVolume = Mathf.Max(0, throttleOn.masterVolume - blendFactor * Time.deltaTime);
            throttleOff.masterVolume = Mathf.Min(1, throttleOff.masterVolume + blendFactor * Time.deltaTime);
        }
    }
}
