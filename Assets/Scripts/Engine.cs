using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Definition of engine-related specifications and behaviours.
/// </summary>
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
    [Tooltip("The amount by which to reduce RPMs when neither the accelerator or the brake are pressed aka engine braking.")]
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
    float CalculateRpmSimple()
    {
        // Evaluate how much RPM to add to/subtract from CurrentRpm based on
        // ThrottleInput. Clamp the value so it never exceeds the range 
        // [idleRpm, maxRpm].
        return Mathf.Clamp(
            // ThrottleInput stores the output of Input.GetAxis("Vertical"), so
            // it is within the range [-1, 1].
            //
            // Here, if ThrottleValue == 0.5, we get 
            // -3000dt + (5000dt - (-3000dt)) * 0.5 = 1000dt
            // and so we add 1000dt to CurrentRpm.
            //
            // Lerp returns startValue when t is outside the range [0, 1], so
            // if the decelerator is pressed, -3000dt is returned.
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
    /// <returns>
    /// The current engine torque.
    /// </returns>
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
        // Evaluate the current torque from the torque curve based on 
        // CurrentRpm and ThrottleInput.
        CurrentTorque = Mathf.Lerp(
            // First, look up the torque curve to get the maximum possible torque
            // at the current engine RPM. Maximum torque at a given RPM is obtained
            // when ThrottleInput == 1. If ThrotttleInput <= 0, then CurrentTorque
            // equals backTorque. This negative value results in a negative angular
            // velocity, causing RPMs to drop. If ThrottleInput is between 0 and 1,
            // then, logically, a value between backTorque and the max torque at
            // the given RPM is returned.
            //
            // When working with a single engine torque curve (torque versus RPM),
            // the curve plots values assuming full throttle. If the throttle is at
            // 50% i.e., halfway pressed, then, at a given RPM, only 50% of the
            // torque shown by the curve is generated. This is a simplification;
            // engines have a torque map, where a slightly different curve exists
            // for each throttle level. The single curve works nicely for a 
            // semi-realistic simulation, however.
            backTorque,
            torqueCurve.Evaluate(CurrentRpm) * ThrottleInput,

            // Depending on the throttle value, the engine output torque is a blend 
            // between backTorque and the actual engine torque. At zero throttle, 
            // the engine "brakes" the vehicle, while as throttle input increases, 
            // the engine propels the vehicle. Pow() is used to make braking 
            // prevalent when ThrottleInput is 0 or nearly 0. It has a role in 
            // determining the throttle level at which the transition between
            // back torque and actual driving torque takes place. Notice that, when
            // the interpolation parameter, t > 1, this transition point gets moved 
            // higher.
            //
            // We can therefore also say that t acts to adjust the responsiveness 
            // of the throttle.
            //
            // If you're using a keyboard, keeping this value small (< 0.5f) might 
            // be better, since the throttle key is either pressed or it isn't.
            // If you're using an actual pedal, a value of 1 maps it directly to 
            // how far the pedal is pressed down, which is probably the preferred
            // behaviour.
            Mathf.Pow(ThrottleInput, 1));

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
        // Debug.Log(Mathf.InverseLerp(100, 0, 47));
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
