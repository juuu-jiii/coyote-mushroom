using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Pitch- and volume-related methods for engine audio.
/// Adapted from https://forum.unity.com/threads/car-engine-sound-crossfade.1269065/#post-8056763
/// </summary>
public class EngineSound : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private float minRpm;
    [Tooltip("The RPM at which pitch for the audio source's clip is highest.")]
    [SerializeField] private float peakRpm;
    [SerializeField] private float maxRpm;
    [Tooltip("the RPM at which the engine note should be played at pitch 1 (default pitch). If the audio assets you have state the RPM of the engine, use that, otherwise you'll need to use trial and error.")]
    [SerializeField] private float pitchReferenceRpm;

    public void SetPitchFromRpm(float rpm)
    {
        // Normalise rpm based on the pitch reference value. Use the result to
        // update the pitch of the audio source.
        audioSource.pitch = rpm / pitchReferenceRpm;
    }

    public float GetVolumeFromRpm(float rpm)
    {
        // Do not play this sound if rpm is out of range.
        if (rpm < minRpm || rpm > maxRpm) return 0f;

        // If rpm is below peak, the audio source's clip should be pitched down.
        // Use inverse lerp to get the pitch at which it should be played at, as
        // a normalised value in the range [0, 1].
        // Use InverseLerp to "fade in" audio by passing in a = minRpm and 
        // b = peakRpm.
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

    public void SetVolume(float volume)
    {
        audioSource.volume = volume;
        audioSource.mute = volume == 0;
    }
}
