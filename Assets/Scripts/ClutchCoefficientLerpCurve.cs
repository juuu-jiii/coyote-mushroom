using UnityEngine;

public class ClutchCoefficientLerpCurve : MonoBehaviour
{
    [Tooltip("Measures clutch force scale based on the dot product between the vehicle's forward and velocity vectors.")]
    public AnimationCurve curve;
}
