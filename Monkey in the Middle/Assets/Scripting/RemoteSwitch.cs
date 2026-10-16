using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class RemoteSwitch : MonoBehaviour
{
    private List<Collider> currentlyColliding = new List<Collider>();

    [SerializeField] private UnityEvent onSwitchTurnedOn;
    [SerializeField] private UnityEvent onSwitchTurnedOff;
    [SerializeField] private List<string> validTagsForCollision;

    /// <summary>
    /// Call target methods when something collides with the switch
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        // only specified objects trigger collision logic
        if (!validTagsForCollision.Contains(other.tag))
        {
            return;
        }
        
        currentlyColliding.Add(other);
        if (currentlyColliding.Count == 1 && onSwitchTurnedOn != null)
        {
            onSwitchTurnedOn.Invoke();
        }
    }

    /// <summary>
    /// Call target methods when all collisions leave
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerExit(Collider other)
    {
        // only specified objects trigger collision logic
        if (!validTagsForCollision.Contains(other.tag))
        {
            return;
        }

        if (currentlyColliding.Contains(other))
        {
            currentlyColliding.Remove(other);
            if (currentlyColliding.Count == 0 && onSwitchTurnedOff != null)
            {
                onSwitchTurnedOff.Invoke();
            }
        }

    }
}
