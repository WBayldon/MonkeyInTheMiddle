using UnityEngine;
using UnityEngine.InputSystem;
public class PickUp : MonoBehaviour
{
    [SerializeField] Transform holdAt;
    [SerializeField] float holdSpeed;
    [SerializeField] Transform holding;
    // held item is static so it can be read in the character controller
    public static Holdable heldItem;
    public Holdable ITEM;
    private LayerMask mask;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mask = LayerMask.GetMask("Interactable");
        holding = null;
        heldItem = null;
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

        if (holding != null)
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
            // this can potentially be a child collider, so we want the position of the rigidbody instead
            if (hitColliders[0].attachedRigidbody != null)
                holding = hitColliders[0].attachedRigidbody.transform;
            else
                holding = hitColliders[0].transform;

            if (!holding.TryGetComponent(out heldItem))
            {
                heldItem = null;
            }

            float lowestPoint = hitColliders[0].bounds.min.y;
            float lowestDif = hitColliders[0].transform.position.y - lowestPoint;

            if (heldItem != null) // + Vector3.up since the capsule is offset from the player object
                holding.position = transform.position + transform.forward * heldItem.holdDistance + Vector3.up * heldItem.holdHeight; // + Vector3.up / 2
            else
                holding.position = holdAt.position;

            //holding.Translate(Vector3.up * lowestDif);
            holding.transform.position += Vector3.up * lowestDif;

            // keep the object slightly off the ground
            //holding.transform.position = 
            //holding.Translate(Vector3.up * heldItem.holdHeight);

            // a cube object rotated around any axis other than y changes vertical bounds which causes bad behavior
            // this can be fixed but doesn't really need to be now
            //holding.rotation = Quaternion.Euler(Vector3.Scale(holding.rotation.eulerAngles, Vector3.up));

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

        Destroy(holding.gameObject.GetComponent<FixedJoint>());
        holding = null;
        heldItem = null;
    }

}
