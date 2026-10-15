using UnityEngine;
using UnityEngine.InputSystem;
public class PickUp : MonoBehaviour
{
    [SerializeField] Transform holdAt;
    private Vector3 ogPos;
    [SerializeField] float holdSpeed;
    private float holdMass = 1;
    [SerializeField] Transform holding;
    LayerMask mask;
    Vector3 holdForceDir;
    float clampedSpeed;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mask = LayerMask.GetMask("Interactable");
        ogPos = holdAt.position;
        if(holding != null){
            holdMass = holding.GetComponent<Rigidbody>().mass;
            //holding.GetComponent<Rigidbody>().mass = 0.1f;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (GameStateMachine.Instance.Current == GameState.Paused) return;
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if(holding != null) Letgo();
            else{
            Grab();
            }
        }
        float step = holdSpeed * Time.deltaTime;
        if(holding != null){
            //holding.position = Vector3.MoveTowards(holding.position, holdAt.position, step);
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
        holding = hitColliders[0].transform;
        holdMass = holding.GetComponent<Rigidbody>().mass;
        holding.SetParent(this.transform);
        holding.GetComponent<Collider>().excludeLayers = LayerMask.GetMask("Player");
        holding.GetComponent<Rigidbody>().excludeLayers = LayerMask.GetMask("Player");
        //holding.GetComponent<Rigidbody>().mass = 0.1f;
    }
    void Letgo()
    {
        Debug.Log("Letgo called.");
        //holdAt.position = ogPos;
        holding.GetComponent<Rigidbody>().mass = holdMass;
      holding.SetParent(null);
      holding.GetComponent<Collider>().includeLayers = LayerMask.GetMask("Player");
        holding.GetComponent<Rigidbody>().includeLayers = LayerMask.GetMask("Player");
        holding = null;
        
    }

}
