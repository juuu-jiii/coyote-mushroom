using UnityEngine;

public class Brake : MonoBehaviour
{
    public float brakeTorque;
    public float handbrakeTorque;
    public float combinedTorque;
    public Wheel wheel;
    private float angularAcceleration;
    private float prevWheelAngularVelocity;

    public void ApplyBrakeTorque()
    {
        // torque / inertia = angular acceleration
        // Invert the sign of the wheel's angular velocity because the brake's
        // angular acceleration acts in the direction opposite to that of the
        // wheel's rotation. 

        angularAcceleration = combinedTorque / wheel.inertia * -Mathf.Sign(wheel.AngularVelocity);
        wheel.AngularVelocity += angularAcceleration * Time.deltaTime;
        if (Mathf.Sign(wheel.AngularVelocity) != Mathf.Sign(prevWheelAngularVelocity))
        {
            Debug.Log("wheelAngularVelocity: " + wheel.AngularVelocity);
            Debug.Log("prevWheelAngularVelocity: " + prevWheelAngularVelocity);
            wheel.AngularVelocity = 0f;
        }
        else Debug.Log(wheel.AngularVelocity);
        prevWheelAngularVelocity = wheel.AngularVelocity;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
