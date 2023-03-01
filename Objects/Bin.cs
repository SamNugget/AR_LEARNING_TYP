using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ObjectInstances;

public class Bin : MonoBehaviour, SnapListener
{
    [SerializeField] private SnapLocation snapLocation;

    public void onSnap(Transform snapped)
    {
        ObjectInstance oI = snapped.GetComponent<ObjectInstance>();

        if (oI == null)
        {
            Debug.Log("Err: Snapped object has no ObjectInstance component.");
            return;
        }

        snapLocation.OnTriggerExit(snapped.GetComponent<Collider>());
        Destroy(snapped.gameObject);
    }

    public void onUnsnap()
    {

    }
}
