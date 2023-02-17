using UnityEngine;

public abstract class SnapListener : MonoBehaviour
{
    public abstract void onSnap(Transform snapped);
    public abstract void onUnsnap();
}
