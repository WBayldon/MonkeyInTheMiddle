using UnityEngine;

public class DisappeaingDoor : MonoBehaviour
{
    private Collider col;
    private MeshRenderer meshRenderer;

    private void Awake()
    {
        col = GetComponent<Collider>();
        meshRenderer = GetComponent<MeshRenderer>();
    }

    /// <summary>
    /// Makes the door model and collider disappear or reappear
    /// </summary>
    /// <param name="val">true if door should disappear, false if it should reappear</param>
    public void Disappear(bool val)
    {
        col.enabled = !val;
        meshRenderer.enabled = !val;
    }
}
