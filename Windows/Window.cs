using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Window : MonoBehaviour
{
    // width & height
    [Header("Width and Height")]
    public float width;
    public float height;

    [SerializeField] private float minWidth = 0.3f;
    [SerializeField] private float minHeight = 0.3f;

    public virtual void scaleWindow()
    {
        if (width < minWidth) width = minWidth;
        if (height < minHeight) height = minHeight;

        Transform cube = transform.GetChild(0);
        cube.localScale = new Vector3(width, height, cube.localScale.z);
    }

    public virtual void close()
    {
        Destroy(this);
    }
}
