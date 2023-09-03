using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Adapted from https://forum.unity.com/threads/car-engine-sound-crossfade.1269065/#post-8056763
/// </summary>
public class EngineSoundManager : MonoBehaviour
{
    [SerializeField] private EngineSound[] engineSounds;
    [SerializeField] private Engine engine;
    [Range(0, 1)]
    [SerializeField] private float masterVolume;
    private float[] workingVolumes;
    private float totalVolume;

    // Start is called before the first frame update
    void Start()
    {
        workingVolumes = new float[engineSounds.Length];
    }

    // Update is called once per frame
    void Update()
    {
        totalVolume = 0f;

        for (int i = 0; i < engineSounds.Length; i++)
        {
            engineSounds[i].SetPitchFromRpm(engine.CurrentRpm);
            workingVolumes[i] = engineSounds[i].GetVolumeFromRpm(engine.CurrentRpm);
            totalVolume += workingVolumes[i];
        }

        if (totalVolume > 0f)
        {
            for (int i = 0; i < engineSounds.Length; i++)
            {
                engineSounds[i].SetVolume(masterVolume * workingVolumes[i] / totalVolume);
            }
        }
    }
}
