using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Engine : MonoBehaviour
{
    [SerializeField] private AnimationCurve torqueCurve;
    [SerializeField] private float idleRpm;
    [SerializeField] private float maxRpm;
    public float CurrentRpm { get; private set; }
    [SerializeField] private float inertia;
    [SerializeField] private float backTorque;
    public float CurrentTorque { get; private set; }
    public float ThrottleInput { get; set; }
    public float IdleRpm { get { return idleRpm; } }
    public float MaxRpm { get { return maxRpm; } }
    private float angularAcceleration;
    private float angularVelocity;
    private const float RpmToRadPerSec = Mathf.PI * 2 / 60;
    private const float RadPerSecToRpm = 1 / RpmToRadPerSec;

    float CalculateRpmSimple() {
        return Mathf.Clamp(
            CurrentRpm + Mathf.Lerp(
                -3000 * Time.deltaTime,
                5000 * Time.deltaTime,
                ThrottleInput
            ),
            idleRpm,
            maxRpm
        );
    }

    float CalculateTorqueSimple()
    {
        return torqueCurve.Evaluate(CurrentRpm) * ThrottleInput;
    }

    void CalculateTorqueAndRpmComplex()
    {
        CurrentTorque = Mathf.Lerp(
            backTorque,
            torqueCurve.Evaluate(CurrentRpm) * ThrottleInput,
            Mathf.Pow(ThrottleInput, 1)
        );

        angularAcceleration = CurrentTorque / inertia;

        angularVelocity = Mathf.Clamp(
            angularVelocity + angularAcceleration * Time.deltaTime,
            idleRpm * RpmToRadPerSec,
            maxRpm * RpmToRadPerSec
        );

        CurrentRpm = angularVelocity * RadPerSecToRpm;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        // Debug.Log(CurrentRpm + Mathf.Lerp(
        //         -3000 * Time.deltaTime,
        //         5000 * Time.deltaTime,
        //         ThrottleInput
        //     ));

        // CurrentRpm = CalculateRpmSimple();

        // CurrentTorque = CalculateTorqueSimple();

        CalculateTorqueAndRpmComplex();
    }
}
