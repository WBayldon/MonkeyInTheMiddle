using UnityEngine;
using UnityEngine.InputSystem;
public class PickUp : MonoBehaviour
{
    [SerializeField] Transform holdAt;
    private Vector3 ogPos;
    public float holdSpeed;
    private float holdMass = 1;
    [SerializeField] Transform holding;
    LayerMask mask;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mask = LayerMask.GetMask("Interactable");
        ogPos = holdAt.position;
        if(holding != null){
            holdMass = holding.GetComponent<Rigidbody>().mass;
            holding.GetComponent<Rigidbody>().mass = 0.1f;
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
            holding.SetParent(this.transform);
        }
    }

    void Grab(){
        Debug.Log("Grab called.");
        Collider[] hitColliders = Physics.OverlapSphere(holdAt.position, holdAt.GetComponent<Collider>().bounds.size.x /2, mask);
        holding = hitColliders[0].transform;
        holdMass = holding.GetComponent<Rigidbody>().mass;
        holding.GetComponent<Rigidbody>().mass = 0.1f;
    }
    void Letgo()
    {
        Debug.Log("Letgo called.");
        //holdAt.position = ogPos;
        holding.GetComponent<Rigidbody>().mass = holdMass;
      holding.SetParent(null);
        holding = null;
        
    }

}
