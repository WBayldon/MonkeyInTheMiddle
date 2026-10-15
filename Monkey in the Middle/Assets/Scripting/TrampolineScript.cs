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

        // the trygetcomponent is necessary because both the monkey and the capsule child are tagged as player.
        // The capsule needs to be tagged as player to not block the camera, but it also passes the tag check so this is the solution.
        if (other.CompareTag("Player") && other.TryGetComponent(out ThirdPersonController controller))
        {
            // Currently we can't make affect the player's horizontal velocity so we can only bounce upwards
            //Vector3 normal = transform.up;
            if (bounceByVelocity)
                controller.Bounce(bounceByVelocity);
            else
                controller.Bounce(trampolineSpeed);
        }
        else if (other.gameObject.layer == 6)
        {
            if (other.TryGetComponent(out Rigidbody rb))
            {
                Vector3 normal = transform.up;
                float downVelocity = rb.linearVelocity.y;

                if (bounceByVelocity)
                    rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z) + normal * downVelocity;
                else
                    rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z) + normal * trampolineSpeed;
            }
        }
    }
}

