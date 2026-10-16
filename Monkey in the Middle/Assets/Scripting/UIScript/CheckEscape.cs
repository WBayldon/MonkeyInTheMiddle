using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CheckEscape : MonoBehaviour
{
    [SerializeField] GameObject panel;
    [SerializeField] private string nextLevel;
    [SerializeField] private float nextLevelDelay;

    void Start()
    {   
        panel.SetActive(false);
    }

    /// <summary>
    /// Turns on the "winner" panel once the player leaves the provided area.
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerExit(Collider other){
        if(other.CompareTag("Player"))
        {
            panel.SetActive(true);
            StartCoroutine(LoadLevelAfterDelay());
        }
    }

    IEnumerator LoadLevelAfterDelay()
    {
        yield return new WaitForSeconds(nextLevelDelay);
        if (nextLevel != null && nextLevel != "")
            SceneManager.LoadScene(nextLevel);
    }
}
