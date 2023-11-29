using UnityEngine;

public class AntiRollBars : MonoBehaviour
{
    [SerializeField] private int antiRollBarStiffness; // 5000
    [SerializeField] private Suspension frontLeft;
    [SerializeField] private Suspension frontRight;
    [SerializeField] private Suspension rearLeft;
    [SerializeField] private Suspension rearRight;
    [SerializeField] private Rigidbody vehicleRb;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    public void FixedUpdatePhysics()
    {
        float frontSuspensionDelta = frontLeft.CurrSpringLength
                                        - frontRight.CurrSpringLength;
        float rearSuspensionDelta = rearLeft.CurrSpringLength
                                        - rearRight.CurrSpringLength;

        float frontAntiRollForce = frontSuspensionDelta
                                        * antiRollBarStiffness;
        float rearAntiRollForce = rearSuspensionDelta
                                        * antiRollBarStiffness;
        // Vector3 frontRightAntiRollForce = frontSuspensionDelta
        //                                     * antiRollBarStiffness
        //                                     * frontLeft.transform.up;
        // Vector3 rearLeftAntiRollForce = frontSuspensionDelta
        //                                     * antiRollBarStiffness
        //                                     * frontLeft.transform.up;
        // Vector3 rearRightAntiRollForce = frontSuspensionDelta
        //                                     * antiRollBarStiffness
        //                                     * frontLeft.transform.up;

        vehicleRb.AddForceAtPosition(-frontAntiRollForce * frontLeft.transform.up, frontLeft.transform.position);
        vehicleRb.AddForceAtPosition(frontAntiRollForce * frontRight.transform.up, frontRight.transform.position);
        vehicleRb.AddForceAtPosition(-rearAntiRollForce * rearLeft.transform.up, rearLeft.transform.position);
        vehicleRb.AddForceAtPosition(rearAntiRollForce * rearRight.transform.up, rearRight.transform.position);
    }
}
