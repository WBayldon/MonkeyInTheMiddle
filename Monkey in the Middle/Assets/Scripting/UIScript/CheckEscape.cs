using System;
using UnityEngine;

public class CheckEscape : MonoBehaviour
{
    [SerializeField] GameObject panel;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {   
        panel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    //<summary>
    //Turns on the "winner" panel once the player leaves the provided area.
    //</summary>
    private void OnTriggerExit(Collider other){
        if(other.CompareTag("Player"))
        {
            panel.SetActive(true);
        }
    }
}
