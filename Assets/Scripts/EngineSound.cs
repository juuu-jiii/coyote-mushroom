using UnityEngine;

/// <summary>
/// Pitch- and volume-related data and methods for a single engine sound file.
/// Adapted from https://forum.unity.com/threads/car-engine-sound-crossfade.1269065/#post-8056763
/// </summary>
public class EngineSound : MonoBehaviour
{
    [Tooltip("Audio Source containing the engine sound to be played.")]
    [SerializeField] private AudioSource audioSource;
    [Tooltip("The lowest RPM at which this engine sound can be heard.")]
    [SerializeField] private float minRpm;
    [Tooltip("The RPM at which the volume for the Audio Source's clip is maximum. For values above or below this within the range [minRpm, maxRpm], volume will be reduced proportionally.")]
    [SerializeField] private float peakRpm;
    [Tooltip("The highest RPM at which this engine sound can be heard.")]
    [SerializeField] private float maxRpm;
    [Tooltip("The RPM at which pitch for the engine sound is 1 (default pitch). For RPMs below this value, the engine sound will be pitched down. For RPMs above this value, the engine sound will be pitched up. Use the RPM values provided by the audio assets, if any. If those are not provided, use trial and error (i.e., tune by ear).")]
    [SerializeField] private float pitchReferenceRpm;

    /// <summary>
    /// Pitch the engine sound up or down based on the current RPM value 
    /// relative to that of pitchReferenceRpm.
    /// </summary>
    /// <param name="rpm">
    /// The engine's current RPM.
    /// </param>
    public void SetPitchFromRpm(float rpm)
    {
        // If rpm > pitchReferenceRpm, the engine sound is pitched up.
        // If rpm < pitchReferenceRpm, the engine sound is pitched down.
        // If rpm == pitchReferenceRpm, the division equals 1, meaning the
        // engine sound is played at default pitch.
        audioSource.pitch = rpm / pitchReferenceRpm;
    }

    /// <summary>
    /// Uses minRpm, maxRpm, and peakRpm to fade the engine sound in/out.
    /// </summary>
    /// <param name="rpm">
    /// The engine's current RPM.
    /// </param>
    /// <returns>
    /// The volume at which this engine sound should be played.
    /// </returns>
    public float GetVolumeFromRpm(float rpm)
    {
        // Do not play this sound if rpm is out of range.
        if (rpm < minRpm || rpm > maxRpm) return 0f;

        // If rpm is above/below peak, the volume of the Audio Source's clip
        // should be reduced proportionally. Use InverseLerp to get the volume
        // at which it should be played at, as a normalised value in the range 
        // [0, 1].
        //
        // This way, InverseLerp is used to "fade in/out" audio by passing in 
        // a = minRpm and b = peakRpm as reference points by which to base
        // output audio on.
        //
        // InverseLerp returns the normalised "point" rpm lies between a and b.
        // - If a < b, InverseLerp behaves as expected.
        // - If a > b, InverseLerp returns the 1 - the value returned if a and b
        //      were swapped when passed into the function.
        if (rpm < peakRpm)
            return Mathf.InverseLerp(minRpm, peakRpm, rpm);
        // Otherwise, rpm is >= peakRpm. Use InverseLerp to "fade out" audio by
        // passing in a = maxRpm and b = peakRpm.
        else
            return Mathf.InverseLerp(maxRpm, peakRpm, rpm);
    }

    /// <summary>
    /// Set the volume of the Audio Source, muting it if necessary.
    /// </summary>
    /// <param name="volume">
    /// Value to set the Audio Source's volume to.
    /// </param>
    public void SetVolume(float volume)
    {
        audioSource.volume = volume;
        audioSource.mute = volume == 0;
    }
}
