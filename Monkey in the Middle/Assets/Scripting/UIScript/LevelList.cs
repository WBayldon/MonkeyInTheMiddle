using UnityEngine;

public class LevelList : MonoBehaviour
{
    [SerializeField] private string[] sceneNames;

    public int Count => sceneNames.Length;

    public string Get(int index) =>
        index >= 0 && index < sceneNames.Length ? sceneNames[index] : null;

    /// <summary>
    /// Returns the next scene's name or null if none
    /// </summary>
    /// <param name="previous">The previous scene's name</param>
    public string GetNext(string previous)
    {
        for (int i = 0; i < sceneNames.Length - 1; i++)
            if (sceneNames[i] == previous) return sceneNames[i + 1];
        return null;
    }

    /// <summary>
    /// Returns the first scene's name or null if none
    /// </summary>
    public string GetFirst()
    {
        if (sceneNames.Length > 0) return sceneNames[0];
        else return null;
    }
}