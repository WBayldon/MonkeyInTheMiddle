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
        //Debug.Log("Trigger: " + other.name);
        if (other.CompareTag("Player"))
        {
            // Currently we can't make affect the player's horizontal velocity so we can only bounce upwards
            //Vector3 normal = transform.up;
            if (bounceByVelocity)
                other.GetComponent<ThirdPersonController>().Bounce(trampolineSpeed);
            else
                other.GetComponent<ThirdPersonController>().Bounce(bounceByVelocity);
        }
        else if (other.gameObject.layer == 6)
        {
            if (other.TryGetComponent(out Rigidbody rb))
            {
                Vector3 normal = transform.up;
                float downVelocity = rb.linearVelocity.y;

                if (!bounceByVelocity)
                    rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z) + normal * trampolineSpeed;
                else
                    rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z) + normal * downVelocity;
            }
        }
    }
}

