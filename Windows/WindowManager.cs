using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindowManager : MonoBehaviour
{
    private static WindowManager singleton;

    [System.Serializable]
    public class WindowFab
    {
        public string name;
        public GameObject prefab;
    }

    [SerializeField] private List<WindowFab> windowFabs;
    public static GameObject getWindowFab(string name)
    {
        foreach (WindowFab w in singleton.windowFabs)
            if (w.name == name) return w.prefab;

        Debug.LogError("Prefab " + name + " does not exist.");
        return null;
    }





    public static Window spawnWindow(string name)
    {
        GameObject windowFab = getWindowFab(name);
        if (windowFab == null) return null;

        GameObject g = Instantiate(windowFab, singleton.transform);
        return g.GetComponent<Window>();
    }





    public static Window getWindowWithName(string name)
    {
        foreach (Transform child in singleton.transform)
        {
            if (child.name.Contains(name))
                return child.GetComponent<Window>();
        }
        return null;
    }

    public static List<Window> getWindowsWithName(string name)
    {
        List<Window> found = new List<Window>();

        foreach (Transform child in singleton.transform)
        {
            if (child.name.Contains(name))
                found.Add(child.GetComponent<Window>());
        }
        return found;
    }





    void Awake()
    {
        if (singleton == null) singleton = this;
        else Debug.LogError("Two WindowManager singletons.");
    }
}
