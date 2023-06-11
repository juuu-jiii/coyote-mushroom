using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This is where vehicle data will be stored, including:
/// - wheelbase
/// - vehicle specs
/// - gear ratios
/// - torque curve of the engine
/// - etc.
/// </summary>

public class VehicleController : MonoBehaviour
{
    /*
    * Steering forces are only added to the front wheels. Therefore, the 
    * Ackermann angle steering calculations will only apply to them. Here, we
    * calculate the Ackermann angles for both right and left front wheels.
    * The results of these calculations then get passed to the Wheel scripts.
    */

    [Tooltip("Array tracking all wheels of this vehicle.")]
    [SerializeField] private Wheel[] wheels;

    [Header("Vehicle Specs")]
    [SerializeField] private float wheelBase; // in m
    [SerializeField] private float rearTrack; // in m
    [Tooltip("The minimum dimension of available space required for the vehicle to make a semi-circular U-turn without skidding, in metres. Also known as a turning circle.")]
    [SerializeField] private float turningRadius;

    /// <summary>
    /// Input from the player controller.
    /// </summary>
    private float steerInput;

    /// <summary>
    /// Ackermann angle for the front right wheel.
    /// </summary>
    private float ackermannAngleLeft;

    /// <summary>
    /// Ackermann angle for the front right wheel.
    /// </summary>
    private float ackermannAngleRight;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        steerInput = Input.GetAxis("Horizontal");

        // ======================================================================================
        // ============================= STEERING CALCULATIONS ==================================
        // ======================================================================================

        // Perform Ackermann angle steering calculations for both front wheels separately.
        // Handle the cases where the vehicle is steering right, steering left, and not steering at all.

        // Steering right
        if (steerInput > 0)
        {
            ackermannAngleLeft = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turningRadius + rearTrack / 2)) * steerInput;
            ackermannAngleRight = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turningRadius - rearTrack / 2)) * steerInput;
        }
        // Steering left
        else if (steerInput < 0)
        {
            ackermannAngleLeft = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turningRadius - rearTrack / 2)) * steerInput;
            ackermannAngleRight = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turningRadius + rearTrack / 2)) * steerInput;
        }
        // Not steering i.e., steerInput == 0
        else
        {
            ackermannAngleLeft = ackermannAngleRight = 0;
        }

        // Pass the Ackermann angle calculation results to the appropriate wheels.
        foreach (Wheel w in wheels)
        {
            switch (w.WheelPos)
            {
                case WheelPosition.FrontLeft:
                    w.SteerAngle = ackermannAngleLeft;
                    break;
                case WheelPosition.FrontRight:
                    w.SteerAngle = ackermannAngleRight;
                    break;
                default:
                    break;
            }
        }
    }
}
