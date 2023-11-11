using UnityEngine;

public class TyreForceCurve : MonoBehaviour
{
    [Tooltip("Measures friction versus slip for a tyre by approximating Pacejka's magic formula.")]
    public AnimationCurve curve;
}
