using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnapLocation : MonoBehaviour
{
    [SerializeField] private Transform snapPoint;
    [SerializeField] private Transform parentOnSnap;
    [SerializeField] private Transform _snapListener;
    public SnapListener snapListener;



    private void OnTriggerEnter(Collider other)
    {
        GameObject g = other.gameObject;
        Snappable s = g.GetComponent<Snappable>();
        if (s != null && snapped == null)
            StartCoroutine(trySnap(s));
    }

    private void OnTriggerExit(Collider other)
    {
        GameObject g = other.gameObject;
        Snappable s = g.GetComponent<Snappable>();
        if (s == snapped)
            unsnap();
    }



    private Snappable snapped;
    private IEnumerator trySnap(Snappable toSnap)
    {
        snapped = toSnap;

        // wait till drag ended, then snap
        while (snapped.beingDragged == true)
            yield return null;
        snap();
    }

    private void snap()
    {
        // disable snap area glow
        GetComponent<Renderer>().enabled = false;

        // copy rot and pos
        Transform t = snapped.transform;
        t.rotation = snapPoint.rotation;
        t.position = snapPoint.position;
        // change parent (if necessary)
        if (parentOnSnap != null)
            t.parent = parentOnSnap;

        // remove velocity (if necessary)
        Rigidbody r = snapped.GetComponent<Rigidbody>();
        if (r != null)
        {
            r.velocity = Vector3.zero;
            r.angularVelocity = Vector3.zero;
        }

        // tell snap listener about this
        if (snapListener != null)
            snapListener.onSnap(t);
    }

    private void unsnap()
    {
        // enable snap area glow
        GetComponent<Renderer>().enabled = true;

        // tell snap listener about this
        if (snapListener != null)
            snapListener.onUnsnap();

        snapped = null;
        StopAllCoroutines();
    }

    void Start()
    {
        if (_snapListener != null)
        {
            SnapListener sL = _snapListener.GetComponent<SnapListener>();
            if (sL != null) snapListener = sL;
        }
    }
}
