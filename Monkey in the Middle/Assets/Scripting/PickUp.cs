using UnityEngine;

public class PickUp : MonoBehaviour
{
    [SerializeField] Transform holdAt;
    private Vector3 ogPos;
    public float holdSpeed;
    [SerializeField] Transform holding;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ogPos = holdAt.position;
    }

    // Update is called once per frame
    void Update()
    {
        float step = holdSpeed * Time.deltaTime;
        if(holding != null){
            holding.position = Vector3.MoveTowards(holding.position, holdAt.position, step);
        }
    }

    void Letgo()
    {
        holdAt.position = ogPos;
        holding = null;
    }

}
