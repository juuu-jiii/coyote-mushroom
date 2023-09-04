using UnityEngine;

/// <summary>
/// Stores and blends multiple engine sounds together.
/// Adapted from https://forum.unity.com/threads/car-engine-sound-crossfade.1269065/#post-8056763
/// </summary>
public class EngineSoundManager : MonoBehaviour
{
    [Tooltip("The engine sounds to be blended together.")]
    [SerializeField] private EngineSound[] engineSounds;
    [Tooltip("The vehicle's engine script.")]
    [SerializeField] private Engine engine;
    [Tooltip("The master volume that all stored engine sounds must adhere to.")]
    [Range(0, 1)] public float masterVolume;

    /// <summary>
    /// The individual volumes of all stored engine sounds.
    /// </summary>
    private float[] workingVolumes;

    /// <summary>
    /// The running total of all values stored in workingVolumes.
    /// </summary>
    private float totalVolume;

    // Start is called before the first frame update
    void Start()
    {
        workingVolumes = new float[engineSounds.Length];
    }

    // Update is called once per frame
    void Update()
    {
        // Reset the running total of volumes.
        totalVolume = 0f;

        // For each engine sound, recalculate its pitch and volume based on the 
        // current engine RPM, and update totalVolume.
        for (int i = 0; i < engineSounds.Length; i++)
        {
            // if (i == 0) Debug.Log(engineSounds[i].GetVolumeFromRpm(engine.CurrentRpm));

            engineSounds[i].SetPitchFromRpm(engine.CurrentRpm);
            workingVolumes[i] = engineSounds[i].GetVolumeFromRpm(engine.CurrentRpm);
            totalVolume += workingVolumes[i];
        }

        // Debug.Log(totalVolume);

        // the total volume calculated for all engine sounds does not generally 
        // add up to 1. Divide each working volume by totalVolume to normalise, 
        // before scaling by masterVolume. This ensures consistent volume 
        // across the RPM range that sums to 1.
        if (totalVolume > 0f)
        {
            for (int i = 0; i < engineSounds.Length; i++)
            {
                engineSounds[i].SetVolume(masterVolume * workingVolumes[i] / totalVolume);
            }
        }
    }
}
