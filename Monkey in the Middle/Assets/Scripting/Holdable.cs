using UnityEngine;

public class Holdable : MonoBehaviour
{
    // redundant names in case this class is inherited from
    [Tooltip("This disables player jumping when true")]
    public bool disableJumpingWhenHeld = false;
    [Tooltip("This multiplies the movespeed in vThirdPersonMotor so 0 is no movement, 1 is regular")]
    public float reduceSpeedWhenHeld = 1f;
    [Tooltip("How far away the player should hold this item")]
    public float holdDistance = 1.5f;
}
