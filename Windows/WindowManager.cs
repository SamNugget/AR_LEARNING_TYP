using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FileManagement; // temp
using System.Reflection;

public class WindowManager : MonoBehaviour
{
    private static WindowManager singleton;

    [SerializeField] private GameObject _buttonFab;
    public static GameObject buttonFab { get { return singleton._buttonFab; } }

    [SerializeField] private Vector3 spawnPos;

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
        g.transform.position = singleton.spawnPos;
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





    public static List<Block> getMasterBlocks()
    {
        List<Block> masterBlocks = new List<Block>();

        Window[] windows = singleton.GetComponentsInChildren<Window>();
        foreach (Window w in windows)
        {
            foreach (Transform child in w.transform)
            {
                Block b = child.GetComponent<Block>();
                if (b != null) masterBlocks.Add(b);
            }
        }

        return masterBlocks;
    }

    public static void updateEditWindowColliders(bool enabled, List<string> mask = null, bool invert = false)
    {
        List<Block> masterBlocks = getMasterBlocks();
        foreach (Block m in masterBlocks)    
            m.setColliderEnabled(enabled, mask, invert);
    }

    public static void updateEditWindowSpecialBlocks(int variantIndex, bool enabled) // e.g., for insert line
    {
        List<Block> masterBlocks = getMasterBlocks();
        foreach (Block m in masterBlocks)
            m.setSpecialChildBlock(variantIndex, enabled);
    }

    public static void enableEditWindowLeafBlocks() // for deletion of blocks
    {
        List<Block> masterBlocks = getMasterBlocks();
        foreach (Block m in masterBlocks)
            m.enableLeafBlocks(true);
    }





    void Awake()
    {
        if (singleton == null) singleton = this;
        else Debug.LogError("Two WindowManager singletons.");

        // temp
        FileManager.deleteWorkspace("only_workspace");
        FileManager.loadWorkspace("only_workspace");

        //Debug.Log(Assembly.GetAssembly(this.GetType()).Location);
    }
}
