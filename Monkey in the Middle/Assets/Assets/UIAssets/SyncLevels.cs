using UnityEngine;

public class SyncLevels : MonoBehaviour
{
    [SerializeField] private string[] sceneNames;

    public int Count => sceneNames.Length;

    public string Get(int index) =>
        index >= 0 && index < sceneNames.Length ? sceneNames[index] : null;

    /// <summary>Returns the scene after the given one, or null if there isn't one.</summary>
    public string GetNext(string current)
    {
        for (int i = 0; i < sceneNames.Length - 1; i++)
            if (sceneNames[i] == current) return sceneNames[i + 1];
        return null;
    }

    /// <summary>Returns the first scene in the list, or null if the list is empty.</summary>
    public string GetFirst() => sceneNames.Length > 0 ? sceneNames[0] : null;
}