using StarterAssets;
using UnityEngine;

public class TrampolineScript : MonoBehaviour
{
    public float trampolineSpeed;
    public bool bounceByVelocity;

    /// <summary>
    /// Handles bounce collisions
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.gameObject.layer == 6)
        {
            if (other.TryGetComponent(out Rigidbody rb))
            {
                Vector3 normal = transform.up;
                float downSpeed = Mathf.Abs(rb.linearVelocity.y);

                if (bounceByVelocity)
                    rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z) + normal * downSpeed;
                else
                    rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z) + normal * trampolineSpeed;
            }
        }
    }
}

