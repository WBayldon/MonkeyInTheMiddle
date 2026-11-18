using UnityEngine;
using UnityEngine.InputSystem;
public class OldPickUp : MonoBehaviour
{
    [SerializeField] Transform holdAt;
    private Vector3 ogPos;
    [SerializeField] float holdSpeed;
    private float holdMass = 1;
    [SerializeField] Transform holding;
    LayerMask mask;
    Vector3 holdForceDir;
    float clampedSpeed;
    Rigidbody rb;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mask = LayerMask.GetMask("Interactable");
        ogPos = holdAt.position;
        if(holding != null){

            holdMass = rb.mass;
            //holding.GetComponent<Rigidbody>().mass = 0.1f;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(Mouse.current.leftButton.wasPressedThisFrame)
        {
            if(holding != null) Letgo();
            else{
            Grab();
            }
        }
        float step = holdSpeed * Time.deltaTime;
        if(holding != null){
            holding.position = Vector3.MoveTowards(holding.position, holdAt.position, step);
            holdForceDir = Vector3.Normalize(holdAt.position - holding.position);
            if(0.2 < (Vector3.Distance(holdAt.position, holding.position))){
                holding.GetComponent<Rigidbody>().AddForce(holdForceDir * holdSpeed);
            }
            
            holding.eulerAngles = this.transform.eulerAngles;
        }
    }

    void Grab(){
        Debug.Log("Grab called.");
        Collider[] hitColliders = Physics.OverlapSphere(holdAt.position, holdAt.GetComponent<Collider>().bounds.size.x /2, mask);
        Collider col = hitColliders[0];
            // this can potentially be a child collider, so we want the position of the rigidbody instead
        if (col.attachedRigidbody != null)
        {
            holding = col.attachedRigidbody.transform;
            rb = col.attachedRigidbody;
        }
        else{
            holding = col.transform;
            rb = holding.GetComponent<Rigidbody>();
        }
            
        
        holdMass = rb.mass;
        //holding.SetParent(this.transform);
       col.excludeLayers = LayerMask.GetMask("Player");
        //rb.excludeLayers = LayerMask.GetMask("Player");
        //holding.GetComponent<Rigidbody>().mass = 0.1f;
    }
    void Letgo()
    {
        Debug.Log("Letgo called.");
        //holdAt.position = ogPos;
        rb.mass = holdMass;
      //holding.SetParent(null);
      //holding.GetComponent<Collider>().includeLayers = LayerMask.GetMask("Player");
        //rb.includeLayers = LayerMask.GetMask("Player");
        holding = null;
        
    }

}
