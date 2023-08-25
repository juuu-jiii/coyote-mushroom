using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Engine : MonoBehaviour
{
    [Header("Curve")]
    [Tooltip("An Animation Curve that is used to visualise this vehicle's torque curve.")]
    [SerializeField] private AnimationCurve torqueCurve;

    [Header("Engine Specs")]
    [Tooltip("The lowest possible value the engine's revs can dip to i.e., when the engine is idle.")]
    [SerializeField] private float idleRpm;
    [Tooltip("The highest possible value the engine's revs can hit i.e., when the engine is redlining.")]
    [SerializeField] private float maxRpm;
    [Tooltip("The engine's inertia. The smaller and lighter an engine, the lower the inertia. For passenger cars, engine inertia typically sits between 0.2 and 0.35.")]
    [SerializeField] private float inertia;
    [Tooltip("???")] // TODO: fill this in
    [SerializeField] private float backTorque;

    /// <summary>
    /// The current RPM of the engine.
    /// </summary>
    public float CurrentRpm { get; private set; }

    /// <summary>
    /// The current torque of the engine.
    /// </summary>
    public float CurrentTorque { get; private set; }

    /// <summary>
    /// Player input value for the throttle keybind, in range [-1, 1].
    /// </summary>
    public float ThrottleInput { get; set; }

    /// <summary>
    /// Get-only property for idleRpm.
    /// </summary>
    public float IdleRpm { get { return idleRpm; } }

    /// <summary>
    /// Get-only property for maxRpm.
    /// </summary>
    public float MaxRpm { get { return maxRpm; } }

    /// <summary>
    /// The current angular acceleration of the engine.
    /// </summary>
    private float angularAcceleration;

    /// <summary>
    /// The current angular velocity of the engine.
    /// </summary>
    private float angularVelocity;

    /// <summary>
    /// Conversion constant for moving between rpm and rad/s.
    /// </summary>
    private const float RpmToRadPerSec = Mathf.PI * 2 / 60;

    /// <summary>
    /// Conversion constant for moving between rad/s and rpm.
    /// </summary>
    private const float RadPerSecToRpm = 1 / RpmToRadPerSec;

    /// <summary>
    /// Simple method of approximating engine RPM. Ignores angular acceleration/velocity.
    /// </summary>
    /// <returns>
    /// Approximation of engine RPM.
    /// </returns>
    float CalculateRpmSimple() {
        return Mathf.Clamp(
            // TODO: find out how lerp works and how it applies to this implementation.
            CurrentRpm + Mathf.Lerp(
                -3000 * Time.deltaTime,
                5000 * Time.deltaTime,
                ThrottleInput
            ),
            idleRpm,
            maxRpm
        );
    }

    /// <summary>
    /// Simple method of approximating engine torque. Ignores back torque.
    /// </summary>
    /// <returns></returns>
    float CalculateTorqueSimple()
    {
        return torqueCurve.Evaluate(CurrentRpm) * ThrottleInput;
    }

    /// <summary>
    /// A more detailed implementation of RPM and torque calculations, 
    /// accounting for values the "simpler" methods ignore.
    /// </summary>
    void CalculateTorqueAndRpmComplex()
    {
        // TODO: find out how lerp works and how it applies to this implementation.
        // Adjust the index ThrottleInput is raised to to alter throttle responsiveness.
        // Try values 2 and 0.75, then play around with others to see what works.
        CurrentTorque = Mathf.Lerp(
            backTorque,
            torqueCurve.Evaluate(CurrentRpm) * ThrottleInput,
            Mathf.Pow(ThrottleInput, 1)
        );

        // Applying formula: M = iota * alpha --> alpha = M / iota
        angularAcceleration = CurrentTorque / inertia;

        // Applying formula: w1 = w0 + alpha * deltaTime
        // Clamp values between idle/maxRpm (converted to rad/s) to prevent 
        // revs from going out of range.
        angularVelocity = Mathf.Clamp(
            angularVelocity + angularAcceleration * Time.deltaTime,
            idleRpm * RpmToRadPerSec,
            maxRpm * RpmToRadPerSec
        );

        // angularVelocity stores the RPM of the engine in rad/s. Multiply by
        // RadPerSecToRpm to convert to the correct units.
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
