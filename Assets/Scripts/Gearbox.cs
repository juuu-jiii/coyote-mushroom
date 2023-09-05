using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gearbox : MonoBehaviour
{
    [SerializeField] private float[] gearRatios;
    public char[] gears;
    [SerializeField] private float finalDrive;
    [SerializeField] private float efficiency;
    [SerializeField] private float shiftTime;
    public int CurrentGear { get; private set; } = 1;
    private int gearTarget;
    private float totalGearRatio;
    private bool shifting = false;

    // // Start is called before the first frame update
    // void Start()
    // {

    // }

    // // Update is called once per frame
    // void Update()
    // {

    // }

    private void ShiftGear()
    {
        CurrentGear = gearTarget;

        // The number of engine rotations per wheel rotation is given by the 
        // current gear ratio multiplied by the final drive ratio.
        totalGearRatio = gearRatios[CurrentGear] * finalDrive;

        shifting = false;
    }

    private bool CheckShift()
    {
        // Either the player upshifted while at the maximum gear, or downshifted
        // while at the reverse gear. Exit early.
        if (CurrentGear == gearTarget) return false;

        // Gear is in N while in the midst of shifting.
        CurrentGear = 1;

        totalGearRatio = 0;

        return true;
    }

    public void GearUp()
    {
        if (!shifting)
        {
            gearTarget = Mathf.Min(CurrentGear + 1, gearRatios.Length - 1);
            if (CheckShift())
            {
                shifting = true;
                Invoke(nameof(ShiftGear), shiftTime);
            }
        }
    }

    public void GearDown()
    {
        if (!shifting)
        {
            gearTarget = Mathf.Max(CurrentGear - 1, 0);
            if (CheckShift())
            {
                shifting = true;
                Invoke(nameof(ShiftGear), shiftTime);
            }
        }
    }
}
