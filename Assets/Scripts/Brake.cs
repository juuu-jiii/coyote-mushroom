using UnityEngine;

/// <summary>
/// Defines behaviour for a single brake disc.
/// </summary>
public class Brake : MonoBehaviour
{
    [Tooltip("The wheel this brake disc affects.")]
    [SerializeField] private Wheel wheel;

    /// <summary>
    /// The amount of torque acting opposite to the wheel's torque under regular braking.
    /// </summary>
    public float brakeTorque;

    /// <summary>
    /// The amount of torque acting opposite to the wheel's torque as a result of engaging the handbrake.
    /// </summary>
    public float handbrakeTorque;

    /// <summary>
    /// The sum of brake and handbrake torque applied.
    /// </summary>
    public float combinedTorque;

    /// <summary>
    /// Angular acceleration computed from the combined braking torque. 
    /// </summary>
    private float angularAcceleration;

    /// <summary>
    /// The angular velocity of the wheel from the previous frame.
    /// </summary>
    private float prevWheelAngularVelocity;

    /// <summary>
    /// Applies the combined torque from applying the brake and handbrake to
    /// reduce the angular velocity of the affected wheel. 
    /// </summary>
    public void ApplyBrakeTorque()
    {
        // torque / inertia = angular acceleration
        // Invert the sign of the wheel's angular velocity because the brake's
        // angular acceleration acts in the direction opposite to that of the
        // wheel's rotation. 
        angularAcceleration = combinedTorque / wheel.inertia * -Mathf.Sign(wheel.AngularVelocity);

        // Apply the angular acceleration of braking to the affected wheel's 
        // angular velocity.
        wheel.AngularVelocity += angularAcceleration * Time.deltaTime;

        // Prevent jittery movement resulting from applying brake torque while the
        // vehicle is at a standstill.
        // Compare the sign of wheel velocity with that before braking torque was
        // applied:
        // - if they are the same, the vehicle hasn't stopped; continue braking
        // - if they are different, stop applying brake torque and zero out the affected wheel's angular velocity
        // This approach results behaviour where wheels can lock up under heavy braking.
        if (Mathf.Sign(wheel.AngularVelocity) != Mathf.Sign(prevWheelAngularVelocity))
        {
            // Debug.Log("wheelAngularVelocity: " + wheel.AngularVelocity);
            // Debug.Log("prevWheelAngularVelocity: " + prevWheelAngularVelocity);
            wheel.AngularVelocity = 0f;
        }
        // else Debug.Log(wheel.AngularVelocity);

        prevWheelAngularVelocity = wheel.AngularVelocity;
    }
}
