using UnityEngine;

public class CheckWinning : MonoBehaviour
{
    private PauseMenuView menu;
    private void Start()
    {
        menu = FindFirstObjectByType<PauseMenuView>();
        if (menu == null)
            Debug.LogError("No PauseMenuView found in the scene.", this);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (menu != null && other.CompareTag("Player"))
            menu.OpenWinMenu();
    }
}
