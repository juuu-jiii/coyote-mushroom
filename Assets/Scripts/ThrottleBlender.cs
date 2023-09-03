using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrottleBlender : MonoBehaviour
{
    [SerializeField] private EngineSoundManager throttleOn;
    [SerializeField] private EngineSoundManager throttleOff;
    [SerializeField] private float blendFactor;
    [SerializeField] private Engine engine;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        // No blending
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

        // Blending
        if (engine.ThrottleInput > 0f)
        {
            throttleOn.masterVolume = Mathf.Min(1, throttleOn.masterVolume + blendFactor);
            throttleOff.masterVolume = Mathf.Max(0, throttleOff.masterVolume - blendFactor);
        }
        else
        {
            throttleOn.masterVolume = Mathf.Max(0, throttleOn.masterVolume - blendFactor);
            throttleOff.masterVolume = Mathf.Min(1, throttleOff.masterVolume + blendFactor);
        }
    }
}
