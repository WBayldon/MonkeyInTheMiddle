using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CheckEscape : MonoBehaviour
{
    private PauseMenuView menu;
    private void Start()
    {
        menu = FindFirstObjectByType<PauseMenuView>();
        if (menu == null)
            Debug.LogError("No PauseMenuView found in the scene.", this);
    }

    private void OnTriggerExit(Collider other)
    {
        if (menu != null && other.CompareTag("Player"))
            menu.OpenWinMenu();
    }
}
