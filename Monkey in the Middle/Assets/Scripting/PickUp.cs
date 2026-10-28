using UnityEngine;
using UnityEngine.InputSystem;
public class PickUp : MonoBehaviour
{
    [SerializeField] Transform holdAt;
    [SerializeField] float holdSpeed;
    [SerializeField] Transform holding;
    LayerMask mask;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mask = LayerMask.GetMask("Interactable");
        holding = null;
    }

    // Update is called once per frame
    void Update()
    {
        if (GameStateMachine.Instance.Current == GameState.Paused) return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (holding != null) 
                Letgo();
            else
                Grab();
        }
    }

    void Grab()
    {
        //Debug.Log("Grab called.");

        Collider[] hitColliders = Physics.OverlapSphere(holdAt.position, holdAt.GetComponent<Collider>().bounds.size.x / 2, mask);
        if (hitColliders.Length > 0)
        {
            holding = hitColliders[0].transform;
            holding.position = holdAt.position; 

            // keep the object slightly off the ground
            holding.Translate(Vector3.up * 0.05f);

            // a cube object rotated around any axis other than y changes vertical bounds which causes bad behavior
            // this can be fixed but doesn't really need to be now
            holding.rotation = Quaternion.Euler(Vector3.Scale(holding.rotation.eulerAngles, Vector3.up));

            FixedJoint holdJoint = holding.gameObject.AddComponent<FixedJoint>();
            // the character pivots around the center of mass when connected with a joint,
            // so reduce the held items mass so it doesn't affect the center of mass as much
            holdJoint.connectedMassScale = 0.1f;
            holdJoint.connectedBody = GetComponent<Rigidbody>();
        }
    }

    void Letgo()
    {
        //Debug.Log("Letgo called.");

        Destroy(holding.gameObject.GetComponent<FixedJoint>());
        holding = null;
    }

}
