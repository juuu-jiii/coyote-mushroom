using UnityEngine;

/// <summary>
/// Gearbox-related calculations and gearshift logic.
/// </summary>
public class Gearbox : MonoBehaviour
{
    [Tooltip("Collection of all gear ratios for this vehicle.")]
    [SerializeField] private float[] gearRatios;
    [Tooltip("Final drive ratio for this vehicle.")]
    [SerializeField] private float finalDrive;
    [Tooltip("Multiplier that affects the gearbox's efficiency. The gearbox consists of cogs and shafts that work together to move the car. This produces resistance in the form of friction, resulting in output torque ultimately being less than that at the input.")]
    [SerializeField][Range(0, 1)] private float efficiency;
    [Tooltip("The amount of time it takes to shift gears.")]
    [SerializeField] private float shiftTime;
    [Tooltip("Names of all the gears for this vehicle.")]
    public char[] gears;

    /// <summary>
    /// The gear this vehicle is currently in.
    /// </summary>
    public int CurrentGear { get; private set; } = 1;

    /// <summary>
    /// The target gear as a result of an up or down shift action.
    /// </summary>
    private int gearTarget;

    /// <summary>
    /// The number of engine rotations per wheel rotation for the current gear.
    /// </summary>
    private float totalGearRatio;

    /// <summary>
    /// Whether the vehicle is in the process of shifting gears.
    /// </summary>
    private bool shifting = false;

    /// <summary>
    /// Sets CurrentGear to gearTarget and calculates totalGearRatio.
    /// </summary>
    private void ShiftGear()
    {
        CurrentGear = gearTarget;

        // The number of engine revolutions per wheel rotation is given by the 
        // current gear ratio multiplied by the final drive ratio.
        totalGearRatio = gearRatios[CurrentGear] * finalDrive;

        // Shift operation is complete.
        shifting = false;
    }

    /// <summary>
    /// Checks to see if a shift action is redundant. This can happen when
    /// either the player upshifts while at the highest gear, or downshifts
    // while at the lowest gear.
    /// </summary>
    /// <returns>
    /// True if a shift action is not redundant. False if it is redundant.
    /// /// </returns>
    private bool CheckShift()
    {
        // Either the player upshifted while at the maximum gear, or downshifted
        // while at the reverse gear. Exit early.
        if (CurrentGear == gearTarget) return false;

        // Gear is in N while in the midst of shifting.
        // Since neutral gear has a ratio of 0, totalGearRatio = 0.
        CurrentGear = 1;
        totalGearRatio = 0;

        return true;
    }

    /// <summary>
    /// Attempts to upshift. Does nothing if the vehicle is in the process of 
    /// shifting gears, or the highest possible gear is already engaged.
    /// </summary>
    public void GearUp()
    {
        // Do not try to shift if an up/downshift action is already in progress.
        if (!shifting)
        {
            // Do not let the gear go below the highest possible one.
            gearTarget = Mathf.Min(CurrentGear + 1, gearRatios.Length - 1);

            // Check to see if a shift action is redundant.
            if (CheckShift())
            {
                // "Delay" the gear shift for shiftTime seconds.
                shifting = true;
                Invoke(nameof(ShiftGear), shiftTime);
            }
        }
    }

    /// <summary>
    /// Attempts to downshift. Does nothing if the vehicle is in the process of 
    /// shifting gears, or the lowest possible gear is already engaged.
    /// </summary>
    public void GearDown()
    {
        // Do not try to shift if an up/downshift action is already in progress.
        if (!shifting)
        {
            // Do not let the gear go below the lowest possible one.
            gearTarget = Mathf.Max(CurrentGear - 1, 0);

            // Check to see if a shift action is redundant.
            if (CheckShift())
            {
                // "Delay" the gear shift for shiftTime seconds.
                shifting = true;
                Invoke(nameof(ShiftGear), shiftTime);
            }
        }
    }
}
