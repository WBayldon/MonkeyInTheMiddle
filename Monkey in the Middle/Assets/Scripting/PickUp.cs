using UnityEngine;
using UnityEngine.InputSystem;
public class PickUp : MonoBehaviour
{
    [SerializeField] Transform holdAt;
    [SerializeField] float holdSpeed;
    [Tooltip("This is in addition to the distance of the held item itself")]
    [SerializeField] float baseHoldDistance;
    [SerializeField] Transform holding;
    // held item and climbing object are static so they can be read in the character controller
    public static Holdable heldItem;
    public static Collider climbingCollider;
    public Holdable ITEM;
    private LayerMask mask;
    public LayerMask grabBlockingLayers;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mask = LayerMask.GetMask("Interactable");
        holding = null;
        heldItem = null;
        climbingCollider = null;
    }

    // Update is called once per frame
    void Update()
    {
        if (GameStateMachine.Instance.Current == GameState.Paused) return;

        ITEM = heldItem;
    }

    // This uses player input actions so we can have multiple bindings easier
    void OnGrab()
    {
        if (GameStateMachine.Instance.Current == GameState.Paused) return;

        if (holding != null || climbingCollider != null)
            Letgo();
        else
            Grab();
    }

    void Grab()
    {
        //Debug.Log("Grab called.");

        Collider[] hitColliders = Physics.OverlapSphere(holdAt.position, holdAt.GetComponent<Collider>().bounds.size.x / 2, mask);
        if (hitColliders.Length > 0)
        {
            Collider col = hitColliders[0];
            bool validCollider = false;
            for (int i = 0; i < hitColliders.Length; i++)
            {
                col = hitColliders[i];

                Vector3 playerCenter = transform.position + Vector3.up / 2;
                Vector3 direction = col.transform.position - playerCenter;
                if (!Physics.Raycast(playerCenter, direction, direction.magnitude + 0.01f, grabBlockingLayers))
                {
                    validCollider = true;
                    break;
                }
            }

            if (!validCollider)
                return;

            // enter climbing state if grabbing climbable object
            if (col.CompareTag("Climbable"))
            {
                climbingCollider = col;
                Rigidbody rb = GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.useGravity = false;
                }
                return;
            }

            // this can potentially be a child collider, so we want the position of the rigidbody instead
            if (col.attachedRigidbody != null)
                holding = col.attachedRigidbody.transform;
            else
                holding = col.transform;

            if (!holding.TryGetComponent(out heldItem))
            {
                heldItem = null;
            }

            float lowestPoint = col.bounds.min.y;
            float lowestDif = col.transform.position.y - lowestPoint;

            Vector3 objectOffset = col.transform.position - col.bounds.center;

            if (heldItem != null) // + Vector3.up since the capsule is offset from the player object
            {
                holding.position = transform.position + transform.forward * (heldItem.holdDistance + baseHoldDistance) + Vector3.up * heldItem.holdHeight;
                holding.position += objectOffset;
            }
            else
                holding.position = holdAt.position;

            holding.transform.position += Vector3.up * lowestDif;

            FixedJoint holdJoint = holding.gameObject.AddComponent<FixedJoint>();
            // the character pivots around the center of mass when connected with a joint,
            // so reduce the held items mass so it doesn't affect the center of mass as much
            holdJoint.connectedMassScale = 0.1f / holding.GetComponent<Rigidbody>().mass;
            holdJoint.connectedBody = GetComponent<Rigidbody>();
        }
    }

    void Letgo()
    {
        //Debug.Log("Letgo called.");

        // let go of climbing object
        if (climbingCollider != null)
        {
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.useGravity = true;
            }
            climbingCollider = null;

            return;
        }

        Destroy(holding.gameObject.GetComponent<FixedJoint>());
        holding = null;
        heldItem = null;
    }

}
