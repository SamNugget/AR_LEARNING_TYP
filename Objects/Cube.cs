using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ObjectInstances;

public class Cube : Skin
{
    public int size
    {
        set
        {
            // set size of object
            float s = value / 10f;
            transform.localScale = new Vector3(s, s, s);
        }
    }
}
