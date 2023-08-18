using System.Collections.ObjectModel;
using UnityEngine;

public class AntiRollBars : MonoBehaviour
{
    [SerializeField] private int antiRollBarStiffness;
    [SerializeField] private VehicleController vehicleController;
    [SerializeField] private Wheel frontLeft;
    [SerializeField] private Wheel frontRight;
    [SerializeField] private Wheel rearLeft;
    [SerializeField] private Wheel rearRight;
    [SerializeField] private Rigidbody vehicleRb;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
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
